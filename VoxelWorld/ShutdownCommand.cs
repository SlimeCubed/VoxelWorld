using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevConsole;
using DevConsole.Commands;

namespace VoxelWorld
{
    internal static class ShutdownCommand
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
            new CommandBuilder("shutdown")
                .Run(_ =>
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                })
                .Register();
        }
    }
}
