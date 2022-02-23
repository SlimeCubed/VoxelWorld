using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Diagnostics;

using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace VoxelWorld
{
    internal static class Diag
    {

        public static void Enable()
        {
            On.RainWorld.Start += (orig, self) =>
            {
                orig(self);
                var go = Futile.instance.camera.transform.GetChild(0).gameObject;
                go.AddComponent<FrameTimer>();
                go.AddComponent<FPSMeter>();
                //new GameObject("Cam Timer", typeof(FrameTimer));
            };
        }

        public static void Log(string msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        private class FPSMeter : MonoBehaviour
        {
            private FLabel label;
            private float[] frameTimes = new float[16];
            private int i;

            public void Start()
            {
                label = new FLabel("font", "--")
                {
                    color = new Color(0.1f, 1f, 0.3f),
                    anchorX = 0f,
                    anchorY = 1f
                };
                Futile.stage.AddChild(label);
            }

            public void LateUpdate()
            {
                label.MoveToFront();
                label.x = 10.15f;
                label.y = Futile.screen.height - 10.15f;

                frameTimes[i++] = Time.deltaTime;
                i %= frameTimes.Length;

                label.text = Mathf.RoundToInt(1f / frameTimes.Average()).ToString();
                label.isVisible = Preferences.showFPS;
            }
        }

        private class FrameTimer : MonoBehaviour
        {
            private const int frameBuffer = 60;
            private const float barWidth = 3f;
            private const float barHeightScale = 2f;
            private const float perfectThreshold = 1000f / 60f;
            private const float acceptableThreshold = 1000f / 40f;

            private FContainer container;
            private TriangleMesh mspfIndicator;
            private FSprite perfectBorder;
            private FSprite acceptableBorder;
            private float[] msPerFrame;
            private int currentFrame = frameBuffer - 1;

            private Stopwatch sw;

            public void Start()
            {
                container = new FContainer();
                msPerFrame = new float[frameBuffer];

                var tris = new TriangleMesh.Triangle[frameBuffer * 2];
                for (int i = 0; i < frameBuffer; i++)
                {
                    int firstBot = i;
                    int firstTop = frameBuffer + 1 + i * 2;
                    tris[firstBot * 2 + 0] = new TriangleMesh.Triangle(firstBot, firstTop, firstBot + 1);
                    tris[firstBot * 2 + 1] = new TriangleMesh.Triangle(firstBot + 1, firstTop, firstTop + 1);
                }
                mspfIndicator = new TriangleMesh("Futile_White", tris, true);
                container.AddChild(mspfIndicator);

                container.AddChild(perfectBorder = new FSprite("pixel") { color = Color.yellow, anchorX = 0f, anchorY = 0f });
                container.AddChild(acceptableBorder = new FSprite("pixel") { color = Color.red, anchorX = 0f, anchorY = 0f });

                Futile.stage.AddChild(container);

                sw = new Stopwatch();
                //StartCoroutine(UpdateCoroutine());
            }

            public void Update()
            {
                if(!Preferences.showDiagnostics)
                {
                    container.isVisible = false;
                    return;
                }
                container.isVisible = true;

                for(int i = 0; i < frameBuffer; i++)
                {
                    float ms = msPerFrame[(frameBuffer + currentFrame - i) % frameBuffer];
                    Color c;
                    if(ms < perfectThreshold)
                        c = Color.green;
                    else if(ms < acceptableThreshold)
                        c = Color.yellow;
                    else
                        c = Color.red;

                    int firstBot = i;
                    int firstTop = frameBuffer + 1 + i * 2;
                    for (int o = 0; o <= 1; o++)
                    {
                        mspfIndicator.MoveVertice(firstBot + o, new Vector2((i + o) * barWidth, 0f));
                        mspfIndicator.MoveVertice(firstTop + o, new Vector2((i + o) * barWidth, barHeightScale * ms));
                        mspfIndicator.verticeColors[firstTop + o] = c;
                        mspfIndicator.verticeColors[firstBot + o] = c;
                    }
                }

                perfectBorder.x = 0f;
                perfectBorder.y = perfectThreshold * barHeightScale;
                perfectBorder.scaleX = barWidth * frameBuffer;

                acceptableBorder.x = 0f;
                acceptableBorder.y = acceptableThreshold * barHeightScale;
                acceptableBorder.scaleX = barWidth * frameBuffer;

                container.x = 20f;
                container.y = Futile.screen.height - 500f;
            }

            public void LateUpdate()
            {
                container.MoveToFront();
            }

            public void OnPreCull()
            {
                sw.Start();
            }

            public void OnRenderImage(RenderTexture src, RenderTexture dst)
            {
                sw.Stop();
                currentFrame = (currentFrame + 1) % frameBuffer;
                msPerFrame[currentFrame] = sw.ElapsedMilliseconds;
                Graphics.Blit(src, dst);
                sw.Reset();
            }

            private IEnumerator UpdateCoroutine()
            {
                var waitEof = new WaitForEndOfFrame();
                var sw = new Stopwatch();

                while (true)
                {
                    sw.Start();
                    yield return null;
                    yield return waitEof;
                    sw.Stop();
                    currentFrame = (currentFrame + 1) % frameBuffer;
                    msPerFrame[currentFrame] = sw.ElapsedMilliseconds;
                    sw.Reset();
                }
            }
        }
    }
}
