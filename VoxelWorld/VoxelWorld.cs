using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using RWCustom;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace VoxelWorld
{
    [BepInPlugin("com.slime-cubed.voxelworld", "Voxel World", "1.0.0")]
    public class VoxelWorld : BaseUnityPlugin
    {
        readonly Dictionary<Room, VoxelMap> voxelData = new Dictionary<Room, VoxelMap>();

        public void OnEnable()
        {
            On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
            On.AbstractRoom.Abstractize += AbstractRoom_Abstractize;
            On.Room.Update += Room_Update;
            On.Room.Loaded += Room_Loaded;
            On.RainWorld.Start += RainWorld_Start;

            On.RoomCamera.DrawUpdate += (orig, self, timeStacker, timeSpeed) =>
            {
                orig(self, timeStacker, timeSpeed);
                self.levelGraphic.isVisible = false;
            };
        }

        private void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            orig(self);
            foreach(var pair in voxelData)
            {
                pair.Value.Unload();
            }
            voxelData.Clear();
        }

        private void AbstractRoom_Abstractize(On.AbstractRoom.orig_Abstractize orig, AbstractRoom self)
        {
            if (voxelData.TryGetValue(self.realizedRoom, out var voxelMap))
            {
                voxelMap.Unload();
            }
            orig(self);
        }

        private void Room_Update(On.Room.orig_Update orig, Room self)
        {
            orig(self);
            if (voxelData.TryGetValue(self, out var voxelMap))
                voxelMap.Update();
        }

        private void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            if (self.game != null)
            {
                var voxelMap = new VoxelMap(self);
                voxelData[self] = voxelMap;
                voxelMap.AddVisuals();
            }
        }

        private void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            orig(self);

            self.Shaders["VoxelLevelColor"] = FShader.CreateShader("VoxelLevelColor", new Material(Shaders.VoxelLevelColor).shader);
        }

        private class VoxelMap
        {
            private readonly VoxelChunk[,] chunks;
            private readonly Room room;

            public string FilePath { get; }
            public uint XVoxels { get; }
            public uint YVoxels { get; }
            public uint ZVoxels { get; }
            public IntVector2 DisplayOffset { get; }
            public bool HasVoxelMap => chunks != null;

            public VoxelMap(Room room)
            {
                this.room = room;
                string roomName = room.abstractRoom.name;
                FilePath = $"{Custom.RootFolderDirectory()}Voxels/{roomName.Split(new string[] { "_" }, StringSplitOptions.None)[0]}/{roomName}_Voxels.vx1".Replace('/', Path.DirectorySeparatorChar);

                try
                {
                    using (var br = new BinaryReader(File.OpenRead(FilePath)))
                    {
                        uint width = XVoxels = br.ReadUInt16();
                        uint height = YVoxels = br.ReadUInt16();
                        uint depth = ZVoxels = br.ReadUInt16();

                        DisplayOffset = new IntVector2(
                            br.ReadInt16(),
                            br.ReadInt16()
                        );

                        int xChunks = (int)((width + VoxelChunk.viewSize - 1) / VoxelChunk.viewSize);
                        int yChunks = (int)((height + VoxelChunk.viewSize - 1) / VoxelChunk.viewSize);

                        chunks = new VoxelChunk[xChunks, yChunks];
                        for (int y = 0; y < yChunks; y++)
                        {
                            for (int x = 0; x < xChunks; x++)
                            {
                                chunks[x, y] = new VoxelChunk(this, x, y, br.ReadUInt32());
                            }
                        }
                    }
                }
                catch (System.IO.IsolatedStorage.IsolatedStorageException) { }
                catch (Exception e)
                {
                    Debug.LogException(new Exception("Failed to get voxel map!", e));
                    chunks = null;
                    XVoxels = 0;
                    YVoxels = 0;
                }
            }

            public void AddVisuals()
            {
                if (!HasVoxelMap) return;

                foreach(var chunk in chunks)
                {
                    room.AddObject(new VoxelChunkView(chunk));
                }
            }

            public void Update()
            {
                if(chunks != null)
                    foreach (var chunk in chunks)
                        chunk.Update();
            }

            public void Unload()
            {
                if (chunks != null)
                    foreach (var chunk in chunks)
                        chunk.Unload();
            }

            private class VoxelChunkView : UpdatableAndDeletable, IDrawable
            {
                private readonly VoxelChunk chunk;

                public VoxelChunkView(VoxelChunk chunk)
                {
                    this.chunk = chunk;
                }

                public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
                {
                    if (newContainer == null) newContainer = rCam.ReturnFContainer("Foreground");

                    newContainer.AddChild(sLeaser.sprites[0]);
                    sLeaser.sprites[0].MoveInFrontOfOtherNode(rCam.levelGraphic);
                }

                public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
                {
                }

                public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
                {
                    var spr = sLeaser.sprites[0];
                    spr.SetPosition(
                        new Vector2(
                            VoxelChunk.viewSize * chunk.X + chunk.Map.DisplayOffset.x * 20f,
                            VoxelChunk.viewSize * chunk.Y + chunk.Map.DisplayOffset.y * 20f)
                        - camPos
                    );

                    bool viewed = spr.x > -VoxelChunk.viewSize && spr.x < Futile.screen.width
                        && spr.y > -VoxelChunk.viewSize && spr.y < Futile.screen.height;

                    if(viewed)
                        chunk.Viewed();

                    if (viewed && chunk.Element != null)
                    {
                        spr.element = chunk.Element;
                        spr.width = VoxelChunk.viewSize;
                        spr.height = VoxelChunk.viewSize;
                        spr.isVisible = true;
                    }
                    else
                    {
                        spr.isVisible = false;
                    }
                }

                public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
                {
                    var spr = new FSprite("pixel")
                    {
                        anchorX = 0f,
                        anchorY = 0f,
                        shader = rCam.game.rainWorld.Shaders["VoxelLevelColor"]
                    };
                    sLeaser.sprites = new FSprite[] { spr };

                    AddToContainer(sLeaser, rCam, null);
                }
            }

            private class VoxelChunk
            {
                public const int size = 512;
                public const int viewSize = 384;
                public const int depth = 32;
                private const TextureFormat texFormat = TextureFormat.RGBA32;

                public VoxelMap Map { get; }
                public int X { get; }
                public int Y { get; }
                public FAtlasElement Element
                {
                    get
                    {
                        if (texture == null) return null;

                        if (atlas == null)
                        {
                            atlas = Futile.atlasManager.LoadAtlasFromTexture($"{Map.room.abstractRoom.name} ({X},{Y})", texture);
                            var elem = atlas.elements[0];

                            var uvs = new Rect(
                                (1f - viewSize / (float)size) / 2f,
                                (1f - viewSize / (float)size) / 2f,
                                viewSize / (float)size,
                                viewSize / (float)size);

                            elem.uvRect = uvs;
                            elem.uvTopLeft.Set(uvs.xMin, uvs.yMax);
                            elem.uvTopRight.Set(uvs.xMax, uvs.yMax);
                            elem.uvBottomRight.Set(uvs.xMax, uvs.yMin);
                            elem.uvBottomLeft.Set(uvs.xMin, uvs.yMin);
                        }

                        return atlas.elements[0];
                    }
                }

                private const float unloadDelay = 5f;
                private readonly uint dataAddress;

                private float lastViewed;
                private Texture3D texture;
                private FAtlas atlas;

                public VoxelChunk(VoxelMap map, int x, int y, uint dataAddress)
                {
                    Map = map;
                    X = x;
                    Y = y;
                    this.dataAddress = dataAddress;
                }

                public void Viewed()
                {
                    lastViewed = Time.time;

                    if (texture == null)
                        LoadTexture();
                }

                public void Update()
                {
                    if(Time.time - lastViewed > unloadDelay && texture != null)
                    {
                        UnloadTexture();
                    }
                }

                public void Unload()
                {
                    if (texture != null)
                        UnloadTexture();
                }

                private void LoadTexture()
                {
                    if (texture != null) return;
                    Debug.Log($"Voxel chunk {X}-{Y} loading...");
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        using (var br = new BinaryReader(File.OpenRead(Map.FilePath)))
                        {
                            br.BaseStream.Seek(dataAddress, SeekOrigin.Begin);

                            var tx = new Texture3D(size, size, depth, texFormat, false);
                            texture = tx;
                            tx.filterMode = FilterMode.Point;
                            tx.wrapMode = TextureWrapMode.Clamp;
                            Color[] pixels = new Color[size * size * depth];

                            int repeatCount = 0;
                            byte repeatVoxel = 0;

                            long i = 0;
                            for (int z = 0; z < Map.ZVoxels; z++)
                            {
                                //int endY = Math.Min(size, (int)(Map.YVoxels - size * Y));
                                for (int y = 0; y < size; y++)
                                {
                                    //int endX = Math.Min(size, (int)(Map.XVoxels - size * X));
                                    for (int x = 0; x < size; x++)
                                    {
                                        byte voxel;
                                        if (repeatCount > 0)
                                        {
                                            repeatCount--;
                                            voxel = repeatVoxel;
                                        }
                                        else
                                        {
                                            voxel = br.ReadByte();
                                            if (voxel == 0xFF)
                                            {
                                                repeatVoxel = voxel = br.ReadByte();
                                                repeatCount = br.Read7BitEncodedInt();
                                                repeatCount--;
                                            }
                                            else
                                            {
                                                repeatVoxel = voxel;
                                            }
                                        }

                                        pixels[i++] = new Color(0f, 0f, 0f, voxel / 255f);
                                    }
                                }
                            }

                            tx.SetPixels(pixels);
                            tx.Apply();
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(new FormatException($"Failed to load chunk {Map.room.abstractRoom.name} {X}-{Y}!", e));
                        if (texture == null)
                            texture = new Texture3D(size, size, depth, texFormat, false);
                    }

                    sw.Stop();
                    Debug.Log($"Done in {sw.ElapsedMilliseconds} ms!");
                }

                private void UnloadTexture()
                {
                    if (texture == null) return;
                    Debug.Log($"Voxel chunk {X}-{Y} unloaded");

                    if (atlas != null)
                    {
                        Futile.atlasManager.UnloadAtlas(atlas.name);
                    }
                    Destroy(texture);
                    texture = null;
                }
            }
        }
    }
}
