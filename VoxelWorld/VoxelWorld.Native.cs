using System;
using System.Runtime.InteropServices;
using TerraFX.Interop.DirectX;

namespace VoxelWorld;

public unsafe partial class VoxelWorld
{
    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    private static extern ID3D11Device* Init(IntPtr callback);

    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    private static extern void CopyVoxelsToTex(
        byte* dst, byte* src,
        int w, int h, int d,
        int xMin, int xMax,
        int yMin, int yMax,
        int zMin, int zMax,
        int step);

    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern void LZ4Decompress(
        byte* src, byte* dst, int compressedSize, int dstCapacity);

    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Detach();

    public enum PluginEvents : int
    {
        Init = 0,
        ChunkUpload,
        Detach = -1
    }
}