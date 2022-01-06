using System;
using System.Diagnostics;

namespace VoxelWorld
{
    internal struct Profiler : IDisposable
    {
        private readonly string name;
        private readonly Stopwatch stopwatch;

        public Profiler(string name)
        {
            this.name = name;
            stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (stopwatch == null) return;

            stopwatch.Stop();
            Diag.Log($"Profiled {name}: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
