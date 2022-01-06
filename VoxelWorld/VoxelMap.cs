using System;
using System.Threading;
using System.IO;
using RWCustom;
using UnityEngine;

namespace VoxelWorld
{
    // Holds voxel data for a room
    internal class VoxelMap
    {
        private byte[] voxels;
        private bool[] subchunks;
        private bool loadingVoxels;
        private readonly object syncRoot = new object();

        private int xVoxels;
        private int yVoxels;
        private int zVoxels;
        private int xSubchunks;
        private int ySubchunks;
        private int zSubchunks;
        private IntVector2 displayOffset;

        public readonly Room room;

        public string FilePath { get; }
        public byte[] Voxels
        {
            get
            {
                WaitForVoxels();
                return voxels;
            }
        }
        public int XVoxels
        {
            get
            {
                WaitForVoxels();
                return xVoxels;
            }
        }
        public int YVoxels
        {
            get
            {
                WaitForVoxels();
                return yVoxels;
            }
        }
        public int ZVoxels
        {
            get
            {
                WaitForVoxels();
                return zVoxels;
            }
        }
        public IntVector2 DisplayOffset
        {
            get
            {
                WaitForVoxels();
                return displayOffset;
            }
        }
        public bool HasVoxelMap => loadingVoxels || voxels != null;

        public bool[] Subchunks
        {
            get
            {
                WaitForVoxels();
                return subchunks;
            }
        }
        public int XSubchunks
        {
            get
            {
                WaitForVoxels();
                return xSubchunks;
            }
        }
        public int YSubchunks
        {
            get
            {
                WaitForVoxels();
                return ySubchunks;
            }
        }
        public int ZSubchunks
        {
            get
            {
                WaitForVoxels();
                return zSubchunks;
            }
        }

        public VoxelMap(Room room)
        {
            Debug.Log($"Created voxel map: {room.abstractRoom.name}");

            this.room = room;
            string roomName = room.abstractRoom.name;
            FilePath = $"{Custom.RootFolderDirectory()}Voxels/{roomName.Split(new string[] { "_" }, StringSplitOptions.None)[0]}/{roomName}_Voxels.vx1".Replace('/', Path.DirectorySeparatorChar);
            if (File.Exists(FilePath))
            {
                loadingVoxels = true;
                ThreadPool.QueueUserWorkItem(state => LoadVoxels(), null);
            }
        }

        private void WaitForVoxels()
        {
            while (loadingVoxels)
                Thread.Sleep(0);
        }

        public void LoadVoxels()
        {
            byte[] outVoxels = null;
            bool[] outSubchunks = null;
            try
            {
                using (var br = new BinaryReader(new MemoryStream(File.ReadAllBytes(FilePath))))
                {
                    int width = xVoxels = br.ReadUInt16();
                    int height = yVoxels = br.ReadUInt16();
                    int depth = zVoxels = br.ReadUInt16();

                    displayOffset = new IntVector2(
                        br.ReadInt16(),
                        br.ReadInt16()
                    );

                    int repeatCount = 0;
                    byte repeatVoxel = 0;

                    int scSize = Preferences.subchunkSize;

                    int scsWidth = xSubchunks = (width + scSize - 1) / scSize;
                    int scsHeight = ySubchunks = (height + scSize - 1) / scSize;
                    int scsDepth = zSubchunks = (depth + scSize - 1) / scSize;

                    // Load voxels
                    outVoxels = new byte[width * height * depth];
                    outSubchunks = new bool[scsWidth * scsHeight * scsDepth];
                    for (int z = 0; z < depth; z++)
                    {
                        int zOffset = z * width * height;
                        int scZOffset = z / scSize * scsWidth * scsHeight;

                        for (int y = 0; y < height; y++)
                        {
                            int yOffset = y * width;
                            int scYOffset = y / scSize * scsWidth;

                            for (int x = 0; x < width; x++)
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

                                if(voxel != 0)
                                {
                                    outSubchunks[x / scSize + scYOffset + scZOffset] = true;
                                }

                                outVoxels[x + yOffset + zOffset] = voxel == 0 ? voxel : (byte)(voxel | 0b11000000);
                            }
                        }

                        if (!loadingVoxels) break;
                    }
                }

                VoxelWorld.LogThreaded($"Successfully loaded voxel map: {room.abstractRoom.name}");
            }
            catch (System.IO.IsolatedStorage.IsolatedStorageException) { }
            catch (Exception e)
            {
                VoxelWorld.LogThreaded(new Exception("Failed to get voxel map!", e));
                voxels = null;
                subchunks = null;
                xVoxels = 0;
                yVoxels = 0;
                zVoxels = 0;
                xSubchunks = 0;
                ySubchunks = 0;
                zSubchunks = 0;
            }

            lock (syncRoot)
            {
                if (loadingVoxels)
                {
                    loadingVoxels = false;
                    voxels = outVoxels;
                    subchunks = outSubchunks;
                }
            }
        }

        public int GetDepth(int x, int y)
        {
            x = Mathf.Clamp(x, 0, XVoxels - 1);
            y = Mathf.Clamp(y, 0, YVoxels - 1);

            int z = 0;
            while (z < ZVoxels && Voxels[x + y * XVoxels + z * XVoxels * YVoxels] == 0)
                z++;
            return z;
        }

        public bool IsSolid(int x, int y, int z)
        {
            x = Mathf.Clamp(x, 0, XVoxels - 1);
            y = Mathf.Clamp(y, 0, YVoxels - 1);
            z = Mathf.Clamp(z, 0, ZVoxels - 1);
            return GetPaletteColor(Voxels[x + y * XVoxels + z * XVoxels * YVoxels]) > 0;
        }

        public static int GetPaletteColor(byte voxel) => voxel & 0x3;

        public void Update()
        {
        }

        public void Unload()
        {
            lock (syncRoot)
            {
                voxels = null;
            }
        }
    }
}
