
#include <memory>
#include "shared.h"
#include "logging.h"
#include "plugin.h"

// We track the VoxelMaps behind a shared_ptr,
// so that upload cmds from the render thread can keep them alive.
// Just in case. It is asynchronous, after all.

struct VoxelMap;

struct VoxelMapData
{
	u8** LZ4Chunks;
	i32* LZ4ChunkLengths;
	int CountLZ4Chunks;
	int XVoxels;
	int YVoxels;
	u8 VoxelsLoaded;


	std::shared_ptr<VoxelMap>* PtrPtr;
};

struct VoxelMap
{
	VoxelMapData Data;
	std::wstring Name;

	VoxelMap(const wchar_t *name);
	~VoxelMap();
};

i32 XChunks(VoxelMapData* data)
{
	return (data->XVoxels + preferences.ChunkSize - 1) / preferences.ChunkSize;
}

i32 YChunks(VoxelMapData* data)
{
	return (data->YVoxels + preferences.ChunkSize - 1) / preferences.ChunkSize;
}

void WaitForVoxels(VoxelMapData* data);

void DecompressChunk(const u8* lz4Data, i32 lz4DataLength, u8* buffer, i32 xVoxels, i32 yVoxels, i32 chunkX, i32 chunkY);

void GetVoxels(VoxelMapData* data, u8* buffer, i32 chunkX, i32 chunkY);
