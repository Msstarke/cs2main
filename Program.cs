using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Swed64;
using cs2aim.Core;
using cs2aim.Utils;
using cs2aim.Overlay;

namespace cs2aim
{
    public class Program
    {
        private const int AimbotHotkey = 0x06; // Right mouse
        private static readonly Random rnd = new Random();

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public static void Main(string[] args)
        {
            var swed = new Swed("cs2");
            IntPtr clientBase = swed.GetModuleBase("client.dll");

            var renderer = new AimbotOverlay();
            Task.Run(() => renderer.Run());

            var targets = new List<Entity>();
            var local   = new Entity();

            while (true)
            {
                targets.Clear();

                // — read local player —
                IntPtr listPtr        = swed.ReadPointer(clientBase, (int)Offsets.dwEntityList);
                local.PawnAddress     = swed.ReadPointer(clientBase, (int)Offsets.dwLocalPlayerPawn);
                local.Team            = swed.ReadInt(local.PawnAddress, (int)Offsets.m_iTeamNum);
                local.Origin          = swed.ReadVec(local.PawnAddress, (int)Offsets.m_vOldOrigin);
                local.view            = swed.ReadVec(local.PawnAddress, (int)Offsets.m_vecViewOffset);

                // — find and collect valid targets —
                for (int i = 0; i < 64; i++)
                {
                    IntPtr entry = swed.ReadPointer(listPtr, 0x10);
                    if (entry == IntPtr.Zero) continue;

                    IntPtr ctrl = swed.ReadPointer(entry, i * 0x78);
                    if (ctrl == IntPtr.Zero) continue;

                    int handle = swed.ReadInt(ctrl, (int)Offsets.m_hPlayerPawn);
                    if (handle == 0) continue;

                    IntPtr pawnEntry = swed.ReadPointer(
                        listPtr, ((handle & 0x7FFF) >> 9) * 8 + 0x10);
                    if (pawnEntry == IntPtr.Zero) continue;

                    IntPtr pawn = swed.ReadPointer(
                        pawnEntry, (handle & 0x1FF) * 0x78);
                    if (pawn == local.PawnAddress) continue;
                    if (!swed.ReadBool(pawn, (int)Offsets.M_bIsSpotted)) continue;

                    int    health    = swed.ReadInt(pawn, (int)Offsets.m_iHealth);
                    int    team      = swed.ReadInt(pawn, (int)Offsets.m_iTeamNum);
                    uint   lifeState = swed.ReadUInt(pawn, (int)Offsets.m_lifeState);
                    if (lifeState != 256) continue;
                    if (team == local.Team && !renderer.AimOnTeam) continue;

                    var target = new Entity
                    {
                        PawnAddress      = pawn,
                        ControllerAddress= ctrl,
                        Health           = health,
                        Team             = team,
                        LifeState        = lifeState,
                        Origin           = swed.ReadVec(pawn, (int)Offsets.m_vOldOrigin),
                        view             = swed.ReadVec(pawn, (int)Offsets.m_vecViewOffset)
                    };
                    target.DistanceToLocal = Vector3.Distance(target.Origin, local.Origin);

                    // —— HEAD TRACKING FIX ——
                    IntPtr sceneNode = swed.ReadPointer(pawn, (int)Offsets.m_pGameSceneNode);
                    if (sceneNode != IntPtr.Zero)
                    {
                        // read boneMatrix in one go:
                        IntPtr boneMatrix = swed.ReadPointer(
                            sceneNode,
                            (int)Offsets.m_modelState + 0x80
                        );
                        if (boneMatrix != IntPtr.Zero)
                        {
                            const int HEAD_BONE_IDX = 6;
                            const int BONE_SIZE     = 0x20; // 32 bytes per bone
                            target.head = swed.ReadVec(
                                boneMatrix,
                                HEAD_BONE_IDX * BONE_SIZE
                            );
                        }
                    }
                    // fallback to body if head was invalid
                    if (target.head == Vector3.Zero)
                        target.head = target.Origin + target.view;

                    targets.Add(target);

                    // debug print
                    Console.ForegroundColor = team == local.Team
                                              ? ConsoleColor.Green
                                              : ConsoleColor.Red;
                    Console.WriteLine($"{target.Health}hp @ {target.head}");
                    Console.ResetColor();
                }

                // — select nearest by FOV (aiming at head now) —
                float maxFov = renderer.MaxFov;
                float smooth = renderer.SmoothAmount;
                float jitter = renderer.JitterAmount;

                var nearest = targets
                    .Select(t =>
                    {
                        Vector3 eye    = local.Origin + local.view;
                        Vector2 angs   = MathHelpers.CalculateAngles(eye, t.head);
                        Vector2 currYX = swed.ReadVec(clientBase, (int)Offsets.dwViewAngles).YX();
                        float   fov    = MathF.Sqrt(
                            MathF.Pow(MathHelpers.NormalizeAngle(angs.X - currYX.X), 2) +
                            MathF.Pow(MathHelpers.NormalizeAngle(angs.Y - currYX.Y), 2)
                        );
                        t.distance = fov;
                        return t;
                    })
                    .Where(t => t.distance <= maxFov)
                    .OrderBy(t => t.distance)
                    .FirstOrDefault();

                // — perform aimbot write —
                if (nearest != null &&
                    GetAsyncKeyState(AimbotHotkey) < 0 &&
                    renderer.AimbotEnabled)
                {
                    Vector3 eyePos      = local.Origin + local.view;
                    Vector2 desiredAngs = MathHelpers.CalculateAngles(eyePos, nearest.head);
                    Vector3 currView    = swed.ReadVec(clientBase, (int)Offsets.dwViewAngles);
                    float   currYaw     = currView.Y;
                    float   currPitch   = currView.X;

                    float deltaYaw   = MathHelpers.NormalizeAngle(desiredAngs.X - currYaw);
                    float deltaPitch = MathHelpers.NormalizeAngle(desiredAngs.Y - currPitch);

                    float moveYaw   = deltaYaw   * smooth + (float)(rnd.NextDouble() - 0.5f) * jitter;
                    float movePitch = deltaPitch * smooth + (float)(rnd.NextDouble() - 0.5f) * jitter;

                    swed.WriteVec(
                        clientBase, (int)Offsets.dwViewAngles,
                        new Vector3(currPitch + movePitch,
                                    currYaw   + moveYaw,
                                    0)
                    );
                }

                Thread.Sleep(1);
            }
        }
    }

    public static class VectorHelpers
    {
        public static Vector2 YX(this Vector3 v) => new Vector2(v.Y, v.X);
    }
}
