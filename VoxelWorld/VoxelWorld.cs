using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using UnityEngine;
using RWCustom;

namespace VoxelWorld
{
    [BepInPlugin("com.slime-cubed.voxelworld", "Voxel World", "1.0.0")]
    public partial class VoxelWorld : BaseUnityPlugin
    {
        readonly static Dictionary<Room, VoxelMap> voxelData = new Dictionary<Room, VoxelMap>();
        readonly static List<string> threadedLogs = new List<string>();
        readonly static List<Exception> threadedExceptions = new List<Exception>();

        public void OnEnable()
        {
            try
            {
                StartRenderThread();
                VramCommand.TryRegister();
                ShutdownCommand.TryRegister();

                On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
                On.AbstractRoom.Abstractize += AbstractRoom_Abstractize;
                On.Room.NowViewed += Room_NowViewed;
                On.Room.Update += Room_Update;
                On.Room.Loaded += Room_Loaded;
                On.RainWorld.Start += RainWorld_Start;

                Rendering.Enable();
                CameraScroll.Enable();
                Diag.Enable();
            }
            catch(Exception e)
            {
                On.RainWorld.Start += (orig, self) =>
                {
                    Debug.Log(e);
                };
            }
        }

        private void OnApplicationQuit()
        {
#warning Killing the process probably isn't a great solution.
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            ShutdownRenderThread();
        }

        public void Update()
        {
            // Apply late to overwrite Sharpener's shaders
            ShaderFixes.Apply();
            
            FlushLogs();
        }

        private void FlushLogs()
        {
            // The default log handler ignores messages from other threads
            lock (threadedLogs)
            {
                foreach(var tl in threadedLogs)
                {
                    Debug.Log("Threaded: " + tl);
                }
                threadedLogs.Clear();
            }
            lock(threadedExceptions)
            {
                foreach(var te in threadedExceptions)
                {
                    Debug.LogException(te);
                }
                threadedExceptions.Clear();
            }

            unsafe
            {
                char* renderLog;
                while ((renderLog = LogFetch()) != null)
                {
                    var logStr = new string(renderLog);
                    Logger.LogDebug("Native: " + logStr);
                }
            }
        }
        
        public static string GetRoomFilePath(string name, string suffix)
        {
            return $"{Custom.RootFolderDirectory()}Voxels/{name.Split(new string[] { "_" }, StringSplitOptions.None)[0]}/{name}{suffix}".Replace('/', Path.DirectorySeparatorChar);
        }

        public static void LogThreaded(string msg)
        {
            lock(threadedLogs)
                threadedLogs.Add(msg);
        }

        public static void LogThreaded(Exception exception)
        {
            lock (threadedExceptions)
                threadedExceptions.Add(exception);
        }

        internal static VoxelMap GetVoxelMap(Room room) => room != null && voxelData.TryGetValue(room, out var map) ? map : null;

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
                voxelData.Remove(self.realizedRoom);
            }
            orig(self);
        }

        private void Room_NowViewed(On.Room.orig_NowViewed orig, Room self)
        {
            if (voxelData.TryGetValue(self, out var map))
            {
                if (!self.drawableObjects.Any(o => o is VoxelMapView))
                    self.AddObject(new VoxelMapView(map));
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
            }
        }

        private void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            orig(self);

            CustomAtlases.FetchAtlas("Sprites");

            self.Shaders["VoxelChunk"] = FShader.CreateShader("VoxelChunk", new Material(Shaders.VoxelChunk).shader);
            self.Shaders["VoxelDepth"] = FShader.CreateShader("VoxelDepth", new Material(Shaders.VoxelDepth).shader);
        }
    }
}
