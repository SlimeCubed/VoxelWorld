#include <string>
#include <mutex>
#include <queue>
#include "logging.h"

static std::mutex log_mutex;
static std::queue<std::wstring> log_queue;
static std::wstring cur_log;

EXPORT_API const wchar_t* LogFetch()
{
	std::lock_guard<std::mutex> guard(log_mutex);

	if (log_queue.empty())
	{
		cur_log = {};
		return nullptr;
	}

	cur_log = log_queue.front();
	log_queue.pop();
	return cur_log.c_str();
}

void LogThreaded(std::wstring msg)
{
	std::lock_guard<std::mutex> guard(log_mutex);

	log_queue.push(msg);
}
