#pragma once

struct VoxelWorldNativePreferences
{
	i32 UploadPoolSize;
	i32 ChunkSize;
	i32 ChunkDepth;
};

VoxelWorldNativePreferences preferences;

EXPORT_API void LZ4Decompress(const u8 *src, u8 *dst, i32 compressedSize, i32 dstCapacity);
