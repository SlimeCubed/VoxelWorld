#pragma once

#include <memory>
#include "shared.h"
#include "logging.h"

// A VoxelMap stores the LZ4 chunk data the render thread needs to upload to the GPU.

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

// Allocate a VoxelMap. This does not allocate the LZ4 data.
EXPORT_API VoxelMapData* VoxelMapAllocate(const wchar_t* name);

// Free a VoxelMap. This frees all allocated chunks.
// It is safe to call this when there may still be queued upload tasks for the chunk (via QueueVoxelUpload).
EXPORT_API void VoxelMapFree(VoxelMapData* ptr);

// Allocate VoxelMap chunk storage for a given amount of chunks.
// CountLZ4Chunks, XVoxels and YVoxels must be fileld in by the C# side before calling this.
EXPORT_API void VoxelMapInit(VoxelMapData* ptr);

// Allocate storage for a specific chunk.
// The C# side is expected to fill this by indexing the LZ4Chunks property.
EXPORT_API void VoxelMapAllocChunk(VoxelMapData* ptr, i32 chunk, i32 length);

i32 XChunks(VoxelMapData* data);

i32 YChunks(VoxelMapData* data);

void WaitForVoxels(VoxelMapData* data);

void DecompressChunk(const u8* lz4Data, i32 lz4DataLength, u8* buffer, i32 xVoxels, i32 yVoxels, i32 chunkX, i32 chunkY);

void GetVoxels(VoxelMapData* data, u8* buffer, i32 chunkX, i32 chunkY);
