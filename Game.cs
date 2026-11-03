using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

internal enum GameState { Title, Playing, Paused, Upgrades, GameOver, Victory }

internal sealed class Game : IDisposable
{
    public const int ScreenWidth = 1280;
    public const int ScreenHeight = 720;
    public readonly Random Random = new(4563);
    public readonly AudioBank Audio = new();
    public readonly Effects Effects;
    public Level Level { get; private set; } = new();
    public Player Player { get; private set; } = new();
    public List<Enemy> Enemies { get; private set; } = new();
    public readonly List<Projectile> Projectiles = new();
    public readonly List<Drop> Drops = new();
    public readonly List<Barrel> Barrels = new();
    public Camera2D Camera;
    public GameState State = GameState.Title;
    public float TotalTime, PlayTime, CameraX, Shake, RedFlash, MuzzleFlash;
    public Vector2 MuzzlePosition;
    public Color MuzzleColor;
    public float CheckpointX = 145;
    public int Kills;
    public string Toast = "";
    public float ToastTime;
    public bool IsOverUpgradeStation => State == GameState.Upgrades;
    private bool _disposed;
    private int _lastSection = -1;
    private float _ambientTimer;
    private readonly Ui _ui = new();

    public bool ExitUnlocked => Enemies.Where(e => e.IsFinal).All(e => e.Dead);

    public Game()
    {
        Effects = new Effects(Random);
        Camera = new Camera2D { Target = Vector2.Zero, Offset = Vector2.Zero, Rotation = 0, Zoom = 1 };
        BuildEnemies();
        BuildBarrels();
        Audio.Initialize();
    }

    private void BuildEnemies()
    {
        Enemies = new List<Enemy>
        {
            new(EnemyKind.Drone, 510, 411), new(EnemyKind.Drone, 1165, 436),
            new(EnemyKind.Drone, 1510, 402), new(EnemyKind.Turret, 1465, 485),
            new(EnemyKind.Drone, 2110, 426), new(EnemyKind.Turret, 2405, 483),
            new(EnemyKind.Drone, 2730, 389), new(EnemyKind.Turret, 3190, 487),
            new(EnemyKind.Drone, 3400, 440), new(EnemyKind.Heavy, 3700, 595),
            new(EnemyKind.Drone, 4080, 414), new(EnemyKind.Turret, 4520, 425),
            new(EnemyKind.Heavy, 4915, 595), new(EnemyKind.Drone, 5210, 412),
            new(EnemyKind.Heavy, 5460, 595), new(EnemyKind.Turret, 6040, 484),
            new(EnemyKind.Drone, 6165, 425), new(EnemyKind.Drone, 6435, 395),
            new(EnemyKind.Drone, 6760, 417, true),
            new(EnemyKind.Turret, 7210, 422, true),
            new(EnemyKind.Heavy, 6940, 595, true),
        };
    }

    private void BuildBarrels()
    {
        Barrels.Clear();
        // Pairs are deliberately within blast range, enabling visible chain reactions.
        foreach (float x in global::NeonBreach.Level.BarrelPositions)
            Barrels.Add(new Barrel(x));
    }

    public void Run()
    {
        while (!WindowShouldClose())
        {
            float dt = Math.Clamp(GetFrameTime(), 0, 1f / 30f);
            Update(dt);
            Draw();
        }
    }

    public void NewGame()
    {
        Level = new Level();
        Player = new Player();
        Projectiles.Clear();
        Drops.Clear();
        Effects.Particles.Clear();
        Effects.Numbers.Clear();
        Effects.Shockwaves.Clear();
        BuildEnemies();
        BuildBarrels();
        CameraX = 0;
        Camera.Target = Vector2.Zero;
        PlayTime = 0;
        Kills = 0;
        CheckpointX = 145;
        RedFlash = Shake = 0;
        ToastTime = 0;
        _ambientTimer = 0;
        _lastSection = -1;
        State = GameState.Playing;
        _ui.ResetHud(Player.MaxHealth);
        Notify("MISSION START   //   REACH EXTRACTION", Palette.Cyan);
    }

    public void Die()
    {
        State = GameState.GameOver;
        Shake = MathF.Max(Shake, 15);
    }

    public void Notify(string text, Color? color = null)
    {
        Toast = text;
        ToastTime = 3.1f;
        if (color.HasValue) _ui.NotificationColor = color.Value;
    }

    public void TryUpgrade(int slot)
    {
        int price = slot switch { 0 => 45, 1 => 55, _ => 55 };
        if (slot is < 0 or > 2) return;
        bool purchased = slot switch { 0 => Player.ReinforcedArmor, 1 => Player.OverchargedRounds, _ => Player.CombatAccelerator };
        if (purchased) { Notify("UPGRADE ALREADY INSTALLED", Palette.Muted); Audio.Play("ui", .3f); return; }
        if (Player.Credits < price) { Notify("INSUFFICIENT ENERGY CREDITS", Palette.Red); Audio.Play("hurt", .16f); return; }
        Player.Credits -= price;
        switch (slot)
        {
            case 0: Player.ReinforcedArmor = true; Player.MaxHealth += 60; Player.Heal(75); Notify("REINFORCED ARMOR INSTALLED", Palette.Cyan); break;
            case 1: Player.OverchargedRounds = true; Notify("OVERCHARGED ROUNDS INSTALLED", Palette.Orange); break;
            case 2: Player.CombatAccelerator = true; Notify("COMBAT ACCELERATOR INSTALLED", Palette.Purple); break;
        }
        Effects.Emit(new Vector2(Level.StationX, 530), 40, Palette.Cyan, 160);
        Audio.Play("pickup", .45f);
        State = GameState.Playing;
    }

    private void Update(float dt)
    {
        TotalTime += dt;
        Shake = MathF.Max(0, Shake - 36 * dt);
        RedFlash = MathF.Max(0, RedFlash - dt);
        MuzzleFlash = MathF.Max(0, MuzzleFlash - dt);
        ToastTime = MathF.Max(0, ToastTime - dt);
        if (State == GameState.Title)
        {
            if (IsKeyPressed(KeyboardKey.Enter) || IsKeyPressed(KeyboardKey.Space)) { NewGame(); Audio.Play("ui", .4f); }
            return;
        }
        if (State == GameState.GameOver)
        {
            if (IsKeyPressed(KeyboardKey.R) || IsKeyPressed(KeyboardKey.Enter)) NewGame();
            return;
        }
        if (State == GameState.Victory)
        {
            if (IsKeyPressed(KeyboardKey.Enter) || IsKeyPressed(KeyboardKey.R)) State = GameState.Title;
            return;
        }
        if (State == GameState.Paused)
        {
            if (IsKeyPressed(KeyboardKey.Escape) || IsKeyPressed(KeyboardKey.P)) State = GameState.Playing;
            return;
        }
        if (State == GameState.Upgrades)
        {
            if (IsKeyPressed(KeyboardKey.Escape) || IsKeyPressed(KeyboardKey.E)) { State = GameState.Playing; Audio.Play("ui", .2f); }
            if (IsKeyPressed(KeyboardKey.One)) TryUpgrade(0);
            if (IsKeyPressed(KeyboardKey.Two)) TryUpgrade(1);
            if (IsKeyPressed(KeyboardKey.Three)) TryUpgrade(2);
            return;
        }
        if (IsKeyPressed(KeyboardKey.Escape)) { State = GameState.Paused; Audio.Play("ui", .22f); return; }

        PlayTime += dt;
        Player.Update(this, dt);
        if (State != GameState.Playing) return;

        float cameraGoal = Math.Clamp(Player.Center.X - ScreenWidth * .42f, 0, Level.Length - ScreenWidth);
        CameraX += (cameraGoal - CameraX) * Math.Clamp(dt * 5.2f, 0, 1);
        Camera.Target = new Vector2(CameraX, 0);
        int section = Level.Section(Player.Center.X);
        if (section != _lastSection)
        {
            if (_lastSection >= 0) Notify(Level.SectionName(section) + "   //   ACCESS GRANTED", Level.SectionColor(section));
            _lastSection = section;
        }
        if (Player.Center.X >= 3890 && CheckpointX < 3900) { CheckpointX = 3990; Notify("CHECKPOINT SECURED", Palette.Cyan); Player.Heal(20); }
        if (Player.Center.X >= 5830 && CheckpointX < 5830) { CheckpointX = 5910; Notify("CHECKPOINT SECURED", Palette.Cyan); Player.Heal(20); }

        foreach (var enemy in Enemies)
            if (!enemy.Dead && MathF.Abs(enemy.Position.X - Player.Position.X) < 980) enemy.Update(this, dt);
        foreach (var barrel in Barrels) barrel.Update(dt);
        for (int i = Projectiles.Count - 1; i >= 0; i--)
        {
            Projectiles[i].Update(this, dt);
            if (Projectiles[i].Dead) Projectiles.RemoveAt(i);
        }
        for (int i = Drops.Count - 1; i >= 0; i--)
        {
            Drops[i].Update(this, dt);
            if (Drops[i].Collected) Drops.RemoveAt(i);
        }
        foreach (var pickup in Level.WeaponPickups)
        {
            if (pickup.Taken || Vector2.Distance(Player.Center, pickup.Position) > 65) continue;
            pickup.Taken = true;
            var kind = pickup.Kind == PickupKind.Smg ? WeaponKind.Smg : WeaponKind.Shotgun;
            Player.Weapons[(int)kind].Owned = true;
            Player.Equipped = kind;
            Notify($"WEAPON ACQUIRED   //   {Player.CurrentWeapon.Name}", Player.CurrentWeapon.Color);
            Audio.Play("pickup", .48f);
            Effects.Emit(pickup.Position, 55, Player.CurrentWeapon.Color, 190);
        }

        if (IsKeyPressed(KeyboardKey.E))
        {
            if (Vector2.Distance(Player.Center, new Vector2(Level.StationX, 543)) < 125)
            { State = GameState.Upgrades; Audio.Play("ui", .28f); }
            else if (Player.Center.X >= Level.ExitX - 90 && ExitUnlocked)
            { State = GameState.Victory; Audio.Play("pickup", .55f); }
        }
        if (Player.Center.X > Level.ExitX - 115 && !ExitUnlocked && ToastTime <= 0)
            Notify("EXTRACTION LOCKED   //   NEUTRALIZE HOSTILES", Palette.Red);

        _ambientTimer += dt;
        if (_ambientTimer > .24f)
        {
            _ambientTimer = 0;
            float x = Player.Center.X + Random.Next(-460, 570);
            if (x > 30 && x < Level.Length - 30 && Level.HasFloor(x) && Random.NextDouble() < .7)
                Effects.Emit(new Vector2(x, 560 + Random.Next(-8, 22)), 2, Level.SectionColor(Level.Section(x)), 42);
        }
        Effects.Update(dt);
    }

    private void Draw()
    {
        BeginDrawing();
        if (State == GameState.Title)
        {
            Level.DrawBackdrop(TotalTime * 22, TotalTime);
            // A silhouette of the facility establishes the atmosphere behind the title.
            _ui.DrawTitle(this);
            EndDrawing();
            return;
        }

        Level.DrawBackdrop(CameraX, TotalTime);
        Camera.Target = new Vector2(CameraX + (Shake > 0 ? (float)(Random.NextDouble() - .5) * Shake : 0),
            Shake > 0 ? (float)(Random.NextDouble() - .5) * Shake : 0);
        BeginMode2D(Camera);
        Level.DrawWorld(TotalTime, CameraX, ExitUnlocked);
        foreach (var drop in Drops) drop.Draw(TotalTime);
        foreach (var barrel in Barrels)
            if (MathF.Abs(barrel.Position.X - Player.Position.X) < 950) barrel.Draw();
        foreach (var enemy in Enemies) if (!enemy.Dead && MathF.Abs(enemy.Position.X - Player.Position.X) < 950) enemy.Draw();
        foreach (var projectile in Projectiles) projectile.Draw();
        Player.Draw(TotalTime);
        Effects.Draw();
        if (MuzzleFlash > 0)
        {
            Palette.Glow(MuzzlePosition, 26 * MuzzleFlash / .14f, MuzzleColor, .9f);
            DrawPoly(MuzzlePosition, 4, 13 * MuzzleFlash / .14f, TotalTime * 120, Palette.White);
        }
        Level.DrawForeground(TotalTime, CameraX);
        EndMode2D();
        Camera.Target = new Vector2(CameraX, 0);
        _ui.DrawScreenFilter(TotalTime);
        _ui.DrawHud(this);
        if (RedFlash > 0) DrawRectangle(0, 0, ScreenWidth, ScreenHeight, Palette.Alpha(Palette.Red, RedFlash * .26f));
        if (State == GameState.Paused) _ui.DrawPause(this);
        if (State == GameState.Upgrades) _ui.DrawUpgrades(this);
        if (State == GameState.GameOver) _ui.DrawGameOver(this);
        if (State == GameState.Victory) _ui.DrawVictory(this);
        EndDrawing();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Audio.Dispose();
    }
}
