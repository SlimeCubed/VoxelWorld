using System;
using System.Runtime.InteropServices;
using DevConsole;
using DevConsole.Commands;
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
                VoxelWorld.DxgiDeviceQuery query;
                VoxelWorld.QueryD3D11Device(&query);
                
                if (query.VideoMemoryFetched == 0)
                {
                    GameConsole.WriteLine("Unable to get VRAM info: DXGI 1.4 not available");
                    return;
                }

                var descString = Marshal.PtrToStringUni((IntPtr)query.Desc.Description);
                GameConsole.WriteLine($"Adapter: {descString}");

                const double mibibyte = 1024 * 1024;
                
                GameConsole.WriteLine($"Usage (local): {query.VideoMemoryLocal.CurrentUsage / mibibyte:N2} MiB");
                GameConsole.WriteLine($"Usage (non local): {query.VideoMemoryNonLocal.CurrentUsage / mibibyte:N2} MiB");
            })
            .Register();
    }
}