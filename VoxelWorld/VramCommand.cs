using System;
using System.Runtime.InteropServices;
using DevConsole;
using DevConsole.Commands;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.DXGI_MEMORY_SEGMENT_GROUP;
using static TerraFX.Interop.Windows.Windows;

namespace VoxelWorld;

internal static class VramCommand
{
    public static void TryRegister()
    {
        try
        {
            Register();
        }
        catch
        {
            
        }
    }

    private static unsafe void Register()
    {
        new CommandBuilder("vram")
            .Run(_ =>
            {
                var device = VoxelWorld.D3DDevice;
                IDXGIDevice* dxgiObject;
                device->QueryInterface(__uuidof<IDXGIDevice>(), (void**)&dxgiObject);
                IDXGIAdapter* dxgiAdapter;
                dxgiObject->GetAdapter(&dxgiAdapter);
                dxgiObject->Release();

                IDXGIAdapter3* dxgiAdapter3;
                dxgiAdapter->QueryInterface(__uuidof<IDXGIAdapter3>(), (void**)&dxgiAdapter3);
                dxgiAdapter->Release();

                if (dxgiAdapter3 == null)
                {
                    GameConsole.WriteLine("Unable to get IDXGIAdapter3");
                    return;
                }

                try
                {
                    DXGI_ADAPTER_DESC desc;
                    dxgiAdapter3->GetDesc(&desc);

                    var descString = Marshal.PtrToStringUni((IntPtr)desc.Description);
                    GameConsole.WriteLine($"Adapter: {descString}");

                    DXGI_QUERY_VIDEO_MEMORY_INFO memInfo;

                    const double mibibyte = 1024 * 1024;
                    
                    dxgiAdapter3->QueryVideoMemoryInfo(0, DXGI_MEMORY_SEGMENT_GROUP_LOCAL, &memInfo);
                    GameConsole.WriteLine($"Usage (local): {memInfo.CurrentUsage / mibibyte:N2} MiB");

                    dxgiAdapter3->QueryVideoMemoryInfo(0, DXGI_MEMORY_SEGMENT_GROUP_NON_LOCAL, &memInfo);
                    GameConsole.WriteLine($"Usage (non local): {memInfo.CurrentUsage / mibibyte:N2} MiB");
                }
                finally
                {
                    dxgiAdapter3->Release();
                }
            })
            .Register();
    }
}