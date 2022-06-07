#include <Windows.h>
#include <d3d11.h>
#include <stdint.h>
#include "UnityPluginInterface.h"
#include "lz4.h"

typedef int32_t i32;
typedef uint8_t u8;

typedef struct _MonoThread MonoThread;
typedef struct _MonoDomain MonoDomain;


enum PluginEvents
{
	evInit = 0,
	evChunkUpload,
	evDetach = -1
};

typedef void (__stdcall FRenderEventCallback)(int eventId);
typedef MonoThread*(FMonoThreadAttach)(MonoDomain* domain);
typedef void(FMonoThreadDetach)(MonoThread* thread);
typedef MonoDomain*(FMonoGetRootDomain)(void);

static HMODULE _monoModule;
static FMonoThreadAttach *_monoThreadAttach;
static FMonoThreadDetach *_monoThreadDetach;
static FMonoGetRootDomain *_monoGetRootDomain;
static ID3D11Device *_device;
static FRenderEventCallback *_renderEventCallback;
static MonoThread *_monoThread;

void EXPORT_API UnitySetGraphicsDevice(void *device, int deviceType, int eventType)
{
	if (eventType != kGfxDeviceEventInitialize)
		return;

	_device = (ID3D11Device*)device;

	_monoModule = LoadLibraryA("mono.dll");
	_monoThreadAttach = (FMonoThreadAttach*) GetProcAddress(_monoModule, "mono_thread_attach");
	_monoThreadDetach = (FMonoThreadDetach*) GetProcAddress(_monoModule, "mono_thread_detach");
	_monoGetRootDomain = (FMonoGetRootDomain*) GetProcAddress(_monoModule, "mono_get_root_domain");
}

void EXPORT_API UnityRenderEvent(int eventID)
{
	if (eventID == evInit)
	{
		_monoThread = _monoThreadAttach(_monoGetRootDomain());
	}

	if (_renderEventCallback)
		_renderEventCallback(eventID);
}

ID3D11Device EXPORT_API *Init(FRenderEventCallback *callback)
{
	_renderEventCallback = callback;
	return _device;
}

void EXPORT_API Detach(void)
{
	_monoThreadDetach(_monoThread);
}

void EXPORT_API CopyVoxelsToTex(
	u8 *dst, u8 *src,
	i32 w, i32 h, i32 d,
	i32 xMin, i32 xMax,
	i32 yMin, i32 yMax,
	i32 zMin, i32 zMax,
	i32 step)
{
	i32 xDelta = xMax - xMin;
	i32 yDelta = yMax - yMin;
	i32 zDelta = zMax - zMin;

	memcpy(dst, src, xDelta*yDelta*min(zDelta, d));
}

void EXPORT_API LZ4Decompress(const u8 *src, u8 *dst, i32 compressedSize, i32 dstCapacity)
{
	LZ4_decompress_safe((const char*) src, (char*) dst, compressedSize, dstCapacity);
}