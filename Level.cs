using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

internal readonly record struct Platform(Rectangle Bounds, bool OneWay);
internal enum PickupKind { Smg, Shotgun }
internal sealed class WeaponPickup
{
    public PickupKind Kind;
    public Vector2 Position;
    public bool Taken;
    public WeaponPickup(PickupKind kind, float x, float y) { Kind = kind; Position = new Vector2(x, y); }
    public void Draw(float time)
    {
        if (Taken) return;
        Color accent = Kind == PickupKind.Smg ? Palette.Gold : Palette.Red;
        Vector2 point = Position + new Vector2(0, MathF.Sin(time * 2.6f) * 7);
        Palette.Glow(point, 20, accent, .6f);
        DrawPoly(point, 6, 29, time * 22, Palette.Alpha(accent, .24f));
        DrawPolyLines(point, 6, 29, time * 22, accent);
        DrawRectangleRounded(new Rectangle(point.X - 25, point.Y - 6, 50, 12), .4f, 6, Palette.Navy);
        DrawRectangleRounded(new Rectangle(point.X - 17, point.Y - 3, 35, 5), .4f, 6, accent);
        DrawRectangleRounded(new Rectangle(point.X - 10, point.Y + 5, 8, 9), .3f, 4, Palette.Steel);
        Palette.Centered(Kind == PickupKind.Smg ? "VIPER SMG" : "RAVAGER", point.X, point.Y - 52, 15, accent);
    }
}

internal sealed class Level
{
    public const int Length = 7600;
    public readonly List<Platform> Platforms = new();
    public readonly List<WeaponPickup> WeaponPickups = new();
    public const float StationX = 4180;
    public const float ExitX = 7450;
    public static readonly float[] BarrelPositions =
        { 854, 1710, 1840, 2740, 3205, 3335, 4525, 5450, 6060, 6890, 7020 };

    public Level()
    {
        AddFloor(0, 980); AddFloor(1090, 890); AddFloor(2090, 760); AddFloor(2960, 800);
        AddFloor(3870, 850); AddFloor(4850, 850); AddFloor(5810, 740); AddFloor(6630, 970);
        AddLedge(420, 491, 175); AddLedge(724, 417, 160);
        AddLedge(1320, 491, 230); AddLedge(1630, 425, 160);
        AddLedge(2250, 489, 235); AddLedge(2610, 425, 175);
        AddLedge(3130, 493, 210); AddLedge(3450, 423, 160);
        AddLedge(4010, 490, 185); AddLedge(4440, 431, 175);
        AddLedge(4990, 492, 170); AddLedge(5325, 427, 170);
        AddLedge(5940, 490, 200); AddLedge(6300, 426, 170);
        AddLedge(6790, 490, 200); AddLedge(7110, 427, 170);
        // Low cover can be jumped over and absorbs bullets.
        AddCrate(630, 545, 50, 50); AddCrate(1890, 545, 50, 50);
        AddCrate(3260, 545, 50, 50); AddCrate(4690, 545, 50, 50);
        AddCrate(6170, 545, 50, 50); AddCrate(7240, 545, 50, 50);
        WeaponPickups.Add(new WeaponPickup(PickupKind.Smg, 2340, 452));
        WeaponPickups.Add(new WeaponPickup(PickupKind.Shotgun, 5130, 455));
    }

    private void AddFloor(float x, float width) => Platforms.Add(new Platform(new Rectangle(x, 595, width, 135), false));
    private void AddLedge(float x, float y, float width) => Platforms.Add(new Platform(new Rectangle(x, y, width, 22), true));
    private void AddCrate(float x, float y, float w, float h) => Platforms.Add(new Platform(new Rectangle(x, y, w, h), false));

    public bool HasFloor(float x)
    {
        foreach (var p in Platforms)
            if (!p.OneWay && p.Bounds.Y == 595 && x >= p.Bounds.X + 5 && x < p.Bounds.X + p.Bounds.Width - 5) return true;
        return false;
    }

    public static int Section(float x) => x < 2000 ? 0 : x < 3900 ? 1 : x < 5700 ? 2 : 3;
    public static string SectionName(int section) => section switch { 0 => "01  /  ARRIVAL", 1 => "02  /  SECURITY WING", 2 => "03  /  INDUSTRIAL CORE", _ => "04  /  LOCKDOWN" };
    public static Color SectionColor(int s) => s switch { 0 => Palette.Cyan, 1 => Palette.Purple, 2 => Palette.Orange, _ => Palette.Red };

    private static int Noise(int seed)
    {
        unchecked { uint x = (uint)(seed * 747796405 + 2891336453L); x ^= x >> 16; x *= 2246822519; x ^= x >> 13; return (int)(x & 0x7fffffff); }
    }

    public void DrawBackdrop(float cameraX, float time)
    {
        int section = Section(cameraX + 640);
        Color accent = SectionColor(section);
        DrawRectangleGradientV(0, 0, Game.ScreenWidth, Game.ScreenHeight, new Color(6, 11, 23, 255), new Color(27, 37, 53, 255));
        DrawRectangleGradientV(0, 0, Game.ScreenWidth, 510, Palette.Alpha(Palette.Void, .3f), Palette.Alpha(accent, .075f));
        // Static scan-lines and star-sized sparks.
        for (int i = 0; i < 80; i++)
        {
            int px = Noise(i * 34 + 5) % Game.ScreenWidth;
            int py = Noise(i * 91 + 11) % 500;
            DrawCircle(px, py, i % 5 == 0 ? 1.3f : .6f, Palette.Alpha(Palette.White, .13f + .1f * MathF.Sin(time + i)));
        }
        // Far skyline uses slow parallax, with layered illuminated architecture.
        for (int i = 0; i < 49; i++)
        {
            int worldX = i * 185 - 250;
            float screenX = worldX - cameraX * .12f;
            if (screenX < -260 || screenX > 1360) continue;
            int height = 160 + Noise(i * 19) % 235;
            int width = 110 + Noise(i * 79) % 85;
            int top = 590 - height;
            DrawRectangle((int)screenX, top, width, height + 150, new Color(14, 24, 42, 255));
            DrawRectangle((int)screenX + 5, top + 8, width - 10, 6, new Color(30, 50, 64, 255));
            DrawLine((int)screenX, top, (int)screenX, 590, Palette.Alpha(accent, .18f));
            for (int wy = top + 28; wy < 550; wy += 25)
                for (int wx = (int)screenX + 17; wx < screenX + width - 12; wx += 21)
                    if (Noise(i * 300 + wx + wy) % 4 == 0)
                        DrawRectangle(wx, wy, 8, 3, Palette.Alpha(accent, .17f));
            if (i % 5 == 0)
            {
                DrawRectangle((int)screenX + width / 2, top - 60, 3, 61, Palette.Steel);
                Palette.Glow(new Vector2(screenX + width / 2f, top - 60), 3, accent, .35f);
            }
        }
        // Near silhouettes: suspended beams, cables, gantries and smoke shafts.
        for (int i = 0; i < 42; i++)
        {
            float screenX = i * 250 - 230 - cameraX * .32f;
            if (screenX < -300 || screenX > 1470) continue;
            int roofY = 85 + Noise(i * 99 + 1) % 90;
            DrawRectangle((int)screenX, 0, 23, 635, new Color(18, 29, 44, 255));
            DrawRectangle((int)screenX + 17, 0, 2, 635, Palette.Alpha(accent, .13f));
            DrawRectangle((int)screenX - 35, roofY, 265, 13, new Color(24, 40, 58, 255));
            for (int k = 0; k < 3; k++)
                DrawLineEx(new Vector2(screenX - 15 + 66 * k, roofY + 13), new Vector2(screenX + 46 + 66 * k, roofY + 71), 2, new Color(45, 56, 71, 140));
            if (i % 3 == 0)
            {
                DrawLineEx(new Vector2(screenX + 110, roofY + 12), new Vector2(screenX + 126, roofY + 210), 2.3f, Palette.Steel);
                Palette.Glow(new Vector2(screenX + 126, roofY + 210), 8, accent, .3f);
            }
        }
        DrawInfrastructure(cameraX, time, accent);
        // Volumetric shafts / horizon haze.
        for (int i = 0; i < 7; i++)
        {
            float screenX = (i * 260 - cameraX * .17f) % 1800;
            if (screenX < -180) screenX += 1800;
            DrawRectangleGradientV((int)screenX, 0, 70, 620, Palette.Alpha(accent, .036f), Palette.Alpha(accent, 0));
        }
        DrawRectangleGradientV(0, 460, 1280, 260, Palette.Alpha(accent, .018f), new Color(4, 9, 17, 180));
        for (int y = 0; y < 720; y += 5) DrawLine(0, y, 1280, y, new Color(3, 8, 17, 24));
    }

    // Three distinct visual-depth planes: distant towers, maintenance architecture,
    // then world-space machinery. All geometry is deterministic and texture-free.
    private static void DrawInfrastructure(float cameraX, float time, Color accent)
    {
        for (int i = 0; i < 26; i++)
        {
            float x = i * 325 - 240 - cameraX * .48f;
            if (x < -350 || x > 1590) continue;
            int top = 185 + Noise(i * 29 + 17) % 125;
            DrawRectangle((int)x, top, 225, 310, new Color(9, 20, 34, 255));
            DrawRectangle((int)x + 12, top + 9, 201, 7, new Color(39, 62, 76, 255));
            for (int iy = 0; iy < 4; iy++)
            {
                int yy = top + 35 + iy * 54;
                DrawRectangle((int)x + 22, yy, 181, 25, new Color(16, 34, 50, 255));
                for (int ix = 0; ix < 5; ix++)
                    DrawRectangle((int)x + 32 + ix * 34, yy + 8, 18, 4,
                        Palette.Alpha(accent, (i + ix + iy) % 4 == 0 ? .37f : .075f));
            }
            // Arched machine housing and rotating turbine in the distant plane.
            if (i % 2 == 0)
            {
                Vector2 hub = new(x + 115, top + 190);
                DrawCircleV(hub, 58, new Color(17, 30, 43, 255));
                DrawCircleLines((int)hub.X, (int)hub.Y, 57, new Color(48, 76, 91, 255));
                DrawCircleLines((int)hub.X, (int)hub.Y, 45, Palette.Alpha(accent, .15f));
                for (int blade = 0; blade < 5; blade++)
                {
                    float a = time * .24f + blade * MathF.Tau / 5;
                    Vector2 direction = new(MathF.Cos(a), MathF.Sin(a));
                    Vector2 tangent = new(-direction.Y, direction.X);
                    DrawTriangle(hub + direction * 10, hub + direction * 43 + tangent * 13,
                        hub + direction * 38 - tangent * 6, new Color(40, 61, 70, 255));
                }
                DrawCircleV(hub, 12, new Color(61, 83, 97, 255));
                DrawCircleV(hub, 5, Palette.Alpha(accent, .55f));
            }
            // Overhead coolant pipe and loose hanging power cable.
            DrawLineEx(new Vector2(x - 30, top - 26), new Vector2(x + 244, top - 26), 10, new Color(36, 60, 75, 255));
            DrawLineEx(new Vector2(x + 181, top - 26), new Vector2(x + 178, top + 43), 4, Palette.Steel);
            if (i % 3 == 1)
            {
                Vector2 lamp = new(x + 178, top + 48);
                Palette.Glow(lamp, 12, accent, .17f + MathF.Sin(time * 1.4f + i) * .07f);
                DrawRectangleRounded(new Rectangle(lamp.X - 12, lamp.Y - 4, 24, 7), .2f, 4, accent);
            }
        }
    }

    private static void DrawSetPieces(float time, float cameraX)
    {
        for (int i = 0; i < 19; i++)
        {
            float x = 260 + i * 420f;
            if (x < cameraX - 220 || x > cameraX + 1500) continue;
            int section = Section(x);
            Color tint = SectionColor(section);
            float y = 275 + (i % 3) * 24;
            // Large industrial column and breathing coolant reactor.
            DrawRectangle((int)x - 80, (int)y - 155, 158, 240, new Color(9, 21, 32, 245));
            DrawRectangleLinesEx(new Rectangle(x - 80, y - 155, 158, 240), 5, Palette.Steel);
            DrawRectangle((int)x - 68, (int)y - 137, 136, 10, new Color(42, 61, 76, 255));
            DrawRectangle((int)x - 68, (int)y + 59, 136, 13, new Color(42, 61, 76, 255));
            for (int j = 0; j < 4; j++)
                DrawLineEx(new Vector2(x - 65, y - 94 + j * 32),
                    new Vector2(x + 65, y - 94 + j * 32), 3, Palette.Alpha(tint, .16f));
            DrawCircleV(new Vector2(x, y - 28), 44, new Color(25, 45, 59, 255));
            DrawCircleLines((int)x, (int)(y - 28), 42, Palette.Alpha(tint, .65f));
            DrawCircleLines((int)x, (int)(y - 28), 32, Palette.Alpha(Palette.White, .2f));
            for (int k = 0; k < 6; k++)
            {
                float a = time * (section == 3 ? 2.3f : 1.1f) + k * MathF.Tau / 6;
                Vector2 dir = new(MathF.Cos(a), MathF.Sin(a));
                DrawLineEx(new Vector2(x, y - 28) + dir * 12,
                    new Vector2(x, y - 28) + dir * 28, 6, Palette.Alpha(tint, .52f));
            }
            Palette.Glow(new Vector2(x, y - 28), 16, tint, .13f);
            DrawCircleV(new Vector2(x, y - 28), 10, tint);
            DrawCircleV(new Vector2(x, y - 28), 5, Palette.Navy);
            DrawRectangle((int)x - 47, (int)y + 35, 94, 12, new Color(12, 30, 44, 255));
            int progress = (int)((time * 24 + i * 11) % 82);
            DrawRectangle((int)x - 41 + progress, (int)y + 40, 7, 2, tint);
            Palette.Centered($"CORE / {i + 1:00}", x, y + 84, 12, Palette.Alpha(tint, .7f));
            if (section == 3)
            {
                float flash = .22f + .16f * (1 + MathF.Sin(time * 5 + i));
                DrawRectangle((int)x - 78, (int)y - 150, 155, 3, Palette.Alpha(Palette.Red, flash));
            }
        }
        // Wall-spanning copper lines link the reactors, behind solid geometry.
        for (int k = 0; k < 3; k++)
        {
            DrawLineEx(new Vector2(cameraX - 160, 355 + k * 13),
                new Vector2(cameraX + 1480, 355 + k * 13), k == 1 ? 6 : 3,
                new Color(46, 61, 72, 160));
        }
    }

    public void DrawForeground(float time, float cameraX)
    {
        // Subtle foreground power conduits (below the playable floor, never hide the character).
        for (int i = 0; i < 24; i++)
        {
            float x = 85 + i * 345f;
            if (x < cameraX - 100 || x > cameraX + 1360 || !HasFloor(x)) continue;
            Color tint = SectionColor(Section(x));
            DrawRectangle((int)x - 45, 653, 90, 62, new Color(7, 14, 24, 190));
            DrawRectangle((int)x - 42, 663, 84, 5, Palette.Alpha(tint, .17f));
            DrawRectangle((int)x - 42, 676, 84, 4, Palette.Alpha(Palette.Muted, .32f));
            for (int stripe = 0; stripe < 4; stripe++)
                DrawLineEx(new Vector2(x - 37 + stripe * 22, 686),
                    new Vector2(x - 19 + stripe * 22, 700), 3, Palette.Alpha(tint, .29f));
        }
        for (int i = 0; i < 9; i++)
        {
            float x = cameraX + i * 163 - 60 + MathF.Sin(time * .3f + i) * 13;
            float y = 120 + (i * 71) % 390;
            DrawCircleV(new Vector2(x, y), 2 + i % 2,
                Palette.Alpha(SectionColor(Section(x)), .07f + .06f * MathF.Sin(time + i)));
        }
    }

    public void DrawWorld(float time, float cameraX, bool exitUnlocked)
    {
        DrawSetPieces(time, cameraX);
        foreach (var p in Platforms)
        {
            Rectangle r = p.Bounds;
            if (r.X + r.Width < cameraX - 250 || r.X > cameraX + 1500) continue;
            if (r.Height > 40 && r.Y == 595)
            {
                DrawRectangleRec(r, new Color(16, 27, 42, 255));
                DrawRectangleGradientV((int)r.X, (int)r.Y + 4, (int)r.Width, 46, new Color(53, 69, 78, 255), new Color(22, 30, 46, 255));
                DrawRectangle((int)r.X, (int)r.Y, (int)r.Width, 5, new Color(83, 117, 126, 255));
                DrawRectangle((int)r.X, (int)r.Y + 5, (int)r.Width, 2, Palette.Alpha(Palette.Cyan, .45f));
                for (float x = r.X + 35; x < r.X + r.Width - 15; x += 88)
                {
                    DrawRectangle((int)x, (int)r.Y + 20, 67, 2, Palette.Steel);
                    DrawRectangle((int)x, (int)r.Y + 35, 4, 4, Palette.Orange);
                    DrawLine((int)x, (int)r.Y + 53, (int)x + 19, (int)r.Y + 74, Palette.Steel);
                    DrawLine((int)x + 19, (int)r.Y + 74, (int)x + 49, (int)r.Y + 74, Palette.Steel);
                }
            }
            else if (p.OneWay)
            {
                DrawRectangle((int)r.X + 10, (int)r.Y + 17, (int)r.Width - 20, 33, new Color(7, 14, 23, 200));
                DrawRectangleRec(r, new Color(30, 47, 63, 255));
                DrawRectangle((int)r.X, (int)r.Y, (int)r.Width, 5, Palette.Muted);
                DrawRectangle((int)r.X + 7, (int)r.Y + 4, (int)r.Width - 14, 2, Palette.Alpha(Palette.Cyan, .65f));
                for (float x = r.X + 22; x < r.X + r.Width; x += 40)
                {
                    DrawLine((int)x, (int)r.Y + 8, (int)x - 8, (int)r.Y + 19, new Color(72, 90, 104, 255));
                    DrawCircle((int)x, (int)r.Y + 12, 2f, Palette.Orange);
                }
                // Real support struts make platforms feel anchored to the level.
                DrawLineEx(new Vector2(r.X + 24, r.Y + 21), new Vector2(r.X + 40, r.Y + 95), 5, new Color(32, 46, 62, 255));
                DrawLineEx(new Vector2(r.X + r.Width - 24, r.Y + 21), new Vector2(r.X + r.Width - 40, r.Y + 95), 5, new Color(32, 46, 62, 255));
            }
            else // Cover crate
            {
                DrawRectangleRounded(r, .1f, 4, new Color(36, 52, 66, 255));
                DrawRectangleLinesEx(r, 3, Palette.Muted);
                DrawLineEx(new Vector2(r.X + 8, r.Y + 8), new Vector2(r.X + r.Width - 8, r.Y + r.Height - 8), 4, Palette.Steel);
                DrawLineEx(new Vector2(r.X + r.Width - 8, r.Y + 8), new Vector2(r.X + 8, r.Y + r.Height - 8), 4, Palette.Steel);
                DrawRectangle((int)r.X + 3, (int)r.Y + 3, (int)r.Width - 6, 5, Palette.Orange);
            }
        }
        // Industrial machines and decorative machinery at fixed, navigable locations.
        for (int i = 0; i < 45; i++)
        {
            float x = 180 + i * 167;
            if (x < cameraX - 170 || x > cameraX + 1420 || !HasFloor(x)) continue;
            float y = 595;
            if (i % 4 == 0)
            {
                DrawRectangle((int)x - 27, (int)y - 90, 55, 89, new Color(21, 34, 49, 255));
                DrawRectangleLinesEx(new Rectangle(x - 27, y - 90, 55, 89), 3, Palette.Steel);
                DrawRectangle((int)x - 22, (int)y - 80, 45, 27, new Color(9, 22, 34, 255));
                for (int k = 0; k < 4; k++) DrawRectangle((int)x - 15, (int)y - 74 + k * 6, 30, 2, Palette.Alpha(SectionColor(Section(x)), .45f));
                Palette.Glow(new Vector2(x, y - 43), 7, Palette.Cyan, .5f);
                DrawCircle((int)x, (int)y - 43, 5, Palette.Cyan);
            }
            else if (i % 4 == 1)
            {
                DrawRectangle((int)x, (int)y - 126, 9, 127, new Color(37, 52, 69, 255));
                DrawRectangle((int)x - 13, (int)y - 128, 34, 11, Palette.Steel);
                Palette.Glow(new Vector2(x + 4, y - 125), 6, Palette.Orange, .5f);
                DrawCircle((int)x + 4, (int)y - 125, 4, Palette.Orange);
            }
            else
            {
                DrawRectangle((int)x - 21, (int)y - 34, 42, 35, new Color(31, 44, 62, 255));
                DrawRectangleLinesEx(new Rectangle(x - 21, y - 34, 42, 35), 2, Palette.Steel);
                DrawRectangle((int)x - 15, (int)y - 29, 29, 3, Palette.Alpha(Palette.Cyan, .55f));
                for (int k = 0; k < 3; k++) DrawLine((int)x - 15, (int)y - 16 + k * 5, (int)x + 14, (int)y - 16 + k * 5, Palette.Steel);
            }
        }

        // Upgrade console and extraction vault.
        DrawStation(time);
        DrawExit(time, exitUnlocked);
        foreach (var pickup in WeaponPickups) pickup.Draw(time);
        // Area dividers engraved into floor, not blocking the player.
        for (int i = 1; i <= 3; i++)
        {
            float x = i switch { 1 => 2090f, 2 => 3870f, _ => 5810f };
            if (x < cameraX - 80 || x > cameraX + 1380) continue;
            Palette.LightLine(new Vector2(x, 270), new Vector2(x, 590), 1.2f, SectionColor(i));
            DrawRectangle((int)x - 54, 268, 108, 24, new Color(13, 24, 37, 235));
            Palette.Centered($"SECTOR 0{i + 1}", x, 274, 13, SectionColor(i));
        }
    }

    private static void DrawStation(float time)
    {
        float x = StationX;
        DrawRectangle((int)x - 32, 506, 64, 89, new Color(22, 37, 56, 255));
        DrawRectangleLinesEx(new Rectangle(x - 32, 506, 64, 89), 3, Palette.Cyan);
        DrawRectangleRounded(new Rectangle(x - 25, 519, 50, 43), .18f, 4, new Color(5, 28, 43, 255));
        DrawPoly(new Vector2(x, 540), 6, 16, time * 32, Palette.Cyan);
        DrawPoly(new Vector2(x, 540), 6, 9, -time * 57, Palette.Void);
        Palette.Glow(new Vector2(x, 540), 23, Palette.Cyan, .5f);
        Palette.Centered("UPGRADE TERMINAL", x, 482, 15, Palette.Cyan);
        DrawRectangle((int)x - 40, 585, 80, 9, Palette.Steel);
    }

    private static void DrawExit(float time, bool unlocked)
    {
        float x = ExitX;
        Color c = unlocked ? Palette.Cyan : Palette.Red;
        DrawRectangle((int)x - 54, 397, 108, 198, new Color(24, 37, 53, 255));
        DrawRectangle((int)x - 41, 417, 82, 178, new Color(6, 15, 25, 255));
        DrawRectangleLinesEx(new Rectangle(x - 53, 396, 106, 198), 6, Palette.Steel);
        for (int k = 0; k < 6; k++)
            DrawLine((int)x - 38, 441 + k * 24, (int)x + 38, 441 + k * 24, Palette.Alpha(c, unlocked ? .58f : .23f));
        Palette.Glow(new Vector2(x, 452), 27, c, .35f);
        DrawPoly(new Vector2(x, 452), 6, 16, time * 35, c);
        Palette.Centered(unlocked ? "EXTRACTION READY" : "LOCKDOWN ACTIVE", x, 363, 16, c);
        Palette.Centered(unlocked ? "[ E ] EXTRACT" : "ELIMINATE HOSTILES", x, 386, 13, Palette.White);
    }
}
