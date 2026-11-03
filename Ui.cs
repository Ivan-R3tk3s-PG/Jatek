using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

internal sealed class Ui
{
    public Color NotificationColor = Palette.Cyan;
    private float _displayHealth = 100;
    public void ResetHud(int maxHealth) => _displayHealth = maxHealth;

    public void DrawScreenFilter(float time)
    {
        // Subtle cinematic vignette in screen-space, kept away from gameplay center.
        DrawRectangleGradientH(0, 0, 145, 720, new Color(1, 4, 12, 90), new Color(1, 4, 12, 0));
        DrawRectangleGradientH(1135, 0, 145, 720, new Color(1, 4, 12, 0), new Color(1, 4, 12, 90));
        DrawRectangleGradientV(0, 0, 1280, 55, new Color(1, 4, 12, 90), new Color(1, 4, 12, 0));
        DrawRectangleGradientV(0, 641, 1280, 79, new Color(1, 4, 12, 0), new Color(1, 4, 12, 90));
        DrawRectangle(0, 0, 1280, 2, Palette.Alpha(Palette.Cyan, .16f));
        DrawRectangle(0, 718, 1280, 2, Palette.Alpha(Palette.Cyan, .16f));
    }

    private static void Scrim(float darkness = .84f)
    {
        DrawRectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight, Palette.Alpha(new Color(3, 8, 16, 255), darkness));
        for (int y = 0; y < 720; y += 6) DrawLine(0, y, 1280, y, Palette.Alpha(Palette.Cyan, .016f));
    }

    public void DrawTitle(Game game)
    {
        float t = game.TotalTime;
        DrawRectangleGradientH(0, 0, 920, 720, new Color(3, 8, 18, 248), new Color(4, 9, 18, 36));
        DrawRectangle(87, 113, 4, 310, Palette.Cyan);
        Palette.Glow(new Vector2(89, 200), 9, Palette.Cyan, .6f);
        Palette.Label("TRANSMISSION  //  00-ALPHA", 111, 96, 18, Palette.Cyan);
        Palette.Label("BUILD 1.1  /  PHANTOM CIRCUIT", 820, 38, 16, Palette.Muted);
        DrawRectangle(817, 69, 343, 2, Palette.Alpha(Palette.Cyan, .24f));
        Palette.Label("NEON", 105, 147, 94, Palette.White);
        Palette.Label("BREACH", 101, 247, 110, Palette.Cyan);
        DrawRectangle(107, 372, 525, 2, Palette.Cyan);
        Palette.Label("EXTRACTION PROTOCOL", 112, 391, 25, Palette.White);
        Palette.Label("One operative. One facility. No backup.", 112, 431, 20, Palette.Muted);
        Palette.Label("An original C# run-and-gun platformer", 112, 459, 17, Palette.Muted);
        if (Palette.Button(new Rectangle(112, 521, 284, 66), "INITIATE MISSION  >", Palette.Cyan))
        { game.NewGame(); game.Audio.Play("ui", .35f); }
        Palette.Label("[ ENTER ] START", 122, 599, 16, Palette.Muted);

        // Moving holographic targeting device creates title-screen motion without asset dependencies.
        Vector2 center = new(975, 346);
        Palette.Glow(center, 120, Palette.Cyan, .17f);
        DrawCircleLines((int)center.X, (int)center.Y, 177, Palette.Alpha(Palette.Cyan, .22f));
        DrawCircleLines((int)center.X, (int)center.Y, 130, Palette.Alpha(Palette.Cyan, .55f));
        DrawCircleLines((int)center.X, (int)center.Y, 85, Palette.Alpha(Palette.Red, .48f));
        for (int i = 0; i < 14; i++)
        {
            float a = (i / 14f) * MathF.Tau + t * .13f;
            Vector2 a1 = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * 146;
            Vector2 a2 = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * 171;
            DrawLineEx(a1, a2, i % 4 == 0 ? 4 : 2, i % 4 == 0 ? Palette.Red : Palette.Alpha(Palette.Cyan, .48f));
        }
        DrawPoly(center, 6, 83, t * 11, new Color(22, 45, 58, 210));
        DrawPoly(center, 6, 70, -t * 17, new Color(7, 22, 39, 255));
        DrawPolyLines(center, 6, 73, t * 11, Palette.Cyan);
        DrawPoly(center, 3, 39, -90, Palette.Red);
        DrawPoly(center, 3, 24, -90, Palette.Navy);
        DrawCircleV(center, 8, Palette.White);
        Palette.LightLine(center + new Vector2(-205, 0), center + new Vector2(205, 0), 1, Palette.Cyan);
        Palette.LightLine(center + new Vector2(0, -205), center + new Vector2(0, 205), 1, Palette.Cyan);
        Palette.Centered("FACILITY / 08", center.X, 560, 18, Palette.White);
        Palette.Centered("THREAT LEVEL : CRITICAL", center.X, 589, 16, Palette.Red);
        DrawRectangle(0, 679, 1280, 41, new Color(4, 12, 21, 240));
        Palette.Label("A/D MOVE    SPACE JUMP    SHIFT / Q DASH    CTRL / C SLIDE    MOUSE FIRE    1-3 WEAPON    E INTERACT", 37, 693, 16, Palette.Muted);
    }

    public void DrawHud(Game game)
    {
        Player p = game.Player;
        // Animated armor damage and shield status.
        _displayHealth += (p.Health - _displayHealth) * .11f;
        if (MathF.Abs(p.Health - _displayHealth) < .15f) _displayHealth = p.Health;
        // Left status module.
        Palette.Panel(new Rectangle(23, 19, 317, 104), Palette.Cyan);
        Palette.Label("OPERATIVE / V-09", 38, 31, 18, Palette.White);
        Palette.Label("ARMOR", 38, 65, 16, Palette.Muted);
        DrawRectangle(105, 66, 214, 16, new Color(30, 48, 60, 255));
        Color hpColor = p.Health < p.MaxHealth * .3f ? Palette.Red : Palette.Cyan;
        DrawRectangle(105, 66, (int)(214 * _displayHealth / p.MaxHealth), 16, Palette.Alpha(Palette.Gold, .44f));
        DrawRectangle(105, 66, (int)(214 * p.Health / (float)p.MaxHealth), 16, hpColor);
        for (int i = 1; i < 10; i++) DrawRectangle(105 + i * 21, 66, 2, 16, Palette.Alpha(Palette.Navy, .45f));
        DrawRectangleLinesEx(new Rectangle(105, 66, 214, 16), 1.4f, Palette.Muted);
        Palette.Label($"{p.Health}/{p.MaxHealth}", 104, 92, 15, Palette.White);
        Palette.Label($"{p.Credits:000}  ENERGY", 220, 92, 15, Palette.Gold);

        Palette.Panel(new Rectangle(355, 19, 302, 104), p.CurrentWeapon.Color);
        Palette.Label("WEAPON SYSTEM", 371, 31, 15, Palette.Muted);
        Palette.Label(p.CurrentWeapon.Name, 370, 52, 24, p.CurrentWeapon.Color);
        string ammo = p.CurrentWeapon.HasInfiniteAmmo ? "AMMO  -- / INF" : $"AMMO  {p.CurrentWeapon.Magazine:00} / {p.CurrentWeapon.Reserve:000}";
        if (p.CurrentWeapon.Reloading) ammo = $"RELOADING  {p.CurrentWeapon.ReloadTimer:0.0}s";
        Palette.Label(ammo, 371, 92, 17, Palette.White);
        // Actual equipped/available weapons, not dead decorative indicators.
        for (int i = 0; i < 3; i++)
        {
            var weapon = p.Weapons[i];
            float x = 355 + i * 102;
            bool active = p.Equipped == (WeaponKind)i;
            Color color = active ? weapon.Color : Palette.Steel;
            DrawRectangleRounded(new Rectangle(x, 129, 96, 29), .18f, 4,
                active ? Palette.Alpha(weapon.Color, .22f) : Palette.Alpha(Palette.Navy, .8f));
            DrawRectangleRoundedLinesEx(new Rectangle(x, 129, 96, 29), .18f, 4, active ? 1.5f : 1f,
                Palette.Alpha(color, weapon.Owned ? .75f : .25f));
            string shortName = i == 0 ? "V9" : i == 1 ? "SMG" : "SG";
            Palette.Centered(weapon.Owned ? $"{i + 1}  {shortName}" : $"{i + 1}  LOCK", x + 48, 136, 14,
                weapon.Owned ? (active ? Palette.White : Palette.Muted) : Palette.Steel);
        }

        Palette.Panel(new Rectangle(901, 19, 354, 104), Level.SectionColor(Level.Section(p.Center.X)));
        Palette.Label(Level.SectionName(Level.Section(p.Center.X)), 919, 31, 18, Palette.White);
        Palette.Label($"HOSTILES KIA  {game.Kills:00}    TIME  {TimeSpan.FromSeconds(game.PlayTime):mm\\:ss}", 919, 60, 17, Palette.Muted);
        DrawRectangle(920, 95, 313, 6, Palette.Steel);
        DrawRectangle(920, 95, (int)(313 * Math.Clamp(p.Center.X / Level.Length, 0, 1)), 6, Palette.Cyan);
        Palette.Glow(new Vector2(920 + 313 * Math.Clamp(p.Center.X / Level.Length, 0, 1), 98), 4, Palette.Cyan, .5f);
        for (int i = 1; i <= 3; i++)
        {
            float mark = 920 + 313 * (i switch { 1 => 2000f, 2 => 3900f, _ => 5700f }) / Level.Length;
            DrawRectangle((int)mark, 90, 2, 14, Level.SectionColor(i));
        }

        // Movement capability module shows the actual dash cooldown, not a fake stamina bar.
        Rectangle abilityPanel = new(996, 627, 260, 54);
        Palette.Panel(abilityPanel, p.DashReady ? Palette.Cyan : Palette.Muted);
        Palette.Label("SHIFT  /  PHASE DASH", 1010, 635, 15, p.DashReady ? Palette.Ice : Palette.Muted);
        DrawRectangle(1010, 660, 232, 8, Palette.Steel);
        float ready = 1 - Math.Clamp(p.DashCooldown / p.DashMaxCooldown, 0, 1);
        DrawRectangle(1010, 660, (int)(232 * ready), 8, p.DashReady ? Palette.Cyan : Palette.Purple);
        Palette.Label(p.IsSliding ? "SLIDING" : p.IsDashing ? "DASHING" : p.DashReady ? "READY" : $"{p.DashCooldown:0.0}s", 1167, 635, 14,
            p.DashReady ? Palette.Cyan : Palette.Muted);

        if (game.ToastTime > 0)
        {
            float alpha = Math.Min(1, game.ToastTime * 2);
            float w = Math.Max(320, MeasureText(game.Toast, 18) + 55);
            float slide = (1 - Math.Clamp(game.ToastTime / 3.1f, 0, 1)) * 14f;
            Rectangle r = new(640 - w / 2, 177 - slide, w, 48);
            DrawRectangleRounded(r, .16f, 6, Palette.Alpha(Palette.Navy, alpha * .9f));
            DrawRectangleRoundedLinesEx(r, .16f, 6, 1.6f, Palette.Alpha(NotificationColor, alpha));
            Palette.Centered(game.Toast, 640, 191 - slide, 18, Palette.Alpha(NotificationColor, alpha));
        }

        if (game.State == GameState.Playing)
        {
            if (Vector2.Distance(p.Center, new Vector2(Level.StationX, 543)) < 125)
            {
                DrawRectangleRounded(new Rectangle(482, 592, 317, 44), .12f, 4, Palette.Navy);
                Palette.Centered("[ E ] OPEN UPGRADE TERMINAL", 640, 604, 18, Palette.Cyan);
            }
            if (p.Center.X > Level.ExitX - 120 && game.ExitUnlocked)
            {
                DrawRectangleRounded(new Rectangle(502, 591, 280, 44), .12f, 4, Palette.Navy);
                Palette.Centered("[ E ] EXTRACT", 640, 603, 21, Palette.Cyan);
            }
            // Mouse reticle (only while playing).
            Vector2 m = GetMousePosition();
            Color c = p.CurrentWeapon.Color;
            Palette.Glow(m, 8, c, .36f);
            DrawCircleLines((int)m.X, (int)m.Y, 13, Palette.Alpha(c, .8f));
            DrawLineEx(m - new Vector2(23, 0), m - new Vector2(10, 0), 2, c);
            DrawLineEx(m + new Vector2(10, 0), m + new Vector2(23, 0), 2, c);
            DrawLineEx(m - new Vector2(0, 23), m - new Vector2(0, 10), 2, c);
            DrawLineEx(m + new Vector2(0, 10), m + new Vector2(0, 23), 2, c);
            DrawCircleV(m, 2f, Palette.White);
        }
        Palette.Label("A/D MOVE   SPACE JUMP   SHIFT/Q DASH   CTRL/C SLIDE   LMB FIRE   R RELOAD   ESC PAUSE", 23, 695, 15, Palette.Alpha(Palette.White, .55f));
    }

    public void DrawPause(Game game)
    {
        Scrim(.75f);
        Palette.Panel(new Rectangle(385, 157, 510, 415), Palette.Cyan);
        Palette.Centered("PROTOCOL SUSPENDED", 640, 220, 35, Palette.White);
        Palette.Centered("[ ESC ]  RESUME", 640, 281, 20, Palette.Cyan);
        Palette.Centered("SHIFT / Q  DASH      CTRL / C  SLIDE      1-3  WEAPONS", 640, 318, 16, Palette.Muted);
        if (Palette.Button(new Rectangle(466, 355, 347, 64), "RESUME MISSION", Palette.Cyan)) game.State = GameState.Playing;
        if (Palette.Button(new Rectangle(466, 442, 347, 64), "RESTART MISSION", Palette.Red)) game.NewGame();
    }

    public void DrawUpgrades(Game game)
    {
        Scrim(.88f);
        Palette.Label("TECH RELAY    /    ACTIVE    /    SYSTEM VERSION 1.1", 138, 101, 19, Palette.Cyan);
        Palette.Label("FIELD UPGRADES", 131, 138, 58, Palette.White);
        Palette.Label($"AVAILABLE ENERGY: {game.Player.Credits}     [ ESC / E ] CLOSE", 138, 217, 19, Palette.Gold);
        string[] names = { "REINFORCED ARMOR", "OVERCHARGED ROUNDS", "COMBAT ACCELERATOR" };
        string[] description = { "+60 maximum HP", "+5 damage per hit", "+14% movement speed" };
        string[] detail = { "Instant healing bonus", "Applies to all weapons", "22% faster weapon fire" };
        Color[] colors = { Palette.Cyan, Palette.Orange, Palette.Purple };
        int[] prices = { 45, 55, 55 };
        bool[] owned = { game.Player.ReinforcedArmor, game.Player.OverchargedRounds, game.Player.CombatAccelerator };
        for (int i = 0; i < 3; i++)
        {
            float x = 135 + i * 345;
            Rectangle r = new(x, 276, 322, 316);
            Palette.Panel(r, colors[i]);
            DrawRectangle((int)x + 24, 297, 50, 5, colors[i]);
            DrawCircle((int)x + 51, 363, 26, Palette.Alpha(colors[i], .2f));
            DrawPolyLines(new Vector2(x + 51, 363), 6, 24, game.TotalTime * 10, colors[i]);
            Palette.Label($"0{i + 1}", x + 41, 349, 21, Palette.White);
            Palette.Label(names[i], x + 21, 411, 20, colors[i]);
            Palette.Label(description[i], x + 21, 448, 19, Palette.White);
            Palette.Label(detail[i], x + 21, 476, 17, Palette.Muted);
            string buttonLabel = owned[i] ? "INSTALLED" : $"[{i + 1}] INSTALL - {prices[i]} E";
            if (Palette.Button(new Rectangle(x + 19, 520, 282, 54), buttonLabel, colors[i], !owned[i]))
                game.TryUpgrade(i);
        }
        Palette.Label("Upgrades are permanent for the current mission.", 140, 625, 18, Palette.Muted);
    }

    public void DrawGameOver(Game game)
    {
        Scrim(.83f);
        Palette.Glow(new Vector2(640, 235), 65, Palette.Red, .38f);
        Palette.Centered("SIGNAL LOST", 640, 204, 78, Palette.Red);
        Palette.Centered("OPERATIVE DOWN // CONNECTION TERMINATED", 640, 298, 22, Palette.White);
        Palette.Centered($"HOSTILES NEUTRALIZED {game.Kills}     DISTANCE {game.Player.Position.X / Level.Length:P0}", 640, 356, 20, Palette.Muted);
        if (Palette.Button(new Rectangle(449, 434, 382, 75), "[ R ] REDEPLOY", Palette.Cyan)) game.NewGame();
        Palette.Centered("[ ENTER ] ALSO RESTARTS", 640, 537, 17, Palette.Muted);
    }

    public void DrawVictory(Game game)
    {
        Scrim(.84f);
        Palette.Glow(new Vector2(640, 260), 85, Palette.Cyan, .44f);
        Palette.Centered("EXTRACTION COMPLETE", 640, 179, 56, Palette.Cyan);
        Palette.Centered("FACILITY BREACHED // ALL SYSTEMS NOMINAL", 640, 256, 22, Palette.White);
        Palette.Panel(new Rectangle(355, 314, 570, 137), Palette.Cyan);
        Palette.Label("MISSION ELAPSED", 388, 331, 18, Palette.Muted);
        Palette.Label(TimeSpan.FromSeconds(game.PlayTime).ToString(@"mm\:ss"), 388, 368, 43, Palette.White);
        Palette.Label("HOSTILES KIA", 716, 331, 18, Palette.Muted);
        Palette.Label(game.Kills.ToString("00"), 715, 368, 43, Palette.Gold);
        if (Palette.Button(new Rectangle(450, 495, 379, 70), "[ ENTER ] MAIN MENU", Palette.Cyan)) game.State = GameState.Title;
        Palette.Centered("NEON BREACH 1.1  /  PHANTOM CIRCUIT", 640, 611, 16, Palette.Muted);
    }
}
