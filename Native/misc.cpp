#include <cstring>
#include "lz4.h"

#include "misc.h"

EXPORT_API void VoxelMapMemcpy(void* dst, const void* src, size_t count)
{
	memcpy(dst, src, count);
}

EXPORT_API void LZ4Decompress(const u8 *src, u8 *dst, i32 compressedSize, i32 dstCapacity)
{
	LZ4_decompress_safe((const char*) src, (char*) dst, compressedSize, dstCapacity);
}
