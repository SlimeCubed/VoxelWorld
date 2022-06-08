using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace VoxelWorld;

public static class Helpers
{
    public static long Micros(TimeSpan span)
    {
        return span.Ticks / 10;
    }

    public static TimeSpan RestartStopwatch(Stopwatch stopwatch)
    {
        var value = stopwatch.Elapsed;
        stopwatch.Reset();
        stopwatch.Start();
        return value;
    }
}