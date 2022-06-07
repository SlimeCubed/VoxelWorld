using System.Threading;
using TerraFX.Interop.DirectX;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace VoxelWorld;

public unsafe partial class VoxelWorld
{
    public static ID3D11Device* D3DDevice;
    public static ID3D11DeviceContext* D3DImmediate;
    private static ID3D11Texture3D*[] _stagingPool;
    private static int _poolIndex;

    private static void StartRenderThread()
    {
        LogThreaded($"[VoxelWorld] Main thread: {Thread.CurrentThread.ManagedThreadId:x}");

        VoxelWorldNativePreferences nativePrefs = default;
        nativePrefs.UploadPoolSize = Preferences.uploadPoolSize;
        nativePrefs.ChunkSize = Preferences.chunkSize;
        nativePrefs.ChunkDepth = Preferences.chunkDepth;
        
        D3DDevice = Init(&nativePrefs);

        // fixed (char* p = "SU_A22".ToCharArray())
        /*{
            var ptr = VoxelMapAllocate("SU_A22");
            VoxelMapFree(ptr);
        }*/
        
        GL.IssuePluginEvent((int)PluginEvents.Init);
    }

    private static void ShutdownRenderThread()
    {
        //Debug.Log("ASDASDASD");
        //Detach();
    }

    public static ID3D11Texture3D* GetNextPoolTexture()
    {
        var tex = _stagingPool[_poolIndex];
        Debug.Log($"Handing out pool texture: {_poolIndex}");
        _poolIndex = (_poolIndex + 1) % _stagingPool.Length;
        return tex;
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
            (ID3D11Texture3D*)chunk.Texture.GetNativeTexturePtr(),
            chunk.Map.Data,
            chunk.X, chunk.Y);

        GL.IssuePluginEvent((int)PluginEvents.ChunkUpload);
    }

    /*private static void RenderThreadDoChunkUpload()
    {
        var sw2 = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();

        CmdVoxelChunkUpload cmd;
        lock (VoxelUploadQueue)
        {
            cmd = VoxelUploadQueue.Dequeue();
        }
        
        var context = VoxelWorld.D3DImmediate;
        var poolTex = (ID3D11Resource*)VoxelWorld.GetNextPoolTexture();
        D3D11_MAPPED_SUBRESOURCE mapped;
        var res = context->Map(
            poolTex,
            0,
            D3D11_MAP_WRITE,
            (uint)D3D11_MAP_FLAG_DO_NOT_WAIT,
            &mapped);

        if (res == DXGI_ERROR_WAS_STILL_DRAWING)
        {
            LogThreaded("Have to wait on texture map!!");
            res = context->Map(
                poolTex,
                0,
                D3D11_MAP_READ_WRITE,
                0,
                &mapped);
        }

        ThrowIfFailed(res);

        var tMap = RestartStopwatch(sw);

        var dst = (byte*)mapped.pData;

        cmd.Chunk.Map.GetVoxels(dst, cmd.Chunk.X, cmd.Chunk.Y, out _, out _, out _);

        var tCopy = RestartStopwatch(sw);

        context->Unmap(poolTex, 0);
        var srcBox = new D3D11_BOX(0, 0, 0, Preferences.chunkSize, Preferences.chunkSize, Preferences.chunkDepth);
        context->CopySubresourceRegion(
            (ID3D11Resource*)cmd.Texture, 0,
            0, 0, 0,
            poolTex, 0,
            &srcBox);

        var tSubmit = RestartStopwatch(sw);

        if (Preferences.showPerfLogs)
        {
            LogThreaded($"tex upload: {sw2.ElapsedMilliseconds} ms");
            LogThreaded($"Break down: map {Micros(tMap)} copy {Micros(tCopy)} submit {Micros(tSubmit)}");
        }
    }*/
}