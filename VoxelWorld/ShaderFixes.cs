using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoxelWorld
{
    internal static class ShaderFixes
    {
        public static void Enable()
        {
            On.RainWorld.Start += (orig, self) =>
            {
                orig(self);

                self.Shaders["Decal"].shader = new Material(Shaders.Decal).shader;
                self.Shaders["Fog"].shader = new Material(Shaders.Fog).shader;
                self.Shaders["LevelColor"].shader = new Material(Shaders.LevelColor).shader;
            };
        }
    }
}
