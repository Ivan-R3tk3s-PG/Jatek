using System.Numerics;
using Raylib_cs;

namespace NeonBreach;

/// <summary>Logic-level smoke tests; does not open a game window or initialize sound.</summary>
internal static class SelfTests
{
    public static int Run()
    {
        int passed = 0, failed = 0;
        void Check(string name, bool ok)
        {
            Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] {name}");
            if (ok) passed++; else failed++;
        }

        var level = new Level();
        Check("Four ordered sectors", Level.Section(0) == 0 && Level.Section(2000) == 1 &&
            Level.Section(3900) == 2 && Level.Section(5700) == 3);
        Check("Original weapon pickups", level.WeaponPickups.Count == 2 &&
            level.WeaponPickups[0].Kind == PickupKind.Smg && level.WeaponPickups[1].Kind == PickupKind.Shotgun);
        Check("Original platform layout", level.Platforms.Count >= 25 &&
            level.HasFloor(145) && !level.HasFloor(1000) && level.HasFloor(7450));
        Check("Upgrade and extraction positions", level.HasFloor(Level.StationX) &&
            level.HasFloor(Level.ExitX) && Level.StationX < Level.ExitX);
        var player = new Player();
        Check("Initial player position and hitbox", player.Bounds.Width == 28 &&
            player.Bounds.Height == 56 && player.Bounds.Y + player.Bounds.Height == 595);
        Check("Dash starts ready and is a distinct ability", player.DashReady && !player.IsDashing &&
            player.DashMaxCooldown > 0);
        Check("Slide has a true reduced hitbox", Player.SlideHeight < Player.Height &&
            player.CollisionHeight == Player.Height && !player.IsSliding);
        Check("Three distinct weapons", player.Weapons.Length == 3 &&
            player.Weapons[0].Owned && !player.Weapons[1].Owned && !player.Weapons[2].Owned &&
            player.Weapons[0].FireInterval > player.Weapons[1].FireInterval &&
            player.Weapons[2].FireInterval > player.Weapons[0].FireInterval);
        Check("Distinct enemies and finale", new Enemy(EnemyKind.Drone, 0, 0).Health <
            new Enemy(EnemyKind.Turret, 0, 0).Health &&
            new Enemy(EnemyKind.Turret, 0, 0).Health < new Enemy(EnemyKind.Heavy, 0, 595).Health &&
            new Enemy(EnemyKind.Heavy, 0, 595, true).IsFinal);
        Check("All barrels placed on solid floor", Level.BarrelPositions.Length >= 9 &&
            Array.TrueForAll(Level.BarrelPositions, x => level.HasFloor(x)));
        int pairs = 0;
        for (int i = 0; i < Level.BarrelPositions.Length; i++)
            for (int j = i + 1; j < Level.BarrelPositions.Length; j++)
                if (MathF.Abs(Level.BarrelPositions[i] - Level.BarrelPositions[j]) < 146) pairs++;
        Check("Chain reaction placements", pairs >= 3);
        Check("Barrel destruction defaults", new Barrel(145).Health > 0 && !new Barrel(145).Destroyed);
        Check("Swept projectile hits obstacle", Geometry.SegmentRectangle(new Vector2(-100, 10),
            new Vector2(100, 10), new Rectangle(-10, 0, 20, 20), 3, out float crossing) &&
            crossing is >= 0 and <= 1);
        Check("Swept projectile misses obstacle", !Geometry.SegmentRectangle(new Vector2(-100, 100),
            new Vector2(100, 100), new Rectangle(-10, 0, 20, 20), 3, out _));
        string[] sounds = { "pistol", "smg", "shotgun", "hit", "hurt", "enemy", "jump",
            "land", "pickup", "explosion", "ui", "dash", "slide" };
        Check("All optional WAV files packaged", sounds.All(sound =>
            File.Exists(Path.Combine(AppContext.BaseDirectory, "Assets", sound + ".wav"))));
        Console.WriteLine($"\nNEON BREACH 1.1 - {passed} passed / {failed} failed");
        return failed == 0 ? 0 : 1;
    }
}
