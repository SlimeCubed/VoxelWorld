using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelWorld
{
    public static class Preferences
    {
        // Technical
        public static bool viewRawImage = false;
        public static bool onlyLowQuality = false;
        public static int voxelSpriteLayer = 11;
        public static int chunkSize = 128;
        public static int subchunkSize = 8;

        // General rendering
        public static float playLayerDepth = 1f / 6f;
        public static float terrainThickness = chunkSize;
        public static int loadPerFrameLimit = 1;
        public static int unloadPerFrameLimit = 2;
        public static float preloadDistanceFactor = 0.5f;
        public static float forceLoadDistanceFactor = 0.25f;
        public static float unloadDelay = 3f;
        public static bool allowLowQuality = true;
        public static bool forceLoadOnScreenTransition = true;

        // Shadows
        public static int shadowMapSize = 1024;
        public static float shadowMapScale = 1f;
        public static float lightPenetration = 15f;

        // Camera
        public static ScrollingMode scrollMode = ScrollingMode.BacktrackRectangle;
        public static TrackingMode trackingMode = TrackingMode.Complex;
        public static float cameraDepth = 900f;
        public static float sunDistance = 20000f;

        // Scrolling
        public static float trackRectWidth = 300f;
        public static float trackRectHeight = 10f;
        public static float scrollSmoothTime = 0.1f;
        public static float changeTargetSmoothTime = 0.15f;
        public static float changeTargetDuration = 0.25f;
        public static float rectSpeedFactor = 0.75f;

        // Misc
        public static bool showDiagnostics = false;

        public enum ScrollingMode
        {
            Static,
            BacktrackRectangle
        }

        public enum TrackingMode
        {
            CenterOfMass,
            Complex
        }
    }
}
