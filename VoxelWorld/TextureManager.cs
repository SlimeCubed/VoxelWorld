using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace VoxelWorld
{
    internal class TextureManager : MonoBehaviour
    {
        private static TextureManager instance;
        private static readonly Queue<UnityObject> deletionQueue = new Queue<UnityObject>();
        private static readonly Queue<Action> actionQueue = new Queue<Action>();

        public static void EnqueueDeletion(UnityObject texture)
        {
            if (instance == null) Init();

            lock (deletionQueue)
            {
                deletionQueue.Enqueue(texture);
            }
        }

        public static void EnqueueAction(Action action)
        {
            lock(actionQueue)
            {
                actionQueue.Enqueue(action);
            }
        }

        public static void Init()
        {
            var go = new GameObject("Texture Manager");
            instance = go.AddComponent<TextureManager>();
        }

        public void Update()
        {
            lock (deletionQueue)
            {
                for (int i = 0; i < Preferences.unloadPerFrameLimit && deletionQueue.Count > 0; i++)
                    Destroy(deletionQueue.Dequeue());
            }
            lock(actionQueue)
            {
                for (int i = 0; i < Preferences.unloadPerFrameLimit && actionQueue.Count > 0; i++)
                    actionQueue.Dequeue()();
            }
        }
    }
}
