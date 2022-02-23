using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoxelWorld
{
    // A sprite overlaid in front of the level that casts a shadow
    // This must be a separate GameObject so it can use a different layer than the rest of the stage
    internal class FLightCookieSprite : FGameObjectNode
    {
        private static Material mat;
        private static Mesh mesh;
        private MeshRenderer meshRenderer;
        private Texture2D texture;

        public FLightCookieSprite(Texture2D texture)
        {
            if (mat == null)
            {
                mat = new Material(Shaders.VoxelLightCookie);
            }

            if(mesh == null)
            {
                // Create a mesh with the sprite in the center, extruded by the sprite's size on each side
                mesh = new Mesh();
                mesh.vertices = new Vector3[]
                {
                    new Vector3(-1.5f, -1.5f),
                    new Vector3(-1.5f,  1.5f),
                    new Vector3( 1.5f,  1.5f),
                    new Vector3( 1.5f, -1.5f),
                };
                mesh.uv = new Vector2[]
                {
                    new Vector2(-1f, -1f),
                    new Vector2(-1f,  2f),
                    new Vector2( 2f,  2f),
                    new Vector2( 2f, -1f),
                };
                mesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);
            }

            this.texture = texture;

            var go = new GameObject();
            Init(go, true, true, true);
        }

        private static MaterialPropertyBlock mpb;
        public override void HandleAddedToStage()
        {
            if (gameObject != null)
                UnityEngine.Object.Destroy(gameObject);

            var go = new GameObject();
            meshRenderer = go.AddComponent<MeshRenderer>();
            Debug.Log("Added light cookie sprite!");

            meshRenderer.sharedMaterial = mat;

            if (mpb == null) mpb = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetTexture("_MainTex", texture);
            meshRenderer.SetPropertyBlock(mpb);
            Shader.SetGlobalTexture("_Bababooey", texture);

            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            gameObject = go;

            shouldDestroyOnRemoveFromStage = true;
            base.HandleAddedToStage();
        }

        public override void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
        {
            base.Redraw(shouldForceDirty, shouldUpdateDepth);

            // Futile forces this object's layer to match the stage's
            gameObject.layer = Preferences.lightCookieLayer;
            Vector3 pos = gameObject.transform.localPosition;
            pos.z = -Preferences.terrainThickness * Preferences.playLayerDepth - Preferences.lightCookieDistance;
            gameObject.transform.localPosition = pos;
        }
    }
}
