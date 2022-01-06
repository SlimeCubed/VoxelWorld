using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoxelWorld
{
    // Creates a 1x1x1 cube mesh with a 3D main texture
    internal class FVoxelChunkNode : FGameObjectNode
    {
        private MeshRenderer meshRenderer;
        private static Texture3D placeholderTex;
        private static MaterialPropertyBlock mpb;
        private static bool init;
        private static Material material;
        private static Mesh placeholderMesh;

        private Texture3D texture;
        private Bounds voxelBounds = new Bounds(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1f, 1f, 1f));
        private Mesh mesh;

        public Texture3D Texture
        {
            get => texture;
            set
            {
                if (value != texture)
                    UpdateTexture(value);
            }
        }

        public Bounds VoxelBounds
        {
            get => voxelBounds;
            set
            {
                if (value != voxelBounds)
                    UpdateBounds(value);
            }
        }

        public Mesh Mesh
        {
            get => mesh;
            set
            {
                if (value != mesh)
                    UpdateMesh(value);
            }
        }

        public FVoxelChunkNode(RainWorld rainWorld)
        {
            if (!init) InitVoxelRendering(rainWorld);

            var go = new GameObject();
            go.transform.localScale = new Vector3(1f, 1f, Preferences.terrainThickness);
            Init(go, true, true, true);
        }

        private void UpdateBounds(Bounds bounds)
        {
            voxelBounds = bounds;

            Vector3 sampleScale = bounds.size;
            sampleScale.x = 1f / sampleScale.x;
            sampleScale.y = 1f / sampleScale.y;
            sampleScale.z = 1f / sampleScale.z;

            Vector3 sampleOffset = -bounds.min;

            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetVector("_VoxelSampleOffset", sampleOffset);
            mpb.SetVector("_VoxelSampleScale", sampleScale);
            meshRenderer.SetPropertyBlock(mpb);
        }

        private void UpdateTexture(Texture3D texture)
        {
            this.texture = texture;

            meshRenderer.GetPropertyBlock(mpb);
            var tex = texture ?? placeholderTex;
            mpb.SetTexture("_MainTex", tex);
            mpb.SetVector("_VoxelChunkSize", new Vector4(VoxelMapView.VoxelChunk.size, VoxelMapView.VoxelChunk.size, VoxelMapView.VoxelChunk.depth, 0f));
            meshRenderer.SetPropertyBlock(mpb);
        }

        private void UpdateMesh(Mesh mesh)
        {
            this.mesh = mesh;

            var filter = gameObject?.GetComponent<MeshFilter>();
            if (filter != null)
                filter.sharedMesh = mesh ?? placeholderMesh;
        }

        public override void HandleAddedToStage()
        {
            if (gameObject != null)
                UnityEngine.Object.Destroy(gameObject);

            var go = new GameObject();
            meshRenderer = go.AddComponent<MeshRenderer>();
            UpdateTexture(Texture);
            UpdateBounds(VoxelBounds);

            meshRenderer.sharedMaterial = material;
            go.AddComponent<MeshFilter>().sharedMesh = mesh ?? placeholderMesh;
            gameObject = go;

            shouldDestroyOnRemoveFromStage = true;
            base.HandleAddedToStage();
        }

        public override void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
        {
            base.Redraw(shouldForceDirty, shouldUpdateDepth);

            // Futile forces this object's layer to match the stage's
            gameObject.layer = Preferences.voxelSpriteLayer;
            Vector3 pos = gameObject.transform.localPosition;
            pos.z = -Preferences.terrainThickness * Preferences.playLayerDepth;
            gameObject.transform.localPosition = pos;
        }

        private static void InitVoxelRendering(RainWorld rainWorld)
        {
            init = true;

            placeholderTex = new Texture3D(1, 1, 1, TextureFormat.Alpha8, false);
            placeholderTex.name = "Placeholder";
            placeholderTex.SetPixels(new Color[] { new Color(0f, 0f, 0f, 0f) });
            placeholderTex.Apply();

            mpb = new MaterialPropertyBlock();

            material = new Material(rainWorld.Shaders["VoxelChunk"].shader);

            placeholderMesh = new Mesh();

            placeholderMesh.vertices = new Vector3[]
            {
                new Vector3(0f, 0f, 0f), // 0
                new Vector3(1f, 0f, 0f), // 1
                new Vector3(0f, 1f, 0f), // 2
                new Vector3(1f, 1f, 0f), // 3
                new Vector3(0f, 0f, 1f), // 4
                new Vector3(1f, 0f, 1f), // 5
                new Vector3(0f, 1f, 1f), // 6
                new Vector3(1f, 1f, 1f), // 7
            };

            placeholderMesh.colors = new Color[]
            {
                new Color(0f, 0f, 0f),
                new Color(1f, 0f, 0f),
                new Color(0f, 1f, 0f),
                new Color(1f, 1f, 0f),
                new Color(0f, 0f, 1f),
                new Color(1f, 0f, 1f),
                new Color(0f, 1f, 1f),
                new Color(1f, 1f, 1f),
            };

            placeholderMesh.SetIndices(new int[]
            {
                0, 2, 3, 1, // Front
                2, 6, 7, 3, // Top
                1, 3, 7, 5, // Right
                0, 1, 5, 4, // Bottom
                0, 4, 6, 2, // Left
                4, 5, 7, 6, // Back
            }, MeshTopology.Quads, 0);

            placeholderMesh.bounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
        }
    }
}
