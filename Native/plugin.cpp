#define WIN32_LEAN_AND_MEAN

#include <Windows.h>
#include <d3d11.h>
#include <stdint.h>
#include <mutex>
#include <queue>
#include <memory>
#include "UnityPluginInterface.h"
#include "lz4.h"
#include "shared.h"
#include "plugin.h"

#include "logging.h"
#include "voxel_map.h"

struct VoxelWorldNativeChunkUpload
{
	ID3D11Texture3D *Texture;
	std::shared_ptr<VoxelMap> Map;
	i32 ChunkX;
	i32 ChunkY;

	VoxelWorldNativeChunkUpload(ID3D11Texture3D *texture, std::shared_ptr<VoxelMap> map, i32 chunkX, i32 chunkY)
		: Texture(texture), Map(map), ChunkX(chunkX), ChunkY(chunkY)
	{
	}
};

static std::mutex cmd_mutex;
static std::queue<VoxelWorldNativeChunkUpload> cmd_chunk_upload_queue;

static ID3D11Texture3D **staging_pool;
static i32 staging_pool_index;

static ID3D11Device *_device;
static ID3D11DeviceContext *_deviceContext;

#define CheckHResult(op) _CheckHResult(op, __FILE__, __LINE__);

void _CheckHResult(HRESULT result, const char *file, i32 line)
{
	if (FAILED(result)) {
		char buf[256];
		snprintf(buf, 255, "Operation failed: 0x%X at %s:%d", result, file, line);
		MessageBoxA(NULL, buf, "Voxel World: error", MB_ICONERROR | MB_OK);
		abort();
	}
}

EXPORT_API void UnitySetGraphicsDevice(void *device, int deviceType, int eventType)
{
	if (eventType != kGfxDeviceEventInitialize)
		return;

	_device = (ID3D11Device*)device;
	_device->AddRef();
}

EXPORT_API void Init(VoxelWorldNativePreferences *prefs)
{
	preferences = *prefs;
}

EXPORT_API void Shutdown()
{
	_device->Release();
	_device = nullptr;

	_deviceContext->Release();
	_deviceContext = nullptr;
}

static void GetD3D11Device()
{
	_device->GetImmediateContext(&_deviceContext);

	D3D_FEATURE_LEVEL fl = _device->GetFeatureLevel();
	LogThreaded(string_format(L"D3D11 FL: %X", fl));

	D3D11_FEATURE_DATA_THREADING threading;
	CheckHResult(_device->CheckFeatureSupport(D3D11_FEATURE_THREADING, &threading, sizeof(D3D11_FEATURE_DATA_THREADING)));

	LogThreaded(string_format(L"D3D11 threading supported? Creates: %d, command lists: %d", threading.DriverConcurrentCreates, threading.DriverCommandLists));

	UINT flags = _device->GetCreationFlags();
	LogThreaded(string_format(L"D3D11 creation flags %X", flags));
}

static void InitUploadBuffers()
{
	LogThreaded(string_format(L"Initializing %d upload buffers", preferences.UploadPoolSize));

	staging_pool = new ID3D11Texture3D*[preferences.UploadPoolSize]();

	for (i32 i = 0; i < preferences.UploadPoolSize; i++)
	{
		D3D11_TEXTURE3D_DESC desc = {};
		desc.Width = preferences.ChunkSize;
		desc.Height = preferences.ChunkSize;
		desc.Depth = preferences.ChunkDepth;
		desc.Usage = D3D11_USAGE_STAGING;
		desc.Format = DXGI_FORMAT_A8_UNORM;
		desc.MipLevels = 0;
		desc.BindFlags = 0;
		desc.MiscFlags = 0;
		desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE | D3D11_CPU_ACCESS_READ;

		CheckHResult(_device->CreateTexture3D(&desc, nullptr, &staging_pool[i]));
	}
}

static void RenderThreadInit()
{
	LogThreaded(L"Initializing render thread!");
	LogThreaded(string_format(L"Render thread ID: 0x%p", GetThreadId(GetCurrentThread())));

	GetD3D11Device();
	InitUploadBuffers();
}

static void RenderThreadShutdown()
{
	if (!staging_pool)
		return;

	for (i32 i = 0; i < preferences.UploadPoolSize; i++)
	{
		auto texture = staging_pool[i];
		// Why could this be null? Who the hell knows!
		if (!texture)
			continue;

		texture->Release();
	}

	delete[] staging_pool;
}

static ID3D11Texture3D *GetNextPoolTexture()
{
	auto tex = staging_pool[staging_pool_index];
	LogThreaded(string_format(L"Handing out pool texture: %d", staging_pool_index));

	staging_pool_index = (staging_pool_index + 1) % preferences.UploadPoolSize;
	return tex;
}

static void RenderThreadDoChunkUpload()
{
	// This language is fucking deranged.
	VoxelWorldNativeChunkUpload cmd = []{
		std::lock_guard<std::mutex> guard(cmd_mutex);
		VoxelWorldNativeChunkUpload cmd = cmd_chunk_upload_queue.front();
		cmd_chunk_upload_queue.pop();
		return cmd;
	}();


	// TODO: profiling logic.

	auto poolTex = (ID3D11Resource*) GetNextPoolTexture();
	D3D11_MAPPED_SUBRESOURCE mapped;

	auto res = _deviceContext->Map(
		poolTex,
		0,
		D3D11_MAP_WRITE,
		D3D11_MAP_FLAG_DO_NOT_WAIT,
		&mapped);

	if (res == DXGI_ERROR_WAS_STILL_DRAWING)
	{
		LogThreaded(L"Have to wait on texture map!!");
		res = _deviceContext->Map(
			poolTex,
			0,
			D3D11_MAP_READ_WRITE,
			0,
			&mapped);
	}

	CheckHResult(res);

	auto dst = (u8*) mapped.pData;

	GetVoxels(&cmd.Map->Data, dst, cmd.ChunkX, cmd.ChunkY);

	_deviceContext->Unmap(poolTex, 0);
	D3D11_BOX srcBox{};
	srcBox.left = 0;
	srcBox.top = 0;
	srcBox.front = 0;
	srcBox.right = preferences.ChunkSize;
	srcBox.bottom = preferences.ChunkSize;
	srcBox.back = preferences.ChunkDepth;
	_deviceContext->CopySubresourceRegion(
		cmd.Texture, 0,
		0, 0, 0,
		poolTex, 0,
		&srcBox);
}

EXPORT_API void QueueVoxelUpload(ID3D11Texture3D* texture, VoxelMapData* map, int chunkX, int chunkY)
{
	std::lock_guard<std::mutex> guard(cmd_mutex);
	cmd_chunk_upload_queue.emplace(texture, *map->PtrPtr, chunkX, chunkY);
}

EXPORT_API void UnityRenderEvent(int eventID)
{
	switch (eventID) {
		case evInit:
			RenderThreadInit();
			break;

		case evChunkUpload:
			RenderThreadDoChunkUpload();
			break;

		case evShutdown:
			RenderThreadShutdown();
			break;
	}
}

EXPORT_API void QueryD3D11Device(DxgiDeviceQuery* query)
{
	*query = {};

	IDXGIDevice* dxgiObject;
	_device->QueryInterface(__uuidof(IDXGIDevice), (void**)&dxgiObject);
	IDXGIAdapter* dxgiAdapter;
	dxgiObject->GetAdapter(&dxgiAdapter);
	dxgiObject->Release();

	dxgiAdapter->GetDesc(&query->Desc);

	// VRAM info is only available with DXGI 1.4 through IDXGIAdapter3.

	IDXGIAdapter3* dxgiAdapter3;
	dxgiAdapter->QueryInterface(__uuidof(IDXGIAdapter3), (void**)&dxgiAdapter3);
	dxgiAdapter->Release();

	if (!dxgiAdapter3)
		return;

	query->VideoMemoryFetched = 1;
	dxgiAdapter3->QueryVideoMemoryInfo(0, DXGI_MEMORY_SEGMENT_GROUP_LOCAL, &query->VideoMemoryLocal);
	dxgiAdapter3->QueryVideoMemoryInfo(0, DXGI_MEMORY_SEGMENT_GROUP_NON_LOCAL, &query->VideoMemoryNonLocal);

	dxgiAdapter3->Release();
}
