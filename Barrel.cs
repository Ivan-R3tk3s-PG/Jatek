using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

/// <summary>A volatile set-piece. Projectile collision and radial chain reactions are handled in code.</summary>
internal sealed class Barrel
{
    public Vector2 Position; // base at ground height
    public int Health = 34;
    public bool Destroyed;
    public float HitFlash, Time, SmokeTime;
    public Vector2 Center => Position - new Vector2(0, 24);
    public Rectangle Bounds => new(Position.X - 21, Position.Y - 48, 42, 48);
    public const float BlastRadius = 164;

    public Barrel(float x, float floorY = 595) => Position = new Vector2(x, floorY);

    public void Update(float dt)
    {
        Time += dt;
        HitFlash = MathF.Max(0, HitFlash - dt);
        SmokeTime = MathF.Max(0, SmokeTime - dt);
    }

    public void Damage(Game game, int damage)
    {
        if (Destroyed) return;
        Health -= damage;
        HitFlash = .12f;
        game.Effects.Emit(Center, 5, Palette.Orange, 105);
        if (Health <= 0) Explode(game);
    }

    public void Explode(Game game)
    {
        if (Destroyed) return; // mark first so linked barrels cannot recurse forever
        Destroyed = true;
        SmokeTime = 3.2f;
        Vector2 center = Center;
        game.Effects.Emit(center, 84, Palette.Orange, 450);
        game.Effects.Emit(center, 37, Palette.Red, 345);
        game.Effects.Emit(center, 33, Palette.Steel, 300);
        game.Effects.Ring(center, Palette.Orange, 19, BlastRadius, .40f);
        game.Effects.Ring(center, Palette.White, 9, 105, .22f);
        float playerDistance = Vector2.Distance(center, game.Player.Center);
        if (playerDistance <= BlastRadius)
        {
            int damage = (int)(13 + 24 * (1 - playerDistance / BlastRadius));
            game.Player.Hurt(game, damage, game.Player.Center);
        }
        foreach (var enemy in game.Enemies)
        {
            if (enemy.Dead) continue;
            float distance = Vector2.Distance(center, enemy.Center);
            if (distance > BlastRadius + 20) continue;
            int damage = (int)(34 + 82 * (1 - Math.Clamp(distance / BlastRadius, 0, 1)));
            enemy.Damage(game, damage, center, enemy.Center - center);
        }
        game.Shake = MathF.Max(game.Shake, playerDistance < 560 ? 15 : 6);
        if (playerDistance < 250) game.RedFlash = MathF.Max(game.RedFlash, .10f);
        game.Audio.Play("explosion", playerDistance < 650 ? .75f : .27f);
        // Barrel chains are deterministic; the first one marks itself as destroyed.
        foreach (var other in game.Barrels)
        {
            if (!other.Destroyed && Vector2.Distance(center, other.Center) < 146)
                other.Explode(game);
        }
    }

    public void Draw()
    {
        if (Destroyed)
        {
            DrawEllipse((int)Position.X, (int)Position.Y + 1, 31, 7, Palette.Alpha(Color.Black, .52f));
            DrawRectangleRounded(new Rectangle(Position.X - 19, Position.Y - 11, 38, 10), .28f, 6, Palette.Steel);
            if (SmokeTime > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float rise = (3.2f - SmokeTime) * (24 + i * 15);
                    float x = Center.X + MathF.Sin(Time * (1.4f + i) + i) * (10 + i * 8);
                    DrawCircleV(new Vector2(x, Center.Y - rise), 9 + i * 7, Palette.Alpha(Palette.Muted, SmokeTime / 3.2f * .12f));
                }
            }
            return;
        }
        Color accent = HitFlash > 0 ? Palette.White : Palette.Orange;
        Vector2 c = Center;
        DrawEllipse((int)c.X, (int)(Position.Y + 3), 27, 7, Palette.Alpha(Color.Black, .48f));
        Palette.Glow(c, 17, Palette.Orange, .22f + .09f * MathF.Sin(Time * 5));
        DrawRectangleRounded(new Rectangle(Position.X - 19, Position.Y - 46, 38, 46), .24f, 6, new Color(64, 44, 42, 255));
        DrawRectangleRounded(new Rectangle(Position.X - 15, Position.Y - 43, 30, 41), .22f, 6, new Color(105, 53, 40, 255));
        DrawRectangle((int)Position.X - 20, (int)Position.Y - 45, 40, 7, Palette.Steel);
        DrawRectangle((int)Position.X - 20, (int)Position.Y - 10, 40, 7, Palette.Steel);
        DrawRectangle((int)Position.X - 17, (int)Position.Y - 31, 34, 8, Palette.Gold);
        for (int i = -12; i <= 12; i += 12)
            DrawLineEx(new Vector2(c.X + i - 4, c.Y - 7), new Vector2(c.X + i + 3, c.Y + 5), 4, Palette.Navy);
        DrawPoly(c, 3, 8, 0, accent);
        DrawCircleV(c, 2, Palette.Navy);
        DrawRectangleRounded(new Rectangle(Position.X - 12, Position.Y - 52, 24, 5), .3f, 4, Palette.Steel);
        Palette.LightLine(new Vector2(Position.X - 13, Position.Y - 51), new Vector2(Position.X + 13, Position.Y - 51), 1.5f, accent);
    }
}
