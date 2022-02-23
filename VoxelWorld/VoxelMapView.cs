using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelWorld
{
    // Displays a voxel map
    internal class VoxelMapView : UpdatableAndDeletable, IDrawable
    {
        private float ShouldLoadDistance => VoxelChunk.size * Preferences.preloadDistanceFactor;
        private float MustLoadDistance => VoxelChunk.size * Preferences.forceLoadDistanceFactor;
        private static readonly List<VoxelChunk> shouldLoad = new List<VoxelChunk>();
        private static readonly List<VoxelChunk> mustLoad = new List<VoxelChunk>();

        public int ChunksChanged { get; private set; }
        private readonly VoxelMap map;
        private readonly VoxelChunk[,] chunks;
        private int forceLoadTimer;
        private Texture2D voxelLightCookie;

        public VoxelMapView(VoxelMap map)
        {
            Debug.Log($"Created voxel map view: {map.room.abstractRoom.name}");
            this.map = map;

            // Allocate space for chunk textures
            chunks = new VoxelChunk[(map.XVoxels + VoxelChunk.size - 1) / VoxelChunk.size, (map.YVoxels + VoxelChunk.size - 1) / VoxelChunk.size];
            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                for (int x = 0; x < chunks.GetLength(0); x++)
                {
                    chunks[x, y] = new VoxelChunk(map, x, y);
                }
            }

            try
            {
                var bytes = File.ReadAllBytes(VoxelWorld.GetRoomFilePath(map.room.abstractRoom.name, ".png"));
                var tex = new Texture2D(1, 1, TextureFormat.DXT5, false);
                tex.anisoLevel = 0;
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.LoadImage(bytes);
                tex.Apply(false, true); // Make the texture non-readable, freeing memory

                voxelLightCookie = tex;
            }
            catch
            {
                // Don't add a light cookie if the file doesn't exist
                voxelLightCookie = null;
            }
        }

        public void ForceLoad()
        {
            forceLoadTimer = Math.Max(forceLoadTimer, 10);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            if (newContainer == null) newContainer = rCam.ReturnFContainer("Foreground");

            if(voxelLightCookie != null)
            {
                newContainer.AddChild(sLeaser.containers[1]);
                sLeaser.containers[1].MoveInFrontOfOtherNode(rCam.levelGraphic);
            }
            newContainer.AddChild(sLeaser.containers[0]);
            sLeaser.containers[0].MoveInFrontOfOtherNode(rCam.levelGraphic);
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            ChunksChanged = 0;

            // Update light cookie
            if (voxelLightCookie != null)
            {
                var size = new Vector2(voxelLightCookie.width, voxelLightCookie.height);
                var cookie = (FLightCookieSprite)sLeaser.containers[1].GetChildAt(0);
                cookie.SetPosition(new Vector2(map.XVoxels, map.YVoxels) / 2f + map.DisplayOffset.ToVector2() * 20f - camPos - new Vector2(rCam.room.lightAngle.x, -rCam.room.lightAngle.y) * 30f);
                cookie.scaleX = size.x;
                cookie.scaleY = size.y;
            }

            var container = sLeaser.containers[0];
            int width = chunks.GetLength(0);
            int height = chunks.GetLength(1);

            var shouldLoadQuality = Preferences.onlyLowQuality ? VoxelChunk.TextureQuality.Low : VoxelChunk.TextureQuality.High;
            var mustLoadQuality = (Preferences.allowLowQuality || Preferences.onlyLowQuality) ? VoxelChunk.TextureQuality.Low : VoxelChunk.TextureQuality.High;

            if (forceLoadTimer > 0)
                mustLoadQuality = shouldLoadQuality;

            // Find chunks that should be loaded
            for (int chunkY = 0; chunkY < height; chunkY++)
            {
                for (int chunkX = 0; chunkX < width; chunkX++)
                {
                    var chunk = chunks[chunkX, chunkY];
                    var node = (FVoxelChunkNode)container.GetChildAt(chunkX + chunkY * width);

                    Vector2 pos = new Vector2(
                            VoxelChunk.size * chunkX + map.DisplayOffset.x * 20f,
                            VoxelChunk.size * chunkY + map.DisplayOffset.y * 20f)
                        - camPos;
                    node.SetPosition(pos);
                    node.scale = VoxelChunk.size;


                    float offscreenDistance = Mathf.Max(
                        Mathf.Max(pos.x - Futile.screen.width, -VoxelChunk.size - pos.x),
                        Mathf.Max(pos.y - Futile.screen.height, -VoxelChunk.size - pos.y)
                    );

                    if (offscreenDistance < ShouldLoadDistance)
                    {
                        chunk.Viewed();

                        if(chunk.Quality < shouldLoadQuality)
                            shouldLoad.Add(chunk);

                        if (chunk.Quality < mustLoadQuality && offscreenDistance < MustLoadDistance)
                            mustLoad.Add(chunk);
                    }
                }
            }

            // Load chunks
            if(shouldLoad.Count > 0)
            {
                Vector2 focalPoint = rCam.followAbstractCreature?.realizedCreature?.mainBodyChunk.pos ?? (camPos + new Vector2(Futile.screen.halfWidth, Futile.screen.halfHeight));
                var loadChunksHQ = shouldLoad.OrderBy(c => Vector2.SqrMagnitude(c.WorldCenter - focalPoint)).Take(Preferences.loadPerFrameLimit);

                // Load a fixed amount of chunks at full quality
                foreach (var loadChunkHQ in loadChunksHQ)
                {
                    loadChunkHQ.Quality = shouldLoadQuality;
                    ChunksChanged++;
                }
                
                // Load any chunks that are on screen but not loaded with a low-quality version
                foreach(var loadChunkLQ in mustLoad)
                {
                    if (loadChunksHQ.Contains(loadChunkLQ)) continue;
                    loadChunkLQ.Quality = mustLoadQuality;
                    ChunksChanged++;
                }
            }
            
            // Apply texture changes
            for (int chunkY = 0; chunkY < height; chunkY++)
            {
                for (int chunkX = 0; chunkX < width; chunkX++)
                {
                    var chunk = chunks[chunkX, chunkY];
                    var node = (FVoxelChunkNode)container.GetChildAt(chunkX + chunkY * width);

                    if (chunk.IsViewed && chunk.Quality > VoxelChunk.TextureQuality.Unloaded)
                    {
                        node.Texture = chunk.Texture;
                        node.VoxelBounds = chunk.VoxelBounds;
                        node.Mesh = chunk.Mesh;
                        node.isVisible = true;
                        node.gameObject.SetActive(true);
                    }
                    else
                    {
                        node.isVisible = false;
                        node.gameObject.SetActive(false);
                    }
                }
            }

            mustLoad.Clear();
            shouldLoad.Clear();

            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (slatedForDeletetion)
            {
                foreach (var chunk in chunks)
                    chunk.Quality = VoxelChunk.TextureQuality.Unloaded;
            }
            else
            {
                foreach (var chunk in chunks)
                    chunk.Update();
            }

            if (forceLoadTimer > 0)
                forceLoadTimer--;
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            var voxelContainer = new FContainer();
            var cookieContainer = new FContainer();

            sLeaser.sprites = new FSprite[0];
            sLeaser.containers = new FContainer[] { voxelContainer, cookieContainer };

            if(voxelLightCookie != null)
            {
                Debug.Log("Added cookie!");
                cookieContainer.AddChild(new FLightCookieSprite(voxelLightCookie));
            }

            int width = chunks.GetLength(0);
            int height = chunks.GetLength(1);
            for (int chunkY = 0; chunkY < height; chunkY++)
            {
                for (int chunkX = 0; chunkX < width; chunkX++)
                {
                    voxelContainer.AddChild(new FVoxelChunkNode(rCam.room.game.rainWorld));
                }
            }

            AddToContainer(sLeaser, rCam, null);
        }

        ~VoxelMapView()
        {
            if (voxelLightCookie != null)
            {
                TextureManager.EnqueueDeletion(voxelLightCookie);
            }
        }

        internal class VoxelChunk
        {
            public static int size = Preferences.chunkSize;
            public static int depth = 32;
            private const TextureFormat texFormat = TextureFormat.Alpha8;
            private static Color[] pixelBuffer;

            public VoxelMap Map { get; }
            public int X { get; }
            public int Y { get; }
            public Texture3D Texture { get; private set; }
            public Bounds VoxelBounds { get; private set; }
            public Mesh Mesh { get; private set; }
            public TextureQuality Quality
            {
                get => quality;
                set
                {
                    if (value != quality)
                    {
                        quality = value;
                        if (value == TextureQuality.Unloaded)
                        {
                            DestroyTexture();
                            DestroyMesh();
                        }
                        else
                        {
                            CreateTexture(value);
                            CreateMesh();
                        }
                    }
                }
            }

            private float lastViewed;
            private TextureQuality quality;

            public VoxelChunk(VoxelMap map, int x, int y)
            {
                Map = map;
                X = x;
                Y = y;
            }

            public void Viewed()
            {
                lastViewed = Time.unscaledTime;
            }

            public bool IsViewed => Time.unscaledTime <= lastViewed + 0.1f;

            public Vector2 WorldCenter => new Vector2((X + 0.5f) * size, (Y + 0.5f) * size) + Map.DisplayOffset.ToVector2() * 20f;

            private void CreateTexture(TextureQuality quality)
            {
                DestroyTexture();

                // Higher steps skip more voxels
                int step = quality == TextureQuality.High ? 1 : 2;

                // Determine bounds of useful voxel data
                int xMin = size * X;
                int yMin = size * Y;
                int zMin = 0;
                int xMax = size * (X + 1);
                int yMax = size * (Y + 1);
                int zMax = depth;

                if (quality == TextureQuality.High)
                    CollapseBounds(ref xMin, ref yMin, ref zMin, ref xMax, ref yMax, ref zMax);

                {
                    var bounds = new Bounds();
                    var bMin = new Vector3(xMin - size * X, yMin - size * Y, zMin);
                    var bMax = new Vector3(xMax - size * X, yMax - size * Y, zMax);
                    var scl = new Vector3(1f / size, 1f / size, 1f / depth);
                    bMin.Scale(scl);
                    bMax.Scale(scl);
                    bounds.SetMinMax(bMin, bMax);
                    VoxelBounds = bounds;
                }

                // Create texture
                Texture = new Texture3D(
                    Math.Max((xMax - xMin) / step, 1),
                    Math.Max((yMax - yMin) / step, 1),
                    Math.Max((zMax - zMin) / step, 1),
                    texFormat,
                    mipmap: false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Point,
                    anisoLevel = 0,
                    name = $"Voxel ({X}, {Y})"
                };

                // Fill pixel buffer
                if (pixelBuffer == null)
                    pixelBuffer = new Color[size * size * depth];

                var src = Map.Voxels;
                var dst = pixelBuffer;

                int i = 0;

                int xVoxels = Map.XVoxels;
                int yVoxels = Map.YVoxels;
                int zVoxels = Map.ZVoxels;

                for (int z = zMin; z < zMax; z += step)
                {
                    int zOffset = z * xVoxels * yVoxels;
                    for (int y = yMin; y < yMax; y += step)
                    {
                        int yOffset = y * xVoxels;
                        for (int x = xMin; x < xMax; x += step)
                        {
                            byte a;
                            if (x < xVoxels && y < yVoxels && z < zVoxels)
                                a = src[x + yOffset + zOffset];
                            else
                                a = 0;

                            dst[i++].a = a / 255f;
                        }
                    }
                }

                // Upload texture
                Texture.SetPixels(dst);
                Texture.Apply();
            }

            private static readonly QuadDirs[] quadDirs = new QuadDirs[]
            {
                new QuadDirs(new IntVector3(0, 0, -1), new IntVector3(0, 1, 0), new IntVector3(1, 0, 0)),
                new QuadDirs(new IntVector3(1, 0, 0), new IntVector3(0, 1, 0), new IntVector3(0, 0, 1)),
                new QuadDirs(new IntVector3(0, 0, 1), new IntVector3(0, 1, 0), new IntVector3(-1, 0, 0)),
                new QuadDirs(new IntVector3(-1, 0, 0), new IntVector3(0, 1, 0), new IntVector3(0, 0, -1)),
                new QuadDirs(new IntVector3(0, 1, 0), new IntVector3(0, 0, 1), new IntVector3(1, 0, 0)),
                new QuadDirs(new IntVector3(0, -1, 0), new IntVector3(0, 0, -1), new IntVector3(1, 0, 0)),
            };
            private static bool[] visitedSubchunks;
            private static readonly List<int> inds = new List<int>();
            private static readonly List<Vector3> verts = new List<Vector3>();
            private static readonly Dictionary<Vector3, int> vertInds = new Dictionary<Vector3, int>();
            public void CreateMesh()
            {
                DestroyMesh();
                
                int w = size / Preferences.subchunkSize;
                int h = size / Preferences.subchunkSize;
                int d = depth / Preferences.subchunkSize;
                int ox = X * w;
                int oy = Y * h;
                int scsW = Map.XSubchunks;
                int scsH = Map.YSubchunks;
                int scsD = Map.ZSubchunks;
                bool[] subchunks = Map.Subchunks;

                if (visitedSubchunks == null || visitedSubchunks.Length != w * h * d)
                    visitedSubchunks = new bool[w * h * d];

                bool IsSolid(IntVector3 pos)
                {
                    var x = pos.x;
                    var y = pos.y;
                    var z = pos.z;
                    if (x < 0 || y < 0 || z < 0
                        || x >= w || y >= h || z >= d || z >= scsD) return false;
                    x += ox;
                    y += oy;
                    if (x < 0 || y < 0
                        || x >= scsW || y >= scsH) return false;
                    return subchunks[x + y * scsW + z * scsW * scsH];
                }

                void AddVert(Vector3 vert)
                {
                    vert.x /= w;
                    vert.y /= h;
                    vert.z /= d;
                    if (!vertInds.TryGetValue(vert, out int i))
                    {
                        i = verts.Count;
                        verts.Add(vert);
                        vertInds[vert] = i;
                    }
                    inds.Add(i);
                }

                void AddQuad(Vector3 pos, Vector3 up, Vector3 right)
                {
                    AddVert(pos);
                    AddVert(pos + up);
                    AddVert(pos + up + right);
                    AddVert(pos + right);
                }

                foreach (var quadDir in quadDirs)
                {
                    var normal = quadDir.normal;
                    var up = quadDir.up;
                    var right = quadDir.right;

                    var pos = new IntVector3();
                    for (pos.z = 0; pos.z < d; pos.z++)
                    {
                        int vscZOffset = pos.z * w * h;
                        for (pos.y = 0; pos.y < h; pos.y++)
                        {
                            int vscYOffset = pos.y * w;
                            for (pos.x = 0; pos.x < w; pos.x++)
                            {
                                // Skip air, occluded voxels, or voxels that are already assigned to a face
                                if (!IsSolid(pos)
                                    || IsSolid(pos + normal)
                                    || visitedSubchunks[pos.x + vscYOffset + vscZOffset]) continue;

                                IntVector3 bottomLeft = pos - up;
                                IntVector3 topRight = pos + up;

                                int width = 1;
                                int height = 1;

                                // Find the vertical line this voxel is a part of
                                while (IsSolid(bottomLeft)
                                    && !IsSolid(bottomLeft + normal)
                                    && !visitedSubchunks[bottomLeft.x + bottomLeft.y * w + bottomLeft.z * w * h])
                                {
                                    height++;
                                    bottomLeft -= up;
                                }
                                bottomLeft += up;

                                while (IsSolid(topRight)
                                    && !IsSolid(topRight + normal)
                                    && !visitedSubchunks[bottomLeft.x + topRight.y * w + topRight.z * w * h])
                                {
                                    height++;
                                    topRight += up;
                                }
                                topRight -= up;

                                // Expand the vertical line horizontally in both directions
                                bool lineSolid = true;
                                while(lineSolid)
                                {
                                    bottomLeft -= right;
                                    width++;
                                    for(int upOffset = 0; upOffset < height; upOffset++)
                                    {
                                        var scanPos = bottomLeft + up * upOffset;
                                        if (!IsSolid(scanPos)
                                            || IsSolid(scanPos + normal)
                                            || visitedSubchunks[scanPos.x + scanPos.y * w + scanPos.z * w * h])
                                        {
                                            lineSolid = false;
                                            break;
                                        }
                                    }
                                }
                                bottomLeft += right;
                                width--;

                                lineSolid = true;
                                while (lineSolid)
                                {
                                    topRight += right;
                                    width++;
                                    for (int upOffset = 0; upOffset < height; upOffset++)
                                    {
                                        var scanPos = topRight - up * upOffset;
                                        if (!IsSolid(scanPos)
                                            || IsSolid(scanPos + normal)
                                            || visitedSubchunks[scanPos.x + scanPos.y * w + scanPos.z * w * h])
                                        {
                                            lineSolid = false;
                                            break;
                                        }
                                    }
                                }
                                topRight -= right;
                                width--;

                                // Mark all subchunks as visited
                                for(int upOffset = 0; upOffset < height; upOffset++)
                                {
                                    for(int rightOffset = 0; rightOffset < width; rightOffset++)
                                    {
                                        var scanPos = bottomLeft + up * upOffset + right * rightOffset;
                                        visitedSubchunks[scanPos.x + scanPos.y * w + scanPos.z * w * h] = true;
                                    }
                                }

                                // Finally, actually add the quad
                                Vector3 center = bottomLeft + new Vector3(0.5f, 0.5f, 0.5f);
                                AddQuad(center + normal * 0.5f - up * 0.5f - right * 0.5f, up * height, right * width);

                                /*
                                if (!IsSolid(x - 1, y, z)) AddQuad(new Vector3(x, y, z), new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 0f));
                                if (!IsSolid(x + 1, y, z)) AddQuad(new Vector3(x + 1, y, z), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f));

                                if (!IsSolid(x, y - 1, z)) AddQuad(new Vector3(x, y, z), new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f));
                                if (!IsSolid(x, y + 1, z)) AddQuad(new Vector3(x, y + 1, z), new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 0f));

                                if (!IsSolid(x, y, z - 1)) AddQuad(new Vector3(x, y, z), new Vector3(0f, 1f, 0f), new Vector3(1f, 0f, 0f));
                                if (!IsSolid(x, y, z + 1)) AddQuad(new Vector3(x, y, z + 1), new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f));
                                */
                            }
                        }
                    }

                    Array.Clear(visitedSubchunks, 0, visitedSubchunks.Length);
                }

                Color[] colors = new Color[verts.Count];
                for (int i = colors.Length - 1; i >= 0; i--)
                {
                    Vector3 vert = verts[i];
                    colors[i] = new Color(Mathf.Clamp01(vert.x), Mathf.Clamp01(vert.y), Mathf.Clamp01(vert.z));
                }

                var mesh = new Mesh();
                mesh.vertices = verts.ToArray();
                mesh.colors = colors;
                mesh.SetIndices(inds.ToArray(), MeshTopology.Quads, 0);

                verts.Clear();
                inds.Clear();
                vertInds.Clear();

                Mesh = mesh;
            }

            private void DestroyMesh()
            {
                if (Mesh == null) return;

                TextureManager.EnqueueDeletion(Mesh);
            }

            private void CollapseBounds(ref int xMin, ref int yMin, ref int zMin, ref int xMax, ref int yMax, ref int zMax)
            {
                var voxels = Map.Voxels;

                int txMin = Math.Max(xMin, 0);
                int tyMin = Math.Max(yMin, 0);
                int tzMin = Math.Max(zMin, 0);
                int txMax = Math.Min(xMax, Map.XVoxels);
                int tyMax = Math.Min(yMax, Map.YVoxels);
                int tzMax = Math.Min(zMax, Map.ZVoxels);

                int xVoxels = Map.XVoxels;
                int yVoxels = Map.YVoxels;

                bool XSliceEquals(int srcX, int dstX)
                {
                    if (dstX < txMin || dstX >= txMax) return false;

                    int yScale = xVoxels;
                    for (int z = tzMin; z < tzMax; z++)
                    {
                        int zOffset = z * xVoxels * yVoxels;
                        for (int y = tyMin; y < tyMax; y++)
                        {
                            if (voxels[srcX + y * yScale + zOffset] != voxels[dstX + y * yScale + zOffset])
                                return false;
                        }
                    }

                    return true;
                }

                bool YSliceEquals(int srcY, int dstY)
                {
                    if (dstY < tyMin || dstY >= tyMax) return false;

                    int srcYOffset = srcY * xVoxels;
                    int dstYOffset = dstY * xVoxels;
                    for (int z = tzMin; z < tzMax; z++)
                    {
                        int zOffset = z * xVoxels * yVoxels;
                        for (int x = txMin; x < txMax; x++)
                        {
                            if (voxels[x + srcYOffset + zOffset] != voxels[x + dstYOffset + zOffset])
                                return false;
                        }
                    }

                    return true;
                }

                bool ZSliceEquals(int srcZ, int dstZ)
                {
                    if (dstZ < tzMin || dstZ >= tzMax) return false;

                    int srcZOffset = srcZ * xVoxels * yVoxels;
                    int dstZOffset = dstZ * xVoxels * yVoxels;
                    for (int y = tyMin; y < tyMax; y++)
                    {
                        int yOffset = y * xVoxels;
                        for (int x = txMin; x < txMax; x++)
                        {
                            if (voxels[x + yOffset + srcZOffset] != voxels[x + yOffset + dstZOffset])
                                return false;
                        }
                    }

                    return true;
                }

                // Collapse X
                while (XSliceEquals(txMin, txMin + 1)) txMin++;
                while (XSliceEquals(txMax - 1, txMax - 2)) txMax--;

                // Collapse y
                while (YSliceEquals(tyMin, tyMin + 1)) tyMin++;
                while (YSliceEquals(tyMax - 1, tyMax - 2)) tyMax--;

                // Collapse Z
                while (ZSliceEquals(tzMin, tzMin + 1)) tzMin++;
                while (ZSliceEquals(tzMax - 1, tzMax - 2)) tzMax--;

                // Ensure size is a power of 2 in each dimension
                void IncreaseToPowerOfTwo(ref int min, ref int max, int upperBound)
                {
                    max = Mathf.NextPowerOfTwo(max - min) + min;
                    int shift = Math.Min(0, upperBound - max);
                    min += shift;
                    max += shift;
                }

                IncreaseToPowerOfTwo(ref txMin, ref txMax, xMax);
                IncreaseToPowerOfTwo(ref tyMin, ref tyMax, yMax);
                IncreaseToPowerOfTwo(ref tzMin, ref tzMax, zMax);

                xMin = txMin;
                yMin = tyMin;
                zMin = tzMin;
                xMax = txMax;
                yMax = tyMax;
                zMax = tzMax;
            }

            public void Update()
            {
                if (Time.unscaledTime > lastViewed + Preferences.unloadDelay)
                    Quality = TextureQuality.Unloaded;
            }

            private void DestroyTexture()
            {
                if (Texture == null) return;

                TextureManager.EnqueueDeletion(Texture);
                Texture = null;
            }

            public enum TextureQuality
            {
                Unloaded,
                Low,
                High
            }

            ~VoxelChunk()
            {
                Quality = TextureQuality.Unloaded;
            }
        }

        private class QuadDirs
        {
            public IntVector3 normal;
            public IntVector3 up;
            public IntVector3 right;

            public QuadDirs(IntVector3 normal, IntVector3 up, IntVector3 right)
            {
                this.normal = normal;
                this.up = up;
                this.right = right;
            }
        }
    }
}
