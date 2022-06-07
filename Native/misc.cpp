#include "shared.h"
#include <cstring>

EXPORT_API void VoxelMapMemcpy(void* dst, const void* src, size_t count)
{
	memcpy(dst, src, count);
}
