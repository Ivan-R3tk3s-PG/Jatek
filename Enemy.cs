using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

internal enum EnemyKind { Drone, Turret, Heavy }

internal sealed class Enemy
{
    public EnemyKind Kind;
    public Vector2 Position;
    public float SpawnX;
    public int Health, MaxHealth;
    public bool Dead;
    public float Time, Cooldown, Flash, Telegraph;
    public float Stagger;
    private float _staggerGate;
    private Vector2 _impactVelocity;
    private int _burst;
    private Vector2 _aim = Vector2.UnitX;
    public bool IsFinal;
    public Color Accent => Kind switch { EnemyKind.Drone => Palette.Orange, EnemyKind.Turret => Palette.Purple, _ => Palette.Red };

    public Rectangle Bounds => Kind switch
    {
        EnemyKind.Drone => new(Position.X - 22, Position.Y - 16, 44, 34),
        EnemyKind.Turret => new(Position.X - 24, Position.Y - 29, 48, 34),
        _ => new(Position.X - 27, Position.Y - 72, 54, 78)
    };
    public Vector2 Center => Kind == EnemyKind.Heavy ? Position + new Vector2(0, -38) : Position;

    public Enemy(EnemyKind kind, float x, float y, bool isFinal = false)
    {
        Kind = kind; Position = new Vector2(x, y); SpawnX = x; IsFinal = isFinal;
        MaxHealth = Health = kind switch { EnemyKind.Drone => 36, EnemyKind.Turret => 87, _ => isFinal ? 330 : 210 };
        Cooldown = kind switch { EnemyKind.Drone => .9f, EnemyKind.Turret => 1.2f, _ => 1.3f };
    }

    public void Update(Game game, float dt)
    {
        if (Dead) return;
        Time += dt;
        Cooldown = MathF.Max(0, Cooldown - dt);
        Flash = MathF.Max(0, Flash - dt);
        Stagger = MathF.Max(0, Stagger - dt);
        _staggerGate = MathF.Max(0, _staggerGate - dt);
        if (Kind == EnemyKind.Drone) Position += _impactVelocity * dt;
        else if (Kind == EnemyKind.Heavy && game.Level.HasFloor(Position.X + _impactVelocity.X * dt))
            Position.X += _impactVelocity.X * dt;
        _impactVelocity *= MathF.Max(0, 1 - 10f * dt);
        Vector2 toward = game.Player.Center - Center;
        float distance = toward.Length();
        if (distance > 1) _aim = Vector2.Normalize(toward);
        if (Stagger > 0) return; // A short real interruption, throttled to prevent stun-lock.
        if (Kind == EnemyKind.Drone)
        {
            // Flyer patrols and then kites around the player.
            if (distance < 650 && distance > 215)
            {
                float direction = MathF.Sign(toward.X);
                Position.X += direction * 82 * dt;
            }
            else Position.X += MathF.Sin(Time * 1.2f + SpawnX) * 11 * dt;
            Position.Y += MathF.Sin(Time * 3 + SpawnX) * 11 * dt;
            if (distance < 525 && Cooldown <= 0)
            {
                Fire(game, 355, 11);
                Cooldown = 1.55f + (float)game.Random.NextDouble() * .4f;
            }
        }
        if (Kind == EnemyKind.Turret && distance < 670 && Cooldown <= 0)
        {
            Fire(game, 480, 13);
            Cooldown = 1.9f;
        }
        if (Kind == EnemyKind.Heavy)
        {
            if (distance < 730 && MathF.Abs(toward.X) > 140 && MathF.Abs(Position.X - SpawnX) < 210)
            {
                float direction = MathF.Sign(toward.X);
                if (game.Level.HasFloor(Position.X + direction * 32)) Position.X += direction * 67 * dt;
            }
            if (distance < 670)
            {
                if (Telegraph > 0)
                {
                    Telegraph -= dt;
                    if (Telegraph <= 0)
                    {
                        _burst = 3;
                        Cooldown = .01f;
                    }
                }
                else if (_burst > 0 && Cooldown <= 0)
                {
                    Fire(game, 510, IsFinal ? 19 : 17);
                    _burst--;
                    Cooldown = _burst == 0 ? 2.9f : .18f;
                }
                else if (_burst == 0 && Cooldown <= 0)
                {
                    Telegraph = .72f;
                }
            }
        }
        // Attacks only happen when enemies are close. No off-screen damage spam.
    }

    private void Fire(Game game, float speed, int damage)
    {
        if (game.Player.IsDead) return;
        Vector2 muzzle = Kind switch
        {
            EnemyKind.Drone => Position + _aim * 25,
            EnemyKind.Turret => Position + new Vector2(0, -8) + _aim * 30,
            _ => Position + new Vector2(0, -48) + _aim * 38
        };
        game.Projectiles.Add(new Projectile(muzzle, _aim * speed, damage, false, Accent, 2.7f, Kind == EnemyKind.Heavy ? 7 : 4.5f));
        game.Effects.Emit(muzzle, Kind == EnemyKind.Heavy ? 11 : 6, Accent, 86);
        if (MathF.Abs(game.Player.Position.X - Position.X) < 700) game.Audio.Play("enemy", .17f);
    }

    public void Damage(Game game, int amount, Vector2 impact, Vector2? impactVelocity = null)
    {
        if (Dead) return;
        Health = Math.Max(0, Health - amount);
        Flash = .16f;
        if (_staggerGate <= 0)
        {
            Stagger = Kind switch { EnemyKind.Drone => .19f, EnemyKind.Turret => .09f, _ => .11f };
            _staggerGate = Kind == EnemyKind.Heavy ? .38f : .23f;
        }
        Vector2 direction = impactVelocity ?? (Center - impact);
        if (direction.LengthSquared() > .001f)
        {
            direction = Vector2.Normalize(direction);
            _impactVelocity += direction * (Kind switch { EnemyKind.Drone => 205, EnemyKind.Turret => 0, _ => 73 });
        }
        game.Effects.Emit(impact, Kind == EnemyKind.Heavy ? 13 : 7, Accent, 198);
        game.Effects.Emit(impact, 4, Palette.White, 130);
        game.Effects.Number(Center - new Vector2(0, 30), amount, Palette.White);
        game.Audio.Play("hit", .18f);
        if (Health != 0) return;
        Dead = true;
        game.Kills++;
        game.Shake = MathF.Max(game.Shake, Kind == EnemyKind.Heavy ? 14 : 5);
        game.Effects.Emit(Center, Kind == EnemyKind.Heavy ? 75 : 30, Accent, Kind == EnemyKind.Heavy ? 400 : 250);
        game.Effects.Emit(Center, 20, Palette.Orange, 210);
        game.Effects.Emit(Center, Kind == EnemyKind.Heavy ? 32 : 14, Palette.Steel, 220);
        game.Effects.Ring(Center, Accent, 11, Kind == EnemyKind.Heavy ? 120 : 58, .38f);
        game.Audio.Play("explosion", Kind == EnemyKind.Heavy ? .65f : .38f);
        game.Drops.Add(new Drop(Center, DropKind.Credits, Kind == EnemyKind.Heavy ? 45 : Kind == EnemyKind.Turret ? 20 : 12));
        if (game.Random.NextDouble() < .24) game.Drops.Add(new Drop(Center + new Vector2(12, -10), DropKind.Health, 15));
        if (game.Random.NextDouble() < .35) game.Drops.Add(new Drop(Center + new Vector2(-15, -8), DropKind.Ammo, 1));
    }

    public void Draw()
    {
        if (Dead) return;
        Color c = Flash > 0 ? Palette.White : Accent;
        if (Stagger > 0)
        {
            Vector2 pos = Kind == EnemyKind.Heavy ? Center : Position;
            DrawCircleLines((int)pos.X, (int)pos.Y, 20 + (int)(Stagger * 20), Palette.Alpha(Palette.White, .24f));
        }
        if (Kind == EnemyKind.Drone)
        {
            // Animated swept wings + armored central eye.
            float bob = MathF.Sin(Time * 8) * 3;
            Palette.Glow(Position, 13, c, .42f);
            for (int side = -1; side <= 1; side += 2)
            {
                DrawTriangle(new Vector2(Position.X + side * 8, Position.Y - 2), new Vector2(Position.X + side * 27, Position.Y - 12 + bob), new Vector2(Position.X + side * 18, Position.Y + 10), new Color(52, 60, 74, 255));
                DrawLineEx(new Vector2(Position.X + side * 7, Position.Y), new Vector2(Position.X + side * 29, Position.Y - 10 + bob), 3, c);
                DrawCircleV(new Vector2(Position.X + side * 25, Position.Y - 11 + bob), 3, Palette.White);
            }
            DrawPoly(Position, 6, 19, 30, new Color(27, 34, 49, 255));
            DrawPoly(Position, 6, 14, 30, new Color(80, 73, 72, 255));
            DrawRectangleRounded(new Rectangle(Position.X - 13, Position.Y - 5, 26, 10), .45f, 6, new Color(28, 15, 21, 255));
            DrawRectangleRounded(new Rectangle(Position.X - 10, Position.Y - 2.5f, 20, 5), .45f, 6, c);
            Palette.Glow(Position, 5, c, .58f);
        }
        else if (Kind == EnemyKind.Turret)
        {
            DrawRectangleRounded(new Rectangle(Position.X - 25, Position.Y - 4, 50, 9), .2f, 3, Palette.Steel);
            DrawLineEx(Position + new Vector2(-12, -4), Position + new Vector2(-19, -23), 6, Palette.Muted);
            DrawLineEx(Position + new Vector2(12, -4), Position + new Vector2(19, -23), 6, Palette.Muted);
            DrawPoly(Position + new Vector2(0, -19), 6, 18, 30, new Color(37, 38, 60, 255));
            DrawCircleV(Position + new Vector2(0, -19), 11, c);
            DrawCircleV(Position + new Vector2(0, -19), 6, new Color(15, 21, 39, 255));
            Palette.Rail(Position + new Vector2(0, -19), Position + new Vector2(0, -19) + _aim * 38, 10, Palette.Steel, c);
            Palette.Glow(Position + new Vector2(0, -19) + _aim * 38, 4, c, .7f);
        }
        else
        {
            float stomp = MathF.Sin(Time * 3.8f) * 2;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 hip = Position + new Vector2(side * 14, -32);
                Vector2 foot = Position + new Vector2(side * 17, stomp);
                Palette.Rail(hip, foot, 17, new Color(43, 45, 59, 255), Palette.Steel);
                DrawRectangleRounded(new Rectangle(foot.X - 13, foot.Y - 5, 25, 8), .28f, 4, Palette.Navy);
            }
            DrawPoly(Position + new Vector2(0, -47), 6, 36, 30, new Color(18, 24, 40, 255));
            DrawPoly(Position + new Vector2(0, -46), 6, 31, 30, new Color(67, 55, 67, 255));
            DrawPoly(Position + new Vector2(0, -45), 6, 24, 30, new Color(28, 34, 48, 255));
            DrawRectangleRounded(new Rectangle(Position.X - 17, Position.Y - 66, 34, 10), .3f, 4, c);
            DrawRectangleRounded(new Rectangle(Position.X - 13, Position.Y - 63, 26, 4), .3f, 4, Palette.White);
            DrawCircleV(Position + new Vector2(0, -40), 9, c);
            DrawCircleV(Position + new Vector2(0, -40), 5, Palette.Void);
            Palette.Rail(Position + new Vector2(0, -44), Position + new Vector2(0, -44) + _aim * 45, 16, Palette.Steel, c);
            if (Telegraph > 0)
            {
                float pulse = .65f + .35f * MathF.Sin(Time * 27);
                Palette.Glow(Position + new Vector2(0, -44), 19, Palette.Red, pulse);
                DrawCircleLines((int)Position.X, (int)(Position.Y - 44), (int)(19 + Telegraph * 10), Palette.Red);
            }
        }
        if (Health < MaxHealth && Health > 0)
        {
            float width = Kind == EnemyKind.Heavy ? 61 : 39;
            float y = Bounds.Y - 16;
            DrawRectangle((int)(Center.X - width / 2), (int)y, (int)width, 4, Palette.Steel);
            DrawRectangle((int)(Center.X - width / 2), (int)y, (int)(width * Health / MaxHealth), 4, c);
        }
    }
}
