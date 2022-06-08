using System;
using System.Runtime.InteropServices;
using TerraFX.Interop.DirectX;

namespace VoxelWorld;

public unsafe partial class VoxelWorld
{
    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Init(VoxelWorldNativePreferences* preferences);

    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    private static extern char* LogFetch();
    
    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern void QueueVoxelUpload(IntPtr texture, VoxelMapData* map, int chunkX, int chunkY);
    
    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern void LZ4Decompress(
        byte* src, byte* dst, int compressedSize, int dstCapacity);

    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Shutdown();

    
    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern VoxelMapData* VoxelMapAllocate([MarshalAs(UnmanagedType.LPWStr)] string name);

    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern VoxelMapData* VoxelMapInit(VoxelMapData* ptr);
    
    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern VoxelMapData* VoxelMapAllocChunk(VoxelMapData* ptr, int chunk, int length);

    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern void VoxelMapFree(VoxelMapData* ptr);

    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern void VoxelMapMemcpy(void* dst, void* src, nuint count);

    [DllImport("VoxelWorldNative", CallingConvention = CallingConvention.Cdecl)]
    public static extern void QueryD3D11Device(DxgiDeviceQuery* query);
   
    public enum PluginEvents : int
    {
        Init = 0,
        ChunkUpload,
        Shutdown = -1
    }

    public struct VoxelWorldNativePreferences
    {
        public int UploadPoolSize;
        public int ChunkSize;
        public int ChunkDepth;
    }

    public struct VoxelMapData
    {
        public byte** LZ4Chunks;
        public int* LZ4ChunkLengths;
        public int CountLZ4Chunks;
        public int XVoxels;
        public int YVoxels;
        public byte VoxelsLoaded;
        
        public void* PtrPtr;
    }
    
    public struct DxgiDeviceQuery
    {
        public DXGI_ADAPTER_DESC Desc;
        public byte VideoMemoryFetched;
        public DXGI_QUERY_VIDEO_MEMORY_INFO VideoMemoryLocal;
        public DXGI_QUERY_VIDEO_MEMORY_INFO VideoMemoryNonLocal;
    };
}