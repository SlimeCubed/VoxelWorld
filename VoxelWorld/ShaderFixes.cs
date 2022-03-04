using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoxelWorld
{
    internal static class ShaderFixes
    {
        private static bool applied;

        public static void Apply()
        {
            if (applied) return;
            applied = true;

            var rw = UnityEngine.Object.FindObjectOfType<RainWorld>();
            rw.Shaders["Decal"].shader = new Material(Shaders.Decal).shader;
            rw.Shaders["Fog"].shader = new Material(Shaders.Fog).shader;
            rw.Shaders["LevelColor"].shader = new Material(Shaders.LevelColor).shader;
        }
    }
}
