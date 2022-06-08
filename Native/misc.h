#pragma once

#include "shared.h"

EXPORT_API void VoxelMapMemcpy(void* dst, const void* src, size_t count);
EXPORT_API void LZ4Decompress(const u8 *src, u8 *dst, i32 compressedSize, i32 dstCapacity);
