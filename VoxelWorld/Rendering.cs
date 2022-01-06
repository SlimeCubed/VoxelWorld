using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoxelWorld
{
    internal static class Rendering
    {
        private static VoxelCamera rtCam;
        private static RenderTexture rt;
        private static bool renderingVoxels;

        public static void Enable()
        {
            On.Futile.Init += Futile_Init;
            On.RoomCamera.DrawUpdate += RoomCamera_DrawUpdate;
            On.ShortcutGraphics.GenerateSprites += ShortcutGraphics_GenerateSprites;
        }

        private static readonly Dictionary<int, string> shortcutDotSprites = new Dictionary<int, string>()
        {
            { 0b1111, "ShortcutDotURDL" },
            { 0b0111, "ShortcutDotURD" },
            { 0b0011, "ShortcutDotUR" },
            { 0b0101, "ShortcutDotUD" },
            { 0b0001, "ShortcutDotUD" }
        };
        private static void ShortcutGraphics_GenerateSprites(On.ShortcutGraphics.orig_GenerateSprites orig, ShortcutGraphics self)
        {
            var map = VoxelWorld.GetVoxelMap(self.camera.room);
            self.shortcutShaders[0] = self.camera.game.rainWorld.Shaders[map?.HasVoxelMap ?? false ? "Basic" : "Shortcuts"];

            orig(self);

            if (map?.HasVoxelMap ?? false)
            {
                var voxels = map.Voxels;

                for (int x = 0; x < self.sprites.GetLength(0); x++)
                {
                    for (int y = 0; y < self.sprites.GetLength(1); y++)
                    {
                        var spr = self.sprites[x, y];
                        if (spr != null)
                        {
                            bool u = self.room.GetTile(x, y + 1).shortCut > 0;
                            bool r = self.room.GetTile(x + 1, y).shortCut > 0;
                            bool d = self.room.GetTile(x, y - 1).shortCut > 0;
                            bool l = self.room.GetTile(x - 1, y).shortCut > 0;

                            string elem = "ShortcutDotUD";
                            float rotation = 0f;

                            // Identify the sprite to use
                            int index = (u ? 0b0001 : 0)
                                      + (r ? 0b0010 : 0)
                                      + (d ? 0b0100 : 0)
                                      + (l ? 0b1000 : 0);
                            for(int shift = 0; shift < 4; shift++)
                            {
                                if (shortcutDotSprites.TryGetValue(index, out string tempElem))
                                {
                                    elem = tempElem;
                                    break;
                                }

                                // Rotate by 90 degrees and check again
                                rotation += 90f;
                                index = (index << 1 | index >> 3) & 0b1111;
                            }

                            spr.SetElementByName(elem);
                            spr.rotation = rotation;
                            spr.scale = 1f;
                            spr.color = new Color(1f, 0f, 0f);

                            // Hide sprites without solid gound behind
                            Vector2 pos = self.room.MiddleOfTile(x, y);
                            int vxlX = (int)pos.x;
                            int vxlY = (int)pos.y;
                            int vxlZ = Math.Min(map.GetDepth(vxlX, vxlY), map.ZVoxels - 1);

                            spr.isVisible = vxlZ < map.ZVoxels;
                        }
                    }
                }
            }
        }

        private static void RoomCamera_DrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
        {
            orig(self, timeStacker, timeSpeed);
            var vm = VoxelWorld.GetVoxelMap(self.room);
            var lg = self.levelGraphic;
            if (lg.element.name == "LevelTexture")
            {
                if (vm?.HasVoxelMap ?? false)
                    lg.SetElementByName("VoxelLevelTexture");
            }
            else if(lg.element.name == "VoxelLevelTexture")
            {
                if (!(vm?.HasVoxelMap ?? false))
                    lg.SetElementByName("LevelTexture");
            }

            if (Preferences.viewRawImage)
                lg.shader = self.game.rainWorld.Shaders["Basic"];

            bool nowRenderingVoxels = lg.element.name == "VoxelLevelTexture";
            if(!nowRenderingVoxels && renderingVoxels)
            {
                Shader.SetGlobalTexture("_LevelTex", self.levelTexture);
            }
            renderingVoxels = nowRenderingVoxels;
        }

        private static void Futile_Init(On.Futile.orig_Init orig, Futile self, FutileParams futileParams)
        {
            orig(self, futileParams);

            rt = new RenderTexture(1400, 800, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            rt.anisoLevel = 0;
            rt.generateMips = false;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("VoxelLevelTexture", rt);

            var go = new GameObject("Voxel Camera", typeof(Camera), typeof(VoxelCamera));
            go.transform.parent = self.camera.transform;
            rtCam = go.GetComponent<VoxelCamera>();
        }

        private class VoxelCamera : MonoBehaviour
        {
            private Camera cam;
            private Camera srcCam;

            private Camera Cam => cam ?? (cam = GetComponent<Camera>());
            private Camera SrcCam => srcCam ?? (srcCam = transform.parent.GetComponent<Camera>());

            public void LateUpdate()
            {
                //UpdateSimulatedCamera();
                UpdateCamera();
            }

            // Real camera
            public void UpdateCamera()
            {
                Cam.transform.position = Cam.transform.parent.position + new Vector3(0f, 0f, -Preferences.cameraDepth);
                Cam.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                Cam.orthographic = false;
                Cam.fieldOfView = 2f * Mathf.Rad2Deg * Mathf.Atan2(Cam.pixelHeight / 2f, Cam.transform.localPosition.z);
                Cam.nearClipPlane = Preferences.cameraDepth * 0.05f;
                Cam.farClipPlane = Preferences.cameraDepth * 5f;
                Cam.depth = SrcCam.depth - 1f;
                Cam.enabled = renderingVoxels;
            }

            // Simulated camera
            // Unused
            public void UpdateSimulatedCamera()
            {
                Vector4 camPos = transform.position;
                camPos.w = 1f;
                camPos.z = -Preferences.cameraDepth;
                Matrix4x4 viewProjMatrix =
                    Matrix4x4.Perspective(2f * Mathf.Rad2Deg * Mathf.Atan2(Cam.pixelHeight / 2f, -camPos.z), Cam.aspect, Preferences.cameraDepth * 0.01f, Preferences.cameraDepth * 2f)
                    * Matrix4x4.TRS(new Vector3(-camPos.x, -camPos.y, camPos.z + Preferences.chunkSize * Preferences.playLayerDepth), Quaternion.Inverse(transform.rotation), new Vector3(1f, 1f, -1f));

                Shader.SetGlobalVector("_SimulateCameraPos", camPos);
                Shader.SetGlobalMatrix("_SimulateVP", viewProjMatrix);
            }

            public void OnPreCull()
            {
                // Copy render params from the main camera, but only render voxels
                Cam.targetTexture = rt;
                Cam.backgroundColor = Color.white;
                Cam.cullingMask = 1 << Preferences.voxelSpriteLayer;
                SrcCam.cullingMask &= ~(1 << Preferences.voxelSpriteLayer);

                Shader.SetGlobalTexture("_LevelTex", rt);
            }
        }
    }
}
