#include <cassert>
#include <algorithm>
#include <windows.h>
#include "shared.h"
#include "voxel_map.h"
#include "misc.h"
#include "plugin.h"

EXPORT_API VoxelMapData* VoxelMapAllocate(const wchar_t* name)
{
	auto sharedPtr = new std::shared_ptr<VoxelMap>(std::make_shared<VoxelMap>(name));
	auto data = &(*sharedPtr)->Data;
	data->PtrPtr = sharedPtr;
	return data;
}

EXPORT_API void VoxelMapFree(VoxelMapData* ptr)
{
	delete ptr->PtrPtr;
}

EXPORT_API void VoxelMapAllocChunk(VoxelMapData* ptr, i32 chunk, i32 length)
{
	assert(ptr->LZ4Chunks[chunk] == nullptr);

	ptr->LZ4Chunks[chunk] = new u8[length];
	ptr->LZ4ChunkLengths[chunk] = length;
}

EXPORT_API void VoxelMapInit(VoxelMapData* ptr)
{
	ptr->LZ4Chunks = new u8*[ptr->CountLZ4Chunks];
	ptr->LZ4ChunkLengths = new i32[ptr->CountLZ4Chunks];
}

VoxelMap::VoxelMap(const wchar_t* name) : Name(name), Data{}
{
	LogThreaded(string_format(L"Allocating VoxelMap: %s", name));
}

VoxelMap::~VoxelMap()
{
	LogThreaded(string_format(
		L"Freeing VoxelMap: %s. %d LZ4Chunks",
		Name.c_str(),
		Data.CountLZ4Chunks));

	for (i32 i = 0; i < Data.CountLZ4Chunks; i++)
	{
		delete[] Data.LZ4Chunks[i];
	}

	delete[] Data.LZ4Chunks;
	delete[] Data.LZ4ChunkLengths;

	Data.LZ4Chunks = nullptr;
	Data.LZ4ChunkLengths = nullptr;
}

void WaitForVoxels(VoxelMapData* data)
{
	while (data->VoxelsLoaded == 0)
	{
		Sleep(0);
	}
}

void DecompressChunk(const u8* lz4Data, i32 lz4DataLength, u8* buffer, i32 xVoxels, i32 yVoxels, i32 chunkX, i32 chunkY)
{
	i32 width = min(xVoxels - chunkX * preferences.ChunkSize, preferences.ChunkSize);
	i32 height = min(yVoxels - chunkY * preferences.ChunkSize, preferences.ChunkSize);
	i32 depth = 30;

	LZ4Decompress(lz4Data, buffer, lz4DataLength, width * height * depth);
}

void GetVoxels(VoxelMapData* data, u8* buffer, i32 chunkX, i32 chunkY)
{
	WaitForVoxels(data);

	i32 chunkIdx = chunkX + chunkY * XChunks(data);
	DecompressChunk(
		data->LZ4Chunks[chunkIdx], data->LZ4ChunkLengths[chunkIdx],
		buffer,
		data->XVoxels, data->YVoxels,
		chunkX, chunkY);
}

i32 XChunks(VoxelMapData* data)
{
	return (data->XVoxels + preferences.ChunkSize - 1) / preferences.ChunkSize;
}

i32 YChunks(VoxelMapData* data)
{
	return (data->YVoxels + preferences.ChunkSize - 1) / preferences.ChunkSize;
}
