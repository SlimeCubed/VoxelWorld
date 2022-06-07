// _snwprintf is "insecure" for some ridiculous reason. What. It literally takes a buffer length??
#define _CRT_SECURE_NO_WARNINGS
#define WIN32_LEAN_AND_MEAN

// RW is 32-bit, so...
static_assert(sizeof(void*) == 4, "Not compiling for 32-bit. Did you use the wrong compiler?");

// Pretty simple Unity build.
#include "logging.cpp"
#include "voxel_map.cpp"
#include "plugin.cpp"
#include "misc.cpp"
