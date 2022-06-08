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
        public static FrameTimer Timer { get; private set; }

        public static void Enable()
        {
            On.RainWorld.Start += (orig, self) =>
            {
                orig(self);
                var go = Futile.instance.camera.transform.GetChild(0).gameObject;
                Timer = go.AddComponent<FrameTimer>();
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

                frameTimes[i++] = Time.unscaledDeltaTime;
                i %= frameTimes.Length;

                label.text = Mathf.RoundToInt(1f / frameTimes.Average()).ToString();
                label.isVisible = Preferences.showFPS;
            }
        }

        public class FrameTimer : MonoBehaviour
        {
            public enum Task
            {
                Voxels,
                Shadows,
                Mesh,
                VoxelRead,
                Untracked
            }

            private static readonly Color[] segColors = new Color[]
            {
                Color.red,
                Color.blue,
                Color.green,
                Color.yellow,
                Color.gray
            };

            private static int Segs => segColors.Length;

            private const int frameBuffer = 60;
            private const float barWidth = 3f;
            private const float barHeightScale = 2f;
            private const float perfectThreshold = 1000f / 60f;
            private const float acceptableThreshold = 1000f / 40f;

            private FContainer container;
            private TriangleMesh timeIndicator;
            private FSprite perfectBorder;
            private FSprite acceptableBorder;
            private float[][] frameTimes;
            private int currentFrame = frameBuffer - 1;
            private Stopwatch totalTimer;
            private double lastFrameEnd;

            private double timerTime = -1.0;
            private Task timerTask;

            public void AddTime(Task task, float ms)
            {
                frameTimes[currentFrame][(int)task] += ms;
            }

            public void StartTimer(Task task)
            {
                if (timerTime != -1.0) throw new InvalidOperationException($"Another timer is already running: {timerTask}");

                timerTask = task;
                timerTime = totalTimer.Elapsed.TotalMilliseconds;
            }

            public void StopTimer()
            {
                AddTime(timerTask, (float)(totalTimer.Elapsed.TotalMilliseconds - timerTime));
                timerTime = -1.0;
            }

            public void Start()
            {
                container = new FContainer();
                frameTimes = new float[frameBuffer][];
                for (int i = 0; i < frameBuffer; i++)
                    frameTimes[i] = new float[Segs];

                var tris = new TriangleMesh.Triangle[frameBuffer * 2 * Segs];
                for (int i = 0; i < frameBuffer * Segs; i++)
                {
                    int o = i * 4;
                    tris[i * 2 + 0] = new TriangleMesh.Triangle(o, o + 1, o + 2);
                    tris[i * 2 + 1] = new TriangleMesh.Triangle(o + 1, o + 2, o + 3);
                }
                timeIndicator = new TriangleMesh("Futile_White", tris, true);
                container.AddChild(timeIndicator);

                container.AddChild(perfectBorder = new FSprite("pixel") { color = Color.yellow, anchorX = 0f, anchorY = 0f });
                container.AddChild(acceptableBorder = new FSprite("pixel") { color = Color.red, anchorX = 0f, anchorY = 0f });

                Futile.stage.AddChild(container);

                totalTimer = Stopwatch.StartNew();
            }

            public void LateUpdate()
            {
                if (!Preferences.showDiagnostics)
                {
                    container.isVisible = false;
                    return;
                }
                container.isVisible = true;

                // Update current frame
                var currentTimes = frameTimes[currentFrame];
                AddTime(Task.Untracked, Math.Max((float)(totalTimer.Elapsed.TotalMilliseconds - lastFrameEnd) - currentTimes.Sum(), 0f));
                lastFrameEnd = totalTimer.Elapsed.TotalMilliseconds;

                // Draw bars
                for (int frame = 0; frame < frameBuffer; frame++)
                {
                    float yMin = 0f;
                    float yMax = 0f;
                    var times = frameTimes[(frameBuffer + currentFrame - frame) % frameBuffer];
                    
                    for (int seg = 0; seg < Segs; seg++)
                    {
                        yMax += times[seg] * barHeightScale;
                        Color c = segColors[seg];

                        for (int x = 0; x <= 1; x++)
                        {
                            int o = (frame * Segs + seg) * 4 + x * 2;
                            timeIndicator.MoveVertice(o + 0, new Vector2((frame + x) * barWidth, yMin));
                            timeIndicator.MoveVertice(o + 1, new Vector2((frame + x) * barWidth, yMax));
                            timeIndicator.verticeColors[o + 0] = c;
                            timeIndicator.verticeColors[o + 1] = c;
                        }

                        yMin = yMax;
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

                currentFrame = (currentFrame + 1) % frameBuffer;

                // Clear new frame
                Array.Clear(frameTimes[currentFrame], 0, Segs);

                container.MoveToFront();
            }
        }
    }
}
