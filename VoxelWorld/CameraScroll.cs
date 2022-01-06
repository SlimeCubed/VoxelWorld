using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;

namespace VoxelWorld
{
    internal static class CameraScroll
    {
        private static readonly Dictionary<RoomCamera, CameraController> controllers = new Dictionary<RoomCamera, CameraController>();

        public static void Enable()
        {
            // Create a scroll controller for each new camera
            On.RoomCamera.ctor += (orig, self, game, cameraNumber) =>
            {
                orig(self, game, cameraNumber);

                if (Preferences.scrollMode != Preferences.ScrollingMode.Static)
                    controllers[self] = new CameraController(self);
            };

            // Intercept camera positioning logic to instead use the scrolling camera's position
            IL.RoomCamera.DrawUpdate += il =>
            {
                var c = new ILCursor(il);
                ILLabel skipClamp = c.DefineLabel();

                // Remove camera clamping
                c.GotoNext(MoveType.After,
                        x => x.MatchLdfld<RoomCamera>(nameof(RoomCamera.voidSeaMode)),
                        x => x.MatchBrtrue(out _));

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<RoomCamera, bool>>(cam =>
                {
                    return controllers.TryGetValue(cam, out var controller) && controller.Active;
                });
                c.Emit(OpCodes.Brtrue, skipClamp);

                c.GotoNext(MoveType.Before,
                    x => x.Match(OpCodes.Ldarg_0),
                    x => x.MatchLdfld<RoomCamera>(nameof(RoomCamera.levelGraphic)),
                    x => x.Match(OpCodes.Ldc_I4_1),
                    x => x.MatchCallvirt<FNode>("set_isVisible"));
                c.MarkLabel(skipClamp);

                // Apply fading palette
                c.GotoNext(MoveType.After,
                    x => x.Match(OpCodes.Ldloc_0),
                    x => x.Match(OpCodes.Ldarg_0),
                    x => x.MatchLdfld<RoomCamera>(nameof(RoomCamera.offset)),
                    x => x.MatchCall<Vector2>("op_Addition"),
                    x => x.Match(OpCodes.Stloc_0));
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Action<RoomCamera, Vector2>>((cam, camPos) =>
                {
                    if (cam.room.roomSettings.fadePalette != null
                        && controllers.TryGetValue(cam, out var controller)
                        && controller.Active)
                    {
                        float newBlend = controller.GetPaletteBlend(camPos);
                        if (newBlend != cam.paletteBlend)
                        {
                            cam.paletteBlend = newBlend;
                            cam.ApplyFade();
                            cam.lastFadeCoord = cam.fadeCoord;
                        }
                    }
                });

                // Change _spriteRect calculations
                c.Index = -1;
                c.MoveAfterLabels();
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Action<RoomCamera, Vector2>>((cam, camPos) =>
                {
                    if (!controllers.TryGetValue(cam, out var controller) || !controller.Active) return;
                        
                    Shader.SetGlobalVector("_camPos", new Vector4(camPos.x, camPos.y, camPos.x / cam.levelGraphic.width, camPos.y / cam.levelGraphic.height));

                    cam.levelGraphic.x = (cam.sSize.x - cam.levelGraphic.width) / 2f;
                    cam.levelGraphic.y = (cam.sSize.y - cam.levelGraphic.height) / 2f;

                    Shader.SetGlobalVector("_spriteRect", new Vector4(
                        (cam.levelGraphic.x - 0.5f) / cam.sSize.x,
                        (cam.levelGraphic.y + 0.5f) / cam.sSize.y,
                        (cam.levelGraphic.x - 0.5f + cam.levelGraphic.width) / cam.sSize.x,
                        (cam.levelGraphic.y + 0.5f + cam.levelGraphic.height) / cam.sSize.y
                    ));
                });
            };

            // Replace the camera's position with the scrolling position
            On.RoomCamera.Update += (orig, self) =>
            {
                orig(self);
                if (controllers.TryGetValue(self, out var controller) && controller.Active)
                {
                    controller.Update();
                    self.pos = controller.DrawPos(1f);
                }
            };

            // Forward ChangeRoom to the scroll controller
            On.RoomCamera.ChangeRoom += (orig, self, newRoom, cameraPosition) =>
            {
                if (controllers.TryGetValue(self, out var controller))
                    controller.NewRoom(newRoom, cameraPosition);

                orig(self, newRoom, cameraPosition);
            };

            // Change depth logic to apply from the center of the camera
            On.RoomCamera.ApplyDepth += (orig, self, ps, depth) =>
            {
                if (!controllers.TryGetValue(self, out var controller) || !controller.Active)
                    return orig(self, ps, depth);
                else
                    return Custom.ApplyDepthOnVector(ps, Vector2.Lerp(self.lastPos, self.pos, self.game.myTimeStacker) + new Vector2(700f, 533.3334f), depth);
            };

            // Don't load the level image when not required
            On.RoomCamera.ApplyPositionChange += (orig, self) =>
            {
                if (controllers.TryGetValue(self, out var controller) && controller.ActiveInRoom(self.loadingRoom ?? self.room))
                {
                    self.www = null;
                    self.quenedTexture = string.Empty;
                    if (self.loadingRoom != null)
                    {
                        self.ChangeRoom(self.loadingRoom, self.loadingCameraPos);
                    }
                    self.mostLikelyNextCamPos = self.currentCameraPosition;
                    self.currentCameraPosition = self.loadingCameraPos;
                    if (self.room.roomSettings.fadePalette != null)
                    {
                        self.paletteBlend = self.room.roomSettings.fadePalette.fades[self.currentCameraPosition];
                        self.ApplyFade();
                    }
                    self.seekPos = self.CamPos(self.currentCameraPosition);
                    self.seekPos.x = self.seekPos.x + (self.hDisplace + 8f);
                    self.seekPos.y = self.seekPos.y + 18f;
                    self.leanPos *= 0f;
                    self.loadingRoom = null;
                    self.loadingCameraPos = -1;
                    self.applyPosChangeWhenTextureIsLoaded = false;
                    self.UpdateGhostMode(self.room, self.currentCameraPosition);
                    self.ApplyPalette();
                }
                else
                    orig(self);
            };
        }

        private class CameraController
        {
            public Rect range;

            private readonly RoomCamera cam;
            private Vector2 targetPos;
            private Vector2 pos;
            private Vector2 lastPos;
            private Vector2 vel;
            private bool snap;
            private readonly CameraTarget[] targets;
            private readonly CameraPositioner positioner;
            private int currentTarget = -1;
            private float targetSwapTimer = 0f;

            public bool Active => ActiveInRoom(cam.room);
            public bool Debug => false;
            private readonly FContainer debugContainer;
            private FSprite debugSprite;

            public CameraController(RoomCamera cam)
            {
                this.cam = cam;

                debugContainer = new FContainer();

                // Camera targets, sorted from high to low priority
                switch (Preferences.trackingMode)
                {
                    default:
                    case Preferences.TrackingMode.Complex:
                        targets = new CameraTarget[]
                        {
                            new ShortcutTarget(this),
                            new PoleTarget(this),
                            new GroundedTarget(this),
                            new CenterOfMassTarget(this)
                        };
                        break;

                    case Preferences.TrackingMode.CenterOfMass:
                        targets = new CameraTarget[]
                        {
                            new ShortcutTarget(this),
                            new CenterOfMassTarget(this)
                        };
                        break;
                }

                switch(Preferences.scrollMode)
                {
                    default:
                    case Preferences.ScrollingMode.CenteredRectangle:
                        positioner = new CenteredRectPositioner(this);
                        break;

                    case Preferences.ScrollingMode.BacktrackRectangle:
                        positioner = new BacktrackRectPositioner(this);
                        break;
                }
            }

            public bool ActiveInRoom(Room room)
            {
                return VoxelWorld.GetVoxelMap(room)?.HasVoxelMap ?? false;
            }

            public void NewRoom(Room room, int cameraPos)
            {
                range = new Rect(room.cameraPositions[0].x, room.cameraPositions[0].y, 0f, 0f);
                foreach(var pos in room.cameraPositions)
                {
                    range.xMin = Mathf.Min(range.xMin, pos.x);
                    range.yMin = Mathf.Min(range.yMin, pos.y);
                    range.xMax = Mathf.Max(range.xMax, pos.x);
                    range.yMax = Mathf.Max(range.yMax, pos.y);
                }

                pos = room.cameraPositions[cameraPos];
                lastPos = pos;
                snap = true;
                positioner.Reset();
            }

            public Vector2 DrawPos(float timeStacker)
            {
                return Vector2.Lerp(lastPos, pos, timeStacker);
            }

            public float GetPaletteBlend(Vector2 pos)
            {
                float blendFactor = 0f;
                float weightSum = 0f;

                if (cam.room.roomSettings.fadePalette == null) return 0f;

                float OverlapArea(Rect a, Rect b)
                {
                    float OverlapDistance(float minA, float maxA, float minB, float maxB)
                    {
                        // Fully overlapping
                        if (minB >= minA && maxB <= maxA) return maxB - minB;
                        if (minA >= minB && maxA <= maxB) return maxA - minA;

                        // Partially overlapping
                        if (minB >= minA && minB <= maxA) return maxA - minB;
                        if (minA >= minB && minA <= maxB) return maxB - minA;

                        // Not overlapping
                        return 0f;
                    }

                    return OverlapDistance(a.xMin, a.xMax, b.xMin, b.xMax)
                         * OverlapDistance(a.yMin, a.yMax, b.yMin, b.yMax);
                }

                var view = new Rect(pos.x, pos.y, cam.sSize.x, cam.sSize.y);
                for(int i = 0; i < cam.room.cameraPositions.Length; i++)
                {
                    var camPos = cam.room.cameraPositions[i];
                    var fadeCam = new Rect(camPos.x, camPos.y, cam.sSize.x, cam.sSize.y);

                    float weight = OverlapArea(view, fadeCam);
                    blendFactor += weight * cam.room.roomSettings.fadePalette.fades[i];
                    weightSum += weight;
                }

                if (weightSum < 1f)
                {
                    blendFactor = cam.room.roomSettings.fadePalette.fades[cam.currentCameraPosition];
                    weightSum = 1f;
                }

                blendFactor /= weightSum;

                return blendFactor;
            }

            public void Update()
            {
                lastPos = pos;

                Vector2? newTargetPos = null;

                // Pick a target to follow
                if (cam.followAbstractCreature?.realizedObject is Player player)
                {
                    for(int i = 0; i < targets.Length; i++)
                    {
                        var target = targets[i];
                        if ((player.room != null || !target.NeedsRoom) && target.UpdateCenter(player) is Vector2 newTarget)
                        {
                            if (currentTarget != i && currentTarget > 0)
                            {
                                targets[currentTarget].Reset();
                                targetSwapTimer = 1f;
                            }

                            currentTarget = i;
                            target.Update();
                            newTargetPos = newTarget;
                            break;
                        }
                    }
                }
                else if(cam.followAbstractCreature?.realizedObject is Creature followCrit)
                {
                    newTargetPos = followCrit.mainBodyChunk.pos;
                }

                if (newTargetPos == null)
                    newTargetPos = cam.pos;

                newTargetPos = range.GetClosestInteriorPoint(newTargetPos.Value - new Vector2(Futile.screen.halfWidth, Futile.screen.halfHeight));

                if (snap)
                {
                    targetPos = newTargetPos.Value;
                    pos = targetPos;
                    lastPos = targetPos;
                    snap = false;
                }
                else
                {
                    float smoothTime = Mathf.Lerp(Preferences.scrollSmoothTime, Preferences.changeTargetSmoothTime, targetSwapTimer);
                    targetPos = Vector2.SmoothDamp(targetPos, newTargetPos.Value, ref vel, smoothTime, float.PositiveInfinity, 1f / 40f);

                    targetSwapTimer = Mathf.Clamp01(targetSwapTimer - 1f / 40f / Preferences.changeTargetDuration);
                }

                positioner.UpdatePosition(ref pos, targetPos);

                // Ensure that the camera can't go out of range
                pos = range.GetClosestInteriorPoint(pos);

                // Debugging!
                if (Debug)
                {
                    if (debugSprite == null)
                    {
                        debugSprite = new FSprite("Circle4")
                        {
                            color = Color.red
                        };
                        debugContainer.AddChild(debugSprite);
                    }

                    debugSprite.SetPosition(targetPos);
                }
                else if (debugSprite != null)
                {
                    debugSprite.RemoveFromContainer();
                    debugSprite = null;
                }

                if(Debug)
                {
                    cam.ReturnFContainer("HUD2").AddChild(debugContainer);
                    debugContainer.MoveToFront();
                    debugContainer.SetPosition(-pos + cam.sSize / 2f);
                }
            }

            // Chooses how the camera moves towards the focal point
            private abstract class CameraPositioner
            {
                protected readonly CameraController owner;

                public CameraPositioner(CameraController owner)
                {
                    this.owner = owner;
                }

                public abstract void UpdatePosition(ref Vector2 lastPos, Vector2 target);

                public virtual void Reset() { }
            }

            // Keep the target inside of a rectangle centered on the screen
            private class CenteredRectPositioner : CameraPositioner
            {
                public CenteredRectPositioner(CameraController owner) : base(owner) { }

                public override void UpdatePosition(ref Vector2 pos, Vector2 target)
                {
                    var trackRect = new Rect(pos.x - Preferences.trackRectWidth / 2f, pos.y - Preferences.trackRectWidth / 2f, Preferences.trackRectWidth, Preferences.trackRectHeight);
                    pos += target - trackRect.GetClosestInteriorPoint(target);
                }
            }

            // Keep the target inside of a rectangle behind the target
            private class BacktrackRectPositioner : CameraPositioner
            {
                private Vector2 offset;
                private Vector2 offsetVel;
                private Vector2? lastTarget;
                private FSprite debugSprite;

                public BacktrackRectPositioner(CameraController owner) : base(owner) { }

                public override void UpdatePosition(ref Vector2 pos, Vector2 target)
                {
                    float w = Preferences.trackRectWidth;
                    float h = Preferences.trackRectHeight;

                    if (lastTarget == null)
                        lastTarget = target;

                    void ApplyBacktrack(float x, float dx, ref float rectCenterX, ref float rectVelX, float rectW)
                    {
                        rectCenterX = Mathf.Clamp(rectCenterX - dx * Preferences.rectSpeedFactor, -rectW / 2f, rectW / 2f);
                    }

                    Vector2 targetDelta = target - lastTarget.Value;

                    ApplyBacktrack(target.x, targetDelta.x, ref offset.x, ref offsetVel.x, w);
                    ApplyBacktrack(target.y, targetDelta.y, ref offset.y, ref offsetVel.y, h);

                    var trackRect = new Rect(pos.x - w / 2f + offset.x, pos.y - h / 2f + offset.y, w, h);

                    pos += target - trackRect.GetClosestInteriorPoint(target);
                    lastTarget = target;

                    if(owner.Debug)
                    {
                        if (debugSprite == null)
                        {
                            debugSprite = new FSprite("pixel")
                            {
                                color = Color.blue,
                                alpha = 0.25f,
                                anchorX = 0f,
                                anchorY = 0f
                            };
                            owner.debugContainer.AddChild(debugSprite);
                        }

                        debugSprite.SetPosition(trackRect.min);
                        debugSprite.scaleX = trackRect.width;
                        debugSprite.scaleY = trackRect.height;
                    }
                    else if(debugSprite != null)
                    {
                        debugSprite.RemoveFromContainer();
                        debugSprite = null;
                    }
                }

                public override void Reset()
                {
                    lastTarget = null;
                    offsetVel = new Vector2();
                    offset = new Vector2();
                }
            }


            // Picks a point to focus on
            private abstract class CameraTarget
            {
                protected readonly CameraController owner;

                public virtual bool NeedsRoom => true;

                public CameraTarget(CameraController owner)
                {
                    this.owner = owner;
                }

                public abstract Vector2? UpdateCenter(Player ply);

                public virtual void Reset() { }
                public virtual void Update() { }
            }

            // Lock movement to one axis when on poles
            private class PoleTarget : CameraTarget
            {
                public PoleTarget(CameraController owner) : base(owner) { }

                public override Vector2? UpdateCenter(Player ply)
                {
                    float dist = ply.bodyChunkConnections[0].distance;
                    Vector2 head = ply.bodyChunks[0].pos;
                    Vector2 tail = ply.bodyChunks[1].pos;

                    switch (ply.animation)
                    {
                        case Player.AnimationIndex.ClimbOnBeam:
                            return new Vector2(
                                ply.room.MiddleOfTile(head).x,
                                head.y - dist / 2f
                            );

                        case Player.AnimationIndex.HangUnderVerticalBeam:
                            return new Vector2(
                                ply.room.MiddleOfTile(head).x,
                                ply.room.MiddleOfTile(head).y - dist / 2f
                            );

                        case Player.AnimationIndex.GetUpOnBeam:
                            return new Vector2(
                                tail.x,
                                ply.room.MiddleOfTile(head).y - dist / 2f
                            );

                        case Player.AnimationIndex.HangFromBeam:
                            return new Vector2(
                                head.x,
                                ply.room.MiddleOfTile(head).y - dist / 2f
                            );

                        case Player.AnimationIndex.StandOnBeam:
                            return new Vector2(
                                tail.x,
                                ply.room.MiddleOfTile(tail).y + dist / 2f
                            );

                        case Player.AnimationIndex.GetUpToBeamTip:
                            return new Vector2(
                                ply.room.MiddleOfTile(tail).x,
                                tail.y + dist / 2f
                            );

                        case Player.AnimationIndex.BeamTip:
                            return new Vector2(
                                ply.room.MiddleOfTile(tail).x,
                                ply.room.MiddleOfTile(tail).y + dist / 2f
                            );

                        default:
                            return null;
                    }
                }
            }

            // Target a point above the ground
            private class GroundedTarget : CameraTarget
            {
                private const float maxHeightAboveTarget = 20f * 5f;
                private const float minHeightBelowTarget = -20 * 3f;
                private const float targetSmoothTime = 0.25f;

                private float smoothedTarget;
                private float smoothedTargetVel;
                private float? targetY;

                public GroundedTarget(CameraController owner) : base(owner) { }

                public override Vector2? UpdateCenter(Player ply)
                {
                    if (ply.gravity <= 0f)
                        return null;

                    BodyChunk head = ply.bodyChunks[0];
                    BodyChunk tail = ply.bodyChunks[1];

                    if (targetY != null)
                    {
                        float heightAboveTarget = tail.pos.y - targetY.Value;

                        // Stop being grounded if the player moves too far while in the air
                        if (heightAboveTarget > maxHeightAboveTarget || heightAboveTarget < minHeightBelowTarget)
                            return null;

                        // Stop being grounded if the player moves downwards through a floor
                        if (ply.GoThroughFloors && ply.bodyChunks.Any(c => ply.room.GetTile(c.pos).Terrain == Room.Tile.TerrainType.Floor))
                            return null;

                        // Stop being grounded if the player is going to fall a long distance
                        if (heightAboveTarget < 0f
                            && tail.vel.y < 0f
                            && PredictMinHeight(tail) <= targetY.Value + minHeightBelowTarget)
                            return null;

                        // Stop being grounded if the player is going to jump a high distance
                        if (tail.vel.y > 0f
                            && PredictMaxHeight(tail) >= targetY.Value + maxHeightAboveTarget)
                            return null;

                        if (tail.ContactPoint.y < 0)
                            targetY = tail.pos.y;
                    }
                    else
                    {
                        if (tail.ContactPoint.y < 0)
                        {
                            targetY = tail.pos.y;
                            smoothedTarget = targetY.Value;
                        }
                        else
                            return null;
                    }

                    smoothedTarget = Mathf.SmoothDamp(smoothedTarget, targetY.Value, ref smoothedTargetVel, targetSmoothTime, float.PositiveInfinity, 1f / 40f);

                    return new Vector2(
                        Mathf.Lerp(head.pos.x, tail.pos.x, 0.5f),
                        smoothedTarget + 15f
                    );
                }

                public override void Reset()
                {
                    targetY = null;
                    smoothedTargetVel = 0f;
                    smoothedTarget = 0f;
                }

                private static float PredictMaxHeight(BodyChunk c, float maxSeconds = 1f)
                {
                    float gravity;
                    if (c.ContactPoint.y == 0 && c.lastContactPoint.y == 0)
                        gravity = (c.lastPos.y - c.lastLastPos.y) - (c.pos.y - c.lastPos.y);
                    else
                        gravity = c.owner.gravity;

                    float ticksTilPeak = Mathf.Clamp(Mathf.Floor(c.vel.y / gravity), 0f, maxSeconds);

                    // Derived from sum[n=1..t](vel - gt)
                    float peakHeight = c.pos.y + c.vel.y * ticksTilPeak - gravity * ticksTilPeak * (ticksTilPeak + 1f) / 2f;

                    return Mathf.Max(c.pos.y, peakHeight);
                }

                private static float PredictMinHeight(BodyChunk c)
                {
                    return SharedPhysics.TraceTerrainCollision(c.owner.room, c.pos + new Vector2(0f, minHeightBelowTarget), c.pos, c.rad, c.goThroughFloors).y;
                }
            }

            // Target the player's center
            private class CenterOfMassTarget : CameraTarget
            {
                public CenterOfMassTarget(CameraController owner) : base(owner) { }

                public override Vector2? UpdateCenter(Player ply)
                {
                    return Vector2.Lerp(ply.bodyChunks[0].pos, ply.bodyChunks[1].pos, 0.5f);
                }
            }
            
            // Target the player's shortcut vessel
            private class ShortcutTarget : CameraTarget
            {
                public override bool NeedsRoom => false;

                public ShortcutTarget(CameraController owner) : base(owner) { }

                public override Vector2? UpdateCenter(Player ply)
                {
                    if (!ply.inShortcut) return null;
                    return ShortcutVesselPosition(ply, owner.cam.room);
                }

                private static Vector2? ShortcutVesselPosition(Creature creature, Room room)
                {
                    var playerVessel = FindCreatureVessel(creature, room);
                    if (playerVessel == null) return null;

                    return Vector2.Lerp(room.MiddleOfTile(playerVessel.lastPos), room.MiddleOfTile(playerVessel.pos), room.game.updateShortCut / 3f);
                }

                private static ShortcutHandler.ShortCutVessel FindCreatureVessel(Creature creature, Room room)
                {
                    var shortcuts = room.game.shortcuts;

                    foreach (var vessel in shortcuts.transportVessels)
                    {
                        if (vessel.creature == creature)
                        {
                            if (room.abstractRoom == vessel.room)
                                return vessel;
                            else
                                return null;
                        }
                        else if (room.abstractRoom == vessel.room
                            && vessel.creature.abstractCreature.stuckObjects.Count > 0)
                        {
                            foreach (var stick in vessel.creature.abstractCreature.stuckObjects)
                            {
                                if (stick.A == creature.abstractCreature || stick.B == creature.abstractCreature)
                                    return vessel;
                            }
                        }
                    }

                    return null;
                }
            }
        }
    }
}
