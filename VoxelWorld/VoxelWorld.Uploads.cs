using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using TerraFX.Interop.DirectX;
using UnityEngine;
using static TerraFX.Interop.DirectX.D3D11_CPU_ACCESS_FLAG;
using static TerraFX.Interop.DirectX.D3D11_FEATURE;
using static TerraFX.Interop.DirectX.D3D11_MAP;
using static TerraFX.Interop.DirectX.D3D11_MAP_FLAG;
using static TerraFX.Interop.DirectX.D3D11_USAGE;
using static TerraFX.Interop.DirectX.DXGI_FORMAT;
using static TerraFX.Interop.DirectX.DXGI;
using static VoxelWorld.Helpers;
using Debug = UnityEngine.Debug;

namespace VoxelWorld;

public unsafe partial class VoxelWorld
{
    public static ID3D11Device* D3DDevice;
    public static ID3D11DeviceContext* D3DImmediate;
    private static ID3D11Texture3D*[] _stagingPool;
    private static int _poolIndex;
    private static readonly Queue<CmdVoxelChunkUpload> VoxelUploadQueue = new();

    private static void StartRenderThread()
    {
        LogThreaded($"[VoxelWorld] Main thread: {Thread.CurrentThread.ManagedThreadId:x}");
        D3DDevice = Init(Marshal.GetFunctionPointerForDelegate(renderThreadCallback));

        GL.IssuePluginEvent((int)PluginEvents.Init);
    }

    private static void ShutdownRenderThread()
    {
        //Debug.Log("ASDASDASD");
        //Detach();
    }

    private static readonly Action<int> renderThreadCallback = RenderThreadCallback;

    private static void RenderThreadCallback(int eventId)
    {
        try
        {
            var eventEnum = (PluginEvents)eventId;
            switch (eventEnum)
            {
                case PluginEvents.Init:
                    RenderThreadInit();
                    break;

                case PluginEvents.ChunkUpload:
                    RenderThreadDoChunkUpload();
                    break;
            }
        }
        catch (Exception e)
        {
            LogThreaded($"Exception in render thread! {e}");
        }
    }

    private static void RenderThreadInit()
    {
        GetD3D11Device();
        InitUploadBuffers();
    }

    private static void GetD3D11Device()
    {
        D3DDevice->AddRef();

        LogThreaded($"Initializing VoxelWorld in render thread!");
        LogThreaded($"[VoxelWorld] Render thread: {Thread.CurrentThread.ManagedThreadId:x}");

        fixed (ID3D11DeviceContext** imm = &D3DImmediate)
            D3DDevice->GetImmediateContext(imm);

        var fl = D3DDevice->GetFeatureLevel();

        LogThreaded($"D3D11 FL: {fl}");

        D3D11_FEATURE_DATA_THREADING threading;
        ThrowIfFailed(D3DDevice->CheckFeatureSupport(
            D3D11_FEATURE_THREADING,
            &threading,
            (uint)sizeof(D3D11_FEATURE_DATA_THREADING)));

        LogThreaded(
            $"D3D11 threading supported? Creates: {(bool)threading.DriverConcurrentCreates}, command lists: {(bool)threading.DriverCommandLists}");

        var flags = (D3D11_CREATE_DEVICE_FLAG)D3DDevice->GetCreationFlags();
        LogThreaded($"D3D11 creation flags: {flags}");

        Thread.CurrentThread.IsBackground = true;
    }

    private static void InitUploadBuffers()
    {
        _stagingPool = new ID3D11Texture3D*[Preferences.uploadPoolSize];

        for (var i = 0; i < _stagingPool.Length; i++)
        {
            var desc = new D3D11_TEXTURE3D_DESC
            {
                Width = Preferences.chunkSize,
                Height = Preferences.chunkSize,
                Depth = Preferences.chunkDepth,
                Usage = D3D11_USAGE_STAGING,
                Format = DXGI_FORMAT_A8_UNORM,
                MipLevels = 0,
                BindFlags = 0,
                MiscFlags = 0,
                CPUAccessFlags = (uint)D3D11_CPU_ACCESS_WRITE | (uint)D3D11_CPU_ACCESS_READ
            };

            fixed (ID3D11Texture3D** tex = &_stagingPool[i])
                ThrowIfFailed(D3DDevice->CreateTexture3D(&desc, null, tex));
        }
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
        var cmd = new CmdVoxelChunkUpload
        {
            Chunk = chunk,
        };

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

        cmd.Texture = (ID3D11Texture3D*)chunk.Texture.GetNativeTexturePtr();

        lock (VoxelUploadQueue)
        {
            VoxelUploadQueue.Enqueue(cmd);
        }

        GL.IssuePluginEvent((int)PluginEvents.ChunkUpload);
    }

    private static void RenderThreadDoChunkUpload()
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
    }

    private sealed class CmdVoxelChunkUpload
    {
        public ID3D11Texture3D* Texture;
        public VoxelMapView.VoxelChunk Chunk;
    }
}