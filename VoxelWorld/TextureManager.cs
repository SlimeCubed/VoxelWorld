using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoxelWorld
{
    internal class TextureManager : MonoBehaviour
    {
        private static TextureManager instance;
        private static readonly Queue<Texture> deletionQueue = new Queue<Texture>();

        public static void EnqueueDeletion(Texture texture)
        {
            if (instance == null) Init();

            deletionQueue.Enqueue(texture);
        }

        public static void Init()
        {
            var go = new GameObject("Texture Manager");
            instance = go.AddComponent<TextureManager>();
        }

        public void Update()
        {
            for (int i = 0; i < Preferences.unloadPerFrameLimit && deletionQueue.Count > 0; i++)
                Destroy(deletionQueue.Dequeue());
        }
    }
}
