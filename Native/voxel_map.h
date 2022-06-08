#pragma once

#include <memory>
#include "shared.h"
#include "logging.h"

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

EXPORT_API VoxelMapData* VoxelMapAllocate(const wchar_t* name);

EXPORT_API void VoxelMapFree(VoxelMapData* ptr);

EXPORT_API void VoxelMapAllocChunk(VoxelMapData* ptr, i32 chunk, i32 length);

EXPORT_API void VoxelMapInit(VoxelMapData* ptr);

i32 XChunks(VoxelMapData* data);

i32 YChunks(VoxelMapData* data);

void WaitForVoxels(VoxelMapData* data);

void DecompressChunk(const u8* lz4Data, i32 lz4DataLength, u8* buffer, i32 xVoxels, i32 yVoxels, i32 chunkX, i32 chunkY);

void GetVoxels(VoxelMapData* data, u8* buffer, i32 chunkX, i32 chunkY);
