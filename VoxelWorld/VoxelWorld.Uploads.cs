using System.Threading;
using UnityEngine;

namespace VoxelWorld;

public unsafe partial class VoxelWorld
{
    private static void StartRenderThread()
    {
        LogThreaded($"[VoxelWorld] Main thread: {Thread.CurrentThread.ManagedThreadId:x}");

        VoxelWorldNativePreferences nativePrefs = default;
        nativePrefs.UploadPoolSize = Preferences.uploadPoolSize;
        nativePrefs.ChunkSize = Preferences.chunkSize;
        nativePrefs.ChunkDepth = Preferences.chunkDepth;
        
        Init(&nativePrefs);

        GL.IssuePluginEvent((int)PluginEvents.Init);
    }

    private static void ShutdownRenderThread()
    {
        Shutdown();
        
        // No idea whether the render thread will even still process this.
        GL.IssuePluginEvent((int)PluginEvents.Shutdown);
    }

    internal static void DoVoxelChunkUpload(VoxelMapView.VoxelChunk chunk)
    {
        // Create texture
        chunk.Texture = new Texture3D(
            128,
            128,
            32,
            TextureFormat.Alpha8,
            mipmap: false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            anisoLevel = 0,
            name = $"Voxel ({chunk.X}, {chunk.Y})"
        };

        {
            var bounds = new Bounds();
            var bMin = new Vector3(0, 0, 0);
            var bMax = new Vector3(128, 128, 32);
            var scl = new Vector3(1f / Preferences.chunkSize, 1f / Preferences.chunkSize, 1f / Preferences.chunkDepth);
            bMin.Scale(scl);
            bMax.Scale(scl);
            bounds.SetMinMax(bMin, bMax);
            chunk.VoxelBounds = bounds;
        }

        chunk.Map.CheckValid();
        QueueVoxelUpload(
            chunk.Texture.GetNativeTexturePtr(),
            chunk.Map.Data,
            chunk.X, chunk.Y);

        GL.IssuePluginEvent((int)PluginEvents.ChunkUpload);
    }
}