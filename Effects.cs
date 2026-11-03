using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

internal enum DropKind { Credits, Health, Ammo }
internal sealed class Drop
{
    public Vector2 Position, Velocity;
    public DropKind Kind;
    public int Amount;
    public float Life = 19f, Age;
    public bool Collected;
    public Color Color => Kind switch { DropKind.Credits => Palette.Gold, DropKind.Health => Palette.Cyan, _ => Palette.Purple };

    public Drop(Vector2 position, DropKind kind, int amount)
    {
        Position = position;
        Kind = kind;
        Amount = amount;
        Velocity = new Vector2(kind == DropKind.Health ? 75 : -65, -170);
    }

    public void Update(Game game, float dt)
    {
        Life -= dt;
        Age += dt;
        if (Life <= 0) { Collected = true; return; }
        Vector2 delta = game.Player.Center - Position;
        float dist = delta.Length();
        if (dist < 130 && dist > .01f)
        {
            Velocity = Vector2.Lerp(Velocity, delta / dist * 490, Math.Clamp(dt * 9, 0, 1));
        }
        else
        {
            Velocity.Y = MathF.Min(460, Velocity.Y + 950 * dt);
            // Pickups rest on platforms instead of falling off the level.
            foreach (var p in game.Level.Platforms)
            {
                if (Velocity.Y > 0 && Position.X > p.Bounds.X && Position.X < p.Bounds.X + p.Bounds.Width &&
                    Position.Y <= p.Bounds.Y - 4 && Position.Y + Velocity.Y * dt >= p.Bounds.Y - 4)
                { Position.Y = p.Bounds.Y - 4; Velocity.Y = 0; break; }
            }
        }
        Position += Velocity * dt;
        if (dist < 26)
        {
            Collected = true;
            if (Kind == DropKind.Credits) game.Player.Credits += Amount;
            else if (Kind == DropKind.Health) game.Player.Heal(Amount);
            else { game.Player.Weapons[1].AddAmmo(30); game.Player.Weapons[2].AddAmmo(8); }
            game.Effects.Emit(Position, 14, Color, 130f);
            game.Audio.Play("pickup", .3f);
        }
    }

    public void Draw(float time)
    {
        if (Collected) return;
        float t = MathF.Sin(time * 4 + Position.X) * 2;
        Vector2 p = Position + new Vector2(0, t);
        Palette.Glow(p, 7, Color, .6f);
        DrawPoly(p, Kind == DropKind.Credits ? 6 : 4, 8, time * 80, Color);
        DrawCircleV(p, 2, Palette.White);
    }
}

internal sealed class Particle
{
    public Vector2 Position, Velocity;
    public Color Color;
    public float Life, Maximum, Size;
    public Particle(Vector2 position, Vector2 velocity, Color color, float life, float size)
    { Position = position; Velocity = velocity; Color = color; Maximum = Life = life; Size = size; }
    public void Update(float dt) { Life -= dt; Position += Velocity * dt; Velocity.Y += 340 * dt; Velocity *= MathF.Max(0, 1 - dt * 1.3f); }
    public void Draw()
    {
        float alpha = Math.Clamp(Life / Maximum, 0, 1);
        if (alpha <= 0) return;
        if (Size > 2.8f) Palette.Glow(Position, Size * .8f, Color, alpha * .25f);
        DrawLineEx(Position, Position - Velocity * .021f, MathF.Max(1, Size * alpha), Palette.Alpha(Color, alpha));
    }
}

internal sealed class DamageNumber
{
    public Vector2 Position;
    public int Amount;
    public Color Color;
    public float Life = .72f;
    public DamageNumber(Vector2 position, int amount, Color color) { Position = position; Amount = amount; Color = color; }
    public void Update(float dt) { Position.Y -= 43 * dt; Life -= dt; }
    public void Draw() => DrawText(Amount.ToString(), (int)Position.X, (int)Position.Y, 19, Palette.Alpha(Color, Life / .72f));
}

internal sealed class Shockwave
{
    public Vector2 Origin;
    public Color Color;
    public float StartRadius, EndRadius, Age, Duration;
    public Shockwave(Vector2 origin, Color color, float start, float end, float duration)
    { Origin = origin; Color = color; StartRadius = start; EndRadius = end; Duration = duration; }
    public bool Finished => Age >= Duration;
    public void Update(float dt) => Age += dt;
    public void Draw()
    {
        float f = Math.Clamp(Age / Duration, 0, 1);
        float radius = StartRadius + (EndRadius - StartRadius) * (1 - (1 - f) * (1 - f));
        Color tint = Palette.Alpha(Color, (1 - f) * .72f);
        DrawCircleLines((int)Origin.X, (int)Origin.Y, Math.Max(1, (int)radius), tint);
        DrawCircleLines((int)Origin.X, (int)Origin.Y, Math.Max(1, (int)(radius + 3)), Palette.Alpha(Color, (1 - f) * .20f));
    }
}

internal sealed class Effects
{
    private readonly Random _random;
    public readonly List<Particle> Particles = new();
    public readonly List<DamageNumber> Numbers = new();
    public readonly List<Shockwave> Shockwaves = new();
    public Effects(Random random) { _random = random; }
    public void Emit(Vector2 origin, int count, Color color, float speed)
    {
        for (int i = 0; i < count && Particles.Count < 600; i++)
        {
            float angle = (float)_random.NextDouble() * MathF.Tau;
            float magnitude = (float)(.25 + _random.NextDouble() * .85) * speed;
            var velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * magnitude;
            Particles.Add(new Particle(origin, velocity, color, .18f + (float)_random.NextDouble() * .57f, 1 + (float)_random.NextDouble() * 3.4f));
        }
    }
    public void Ring(Vector2 origin, Color color, float start, float end, float duration)
    {
        if (Shockwaves.Count < 45) Shockwaves.Add(new Shockwave(origin, color, start, end, duration));
    }
    public void Number(Vector2 origin, int amount, Color color)
    {
        if (Numbers.Count < 80) Numbers.Add(new DamageNumber(origin, amount, color));
    }
    public void Update(float dt)
    {
        for (int i = Particles.Count - 1; i >= 0; i--) { Particles[i].Update(dt); if (Particles[i].Life <= 0) Particles.RemoveAt(i); }
        for (int i = Numbers.Count - 1; i >= 0; i--) { Numbers[i].Update(dt); if (Numbers[i].Life <= 0) Numbers.RemoveAt(i); }
        for (int i = Shockwaves.Count - 1; i >= 0; i--) { Shockwaves[i].Update(dt); if (Shockwaves[i].Finished) Shockwaves.RemoveAt(i); }
    }
    public void Draw()
    {
        foreach (var wave in Shockwaves) wave.Draw();
        foreach (var p in Particles) p.Draw();
        foreach (var n in Numbers) n.Draw();
    }
}
