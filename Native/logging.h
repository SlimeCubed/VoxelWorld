#pragma once

#include <string>
#include "shared.h"

EXPORT_API const wchar_t* LogFetch();

// https://stackoverflow.com/a/26221725/4678631
template<typename ... Args>
std::wstring string_format( const std::wstring& format, Args ... args )
{
	int size_s = _snwprintf(nullptr, 0, format.c_str(), args ...) + 1; // Extra space for '\0'
	if (size_s <= 0)
		throw std::runtime_error("Error during formatting.");

	size_t size = (size_t) size_s;
	std::unique_ptr<wchar_t[]> buf(new wchar_t[size]);
	_snwprintf(buf.get(), size, format.c_str(), args ...);
	return std::wstring(buf.get(), buf.get() + size - 1); // We don't want the '\0' inside
}

void LogThreaded(std::wstring msg);
