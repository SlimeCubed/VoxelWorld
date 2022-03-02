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
        private static RoomPos renderingCamPos;

        public static void Enable()
        {
            On.Futile.Init += Futile_Init;
            On.RoomCamera.DrawUpdate += RoomCamera_DrawUpdate;
            On.ShortcutGraphics.GenerateSprites += ShortcutGraphics_GenerateSprites;
            On.RoomCamera.DepthAtCoordinate += RoomCamera_DepthAtCoordinate;
            On.RoomCamera.PixelColorAtCoordinate += RoomCamera_PixelColorAtCoordinate;
        }

        private static Color RoomCamera_PixelColorAtCoordinate(On.RoomCamera.orig_PixelColorAtCoordinate orig, RoomCamera self, Vector2 coord)
        {
            VoxelMap map = VoxelWorld.GetVoxelMap(self.room);
            if (map?.HasVoxelMap ?? false)
            {
                int x = (int)coord.x;
                int y = (int)coord.y;
                int z = map.GetDepth(x, y);

                byte voxel = map.GetVoxel(x, y, z);
                int paletteIndex = VoxelMap.GetPaletteColor(voxel);

                // Adapted from RoomCamera.PixelColorAtCoordinate
                if (z >= 30 || paletteIndex == 0)
                    return self.paletteTexture.GetPixel(0, 7);
                
                // TODO: Check lightmask
                float shadow = 1f;

                paletteIndex--;
                Color color = Color.Lerp(self.paletteTexture.GetPixel(z, paletteIndex + 3), self.paletteTexture.GetPixel(z, paletteIndex), shadow);
                return Color.Lerp(color, self.paletteTexture.GetPixel(1, 7), z * (1f - self.paletteTexture.GetPixel(9, 7).r) / 30f);
            }
            else
                return orig(self, coord);
        }

        private static float RoomCamera_DepthAtCoordinate(On.RoomCamera.orig_DepthAtCoordinate orig, RoomCamera self, Vector2 coord)
        {
            VoxelMap map = VoxelWorld.GetVoxelMap(self.room);
            if (map?.HasVoxelMap ?? false)
            {
                return map.GetDepth((int)coord.x, (int)coord.y);
            }
            else
                return orig(self, coord);
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
            if (Preferences.uncapFramerates)
            {
                Application.targetFrameRate = -1;
                QualitySettings.vSyncCount = 0;
            }

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
            renderingCamPos = new RoomPos(self.room, Vector2.Lerp(self.lastPos, self.pos, timeStacker));

            if (nowRenderingVoxels)
            {
                rtCam.lightAngle.x = Mathf.Atan2(self.room.lightAngle.x, Preferences.chunkSize / 15f) * Mathf.Rad2Deg;
                rtCam.lightAngle.y = Mathf.Atan2(self.room.lightAngle.y, Preferences.chunkSize / 15f) * Mathf.Rad2Deg;
            }
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
            public Vector3 lightAngle = new Vector3(0f, 0f, 180f);
            private Camera cam;
            private Camera lightCam;
            private Camera srcCam;
            private Shader voxelDepth;
            private RenderTexture shadowMap;
            private RoomPos shadowPos;

            private Camera Cam => cam ?? (cam = GetComponent<Camera>());
            private Camera SrcCam => srcCam ?? (srcCam = transform.parent.GetComponent<Camera>());

            public void Start()
            {
                var go = new GameObject("Sunlight Shadowmapper");
                lightCam = go.AddComponent<Camera>();
                lightCam.enabled = false;
                voxelDepth = FindObjectOfType<RainWorld>().Shaders["VoxelDepth"].shader;

                shadowMap = new RenderTexture(Preferences.shadowMapSize, Preferences.shadowMapSize, 32, RenderTextureFormat.RFloat);
                shadowMap.filterMode = FilterMode.Trilinear;
                shadowMap.anisoLevel = 0;
                lightCam.targetTexture = shadowMap;
                lightCam.backgroundColor = Color.clear;
            }

            public void OnDestroy()
            {
                Destroy(lightCam.gameObject);
            }

            public void LateUpdate()
            {
                //UpdateSimulatedCamera();
                UpdateCamera();
                UpdateLightCamera();
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

            // Camera used for shadowmapping
            public void UpdateLightCamera()
            {
                bool dirty = ((renderingCamPos.Distance(shadowPos) > 20f) || Input.GetKey(KeyCode.Alpha6) || Input.GetKeyUp(KeyCode.Alpha6)) && renderingVoxels;

                lightCam.enabled = dirty;
                if(dirty)
                {
                    shadowPos = renderingCamPos;
                }

                Quaternion angle = Quaternion.Euler(lightAngle);
                lightCam.transform.position = Cam.transform.parent.position - angle * Vector3.forward * Preferences.sunDistance;
                lightCam.transform.position += (Vector3)(shadowPos.pos - renderingCamPos.pos);
                lightCam.transform.localRotation = angle;
                lightCam.orthographic = false;

                // The projection matrix approximates a cube
                // It must always cover the visible portion of the level
                float cubeSize = Mathf.Max(Cam.pixelWidth, Cam.pixelHeight) * Preferences.shadowMapScale;

                lightCam.nearClipPlane = Preferences.sunDistance - cubeSize / 2f;
                lightCam.farClipPlane = Preferences.sunDistance + cubeSize / 2f;
                lightCam.fieldOfView = 2f * Mathf.Rad2Deg * Mathf.Atan2(cubeSize / 2f, lightCam.nearClipPlane);
                
                lightCam.SetReplacementShader(voxelDepth, "RenderType");
                lightCam.depth = -1000f;
                lightCam.cullingMask = 1 << Preferences.voxelSpriteLayer | 1 << Preferences.lightCookieLayer;

                if (Preferences.viewShadowMap)
                {
                    if (Input.GetKey(KeyCode.Alpha6))
                    {
                        lightCam.depth = 1000000f;
                        lightCam.targetTexture = null;
                    }
                    else
                        lightCam.targetTexture = shadowMap;
                }

                // Update shader variables
                Matrix4x4 toCam = lightCam.worldToCameraMatrix;
                toCam[2, 3] += Preferences.lightPenetration;
                Shader.SetGlobalMatrix("_VoxelShadowVP", lightCam.projectionMatrix * toCam);
                Shader.SetGlobalTexture("_VoxelShadowTex", shadowMap);
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
                SrcCam.cullingMask &= ~(1 << Preferences.voxelSpriteLayer | 1 << Preferences.lightCookieLayer);

                Shader.SetGlobalTexture("_LevelTex", rt);
            }
        }

        private struct RoomPos
        {
            public int room;
            public Vector2 pos;

            public RoomPos(Room room, Vector2 pos)
            {
                this.room = room?.abstractRoom?.index ?? -1;
                this.pos = pos;
            }

            public float Distance(RoomPos other)
            {
                if (room == -1 || other.room == -1) return float.PositiveInfinity;
                return room == other.room ? Vector2.Distance(pos, other.pos) : float.PositiveInfinity;
            }

            public override bool Equals(object obj)
            {
                return obj is RoomPos other &&
                       room == other.room &&
                       EqualityComparer<Vector2>.Default.Equals(pos, other.pos);
            }

            public override int GetHashCode()
            {
                int hashCode = -1039860717;
                hashCode = hashCode * -1521134295 + room.GetHashCode();
                hashCode = hashCode * -1521134295 + pos.GetHashCode();
                return hashCode;
            }

            public static bool operator ==(RoomPos left, RoomPos right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(RoomPos left, RoomPos right)
            {
                return !(left == right);
            }
        }
    }
}
