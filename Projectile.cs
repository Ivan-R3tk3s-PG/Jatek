using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

internal sealed class Projectile
{
    public Vector2 Position, Previous, Velocity;
    public int Damage;
    public bool Friendly, Dead;
    public Color Tint;
    public float Life, Radius;

    public Projectile(Vector2 position, Vector2 velocity, int damage, bool friendly, Color tint, float lifetime = 1.7f, float radius = 4f)
    {
        Position = Previous = position;
        Velocity = velocity;
        Damage = damage;
        Friendly = friendly;
        Tint = tint;
        Life = lifetime;
        Radius = radius;
    }

    public void Update(Game game, float dt)
    {
        Previous = Position;
        Position += Velocity * dt;
        Life -= dt;
        if (Life <= 0 || Position.X < -100 || Position.X > Level.Length + 100 || Position.Y > 790 || Position.Y < -100) { Dead = true; return; }

        float closest = 2f;
        Enemy? hitEnemy = null;
        Barrel? hitBarrel = null;
        bool hitPlayer = false;
        bool hitWall = false;

        foreach (var tile in game.Level.Platforms)
        {
            if (Geometry.SegmentRectangle(Previous, Position, tile.Bounds, Radius, out var t) && t < closest)
            {
                closest = t;
                hitWall = true;
                hitEnemy = null;
                hitBarrel = null;
                hitPlayer = false;
            }
        }
        foreach (var barrel in game.Barrels)
        {
            if (barrel.Destroyed || !Geometry.SegmentRectangle(Previous, Position, barrel.Bounds, Radius, out float t) || t >= closest) continue;
            closest = t;
            hitBarrel = barrel;
            hitEnemy = null;
            hitWall = false;
            hitPlayer = false;
        }
        if (Friendly)
        {
            foreach (var enemy in game.Enemies)
            {
                if (enemy.Dead) continue;
                if (!Geometry.SegmentRectangle(Previous, Position, enemy.Bounds, Radius, out float t) || t >= closest) continue;
                closest = t;
                hitEnemy = enemy;
                hitBarrel = null;
                hitPlayer = false;
                hitWall = false;
            }
        }
        else if (Geometry.SegmentRectangle(Previous, Position, game.Player.Bounds, Radius, out var t) && t < closest)
        {
            closest = t;
            hitPlayer = true;
            hitBarrel = null;
            hitWall = false;
        }
        if (closest <= 1)
        {
            Position = Vector2.Lerp(Previous, Position, closest);
            Dead = true;
            if (hitBarrel != null) hitBarrel.Damage(game, Damage);
            else if (hitEnemy != null) hitEnemy.Damage(game, Damage, Position, Velocity);
            else if (hitPlayer) game.Player.Hurt(game, Damage, Position);
            else if (hitWall) game.Effects.Emit(Position, 6, Palette.Steel, 140);
        }
    }

    public void Draw()
    {
        Vector2 dir = Velocity.LengthSquared() > 0 ? Vector2.Normalize(Velocity) : Vector2.UnitX;
        float length = Friendly ? 22 : 15;
        Palette.LightLine(Position - dir * length, Position, Friendly ? 2.3f : 3.5f, Tint);
        Palette.Glow(Position, Friendly ? 3.5f : 4.5f, Tint, .7f);
        DrawCircleV(Position, Friendly ? 2.3f : 3.8f, Palette.White);
    }
}

internal static class Geometry
{
    // Swept point versus expanded AABB: prevents high-speed bullets tunneling through enemies.
    public static bool SegmentRectangle(Vector2 a, Vector2 b, Rectangle rect, float padding, out float t)
    {
        t = 0;
        var left = rect.X - padding;
        var right = rect.X + rect.Width + padding;
        var top = rect.Y - padding;
        var bottom = rect.Y + rect.Height + padding;
        Vector2 d = b - a;
        float near = 0, far = 1;
        if (!Slab(a.X, d.X, left, right, ref near, ref far)) return false;
        if (!Slab(a.Y, d.Y, top, bottom, ref near, ref far)) return false;
        t = near;
        return near <= 1 && far >= 0;
    }

    private static bool Slab(float start, float delta, float min, float max, ref float near, ref float far)
    {
        if (MathF.Abs(delta) < .0001f) return start >= min && start <= max;
        float a = (min - start) / delta, b = (max - start) / delta;
        if (a > b) (a, b) = (b, a);
        near = MathF.Max(near, a);
        far = MathF.Min(far, b);
        return near <= far;
    }
}
