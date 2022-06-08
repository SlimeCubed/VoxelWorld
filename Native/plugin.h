#pragma once

#include <d3d11.h>
#include <dxgi1_4.h>
#include "voxel_map.h"

// Render events that the game passes to the render thread through GL.IssuePluginEvent().
enum PluginEvents
{
	evInit = 0,
	evChunkUpload,
	evShutdown = -1
};

// Configuration preferences C# provides to us, so they are only set in the C# side and not duplicated.
struct VoxelWorldNativePreferences
{
	i32 UploadPoolSize;
	i32 ChunkSize;
	i32 ChunkDepth;
};

// Actual variable storing preferences.
VoxelWorldNativePreferences preferences;

struct DxgiDeviceQuery
{
	DXGI_ADAPTER_DESC Desc;
	u8 VideoMemoryFetched;
	DXGI_QUERY_VIDEO_MEMORY_INFO VideoMemoryLocal;
	DXGI_QUERY_VIDEO_MEMORY_INFO VideoMemoryNonLocal;
};

// Unity render plugin API.
EXPORT_API void UnitySetGraphicsDevice(void *device, int deviceType, int eventType);
EXPORT_API void UnityRenderEvent(int eventID);

// Plugin lifecycle.
EXPORT_API void Init(VoxelWorldNativePreferences *prefs);
EXPORT_API void Shutdown();

// Query device name and VRAM info.
// For the "vram" debug command in-game.
EXPORT_API void QueryD3D11Device(DxgiDeviceQuery* query);

// Render commands.
// These render commands have to be followed by their according UnityRenderEvent
// so the render thread can handle them.
EXPORT_API void QueueVoxelUpload(ID3D11Texture3D* texture, VoxelMapData* map, int chunkX, int chunkY);
