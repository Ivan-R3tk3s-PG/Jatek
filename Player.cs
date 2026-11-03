using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

internal sealed class Player
{
    public Vector2 Position = new(145, 539);
    public Vector2 Velocity;
    public const float Width = 28, Height = 56, SlideHeight = 27;
    public int Health = 100, MaxHealth = 100, Credits;
    public bool ReinforcedArmor, OverchargedRounds, CombatAccelerator;
    public bool Grounded = true;
    public bool IsDead => Health <= 0;
    public bool IsDashing => DashTime > 0;
    public bool IsSliding { get; private set; }
    public bool DashReady => DashCooldown <= 0;
    public float DashMaxCooldown => CombatAccelerator ? .82f : DashRecovery;
    public float DashTime, DashCooldown, SlideTime;
    public float Invulnerable, Recoil, LandPulse, RunPhase;
    public Vector2 Aim = Vector2.UnitX;
    public WeaponKind Equipped = WeaponKind.Sidearm;
    public readonly Weapon[] Weapons = { new(WeaponKind.Sidearm), new(WeaponKind.Smg), new(WeaponKind.Shotgun) };
    private const float DashDuration = .175f;
    private const float DashRecovery = 1.05f;
    private float _coyoteTime, _jumpBuffer, _trailClock, _slideDustClock;
    private bool _wasGrounded = true;
    private float _dashDirection = 1, _slideDirection = 1;
    private readonly List<DashEcho> _echoes = new();

    private readonly record struct DashEcho(Vector2 Position, float Remaining);
    public float CollisionHeight => IsSliding ? SlideHeight : Height;
    public Rectangle Bounds => new(Position.X, Position.Y, Width, CollisionHeight);
    public Vector2 Center => Position + new Vector2(Width / 2, CollisionHeight / 2);
    public Weapon CurrentWeapon => Weapons[(int)Equipped];
    public Vector2 GunTip => Center + new Vector2(0, IsSliding ? -1 : -7) + Aim * (Equipped == WeaponKind.Shotgun ? 37 : 31);

    public void Update(Game game, float dt)
    {
        Invulnerable = MathF.Max(0, Invulnerable - dt);
        Recoil = MathF.Max(0, Recoil - 62 * dt);
        LandPulse = MathF.Max(0, LandPulse - 3.4f * dt);
        DashCooldown = MathF.Max(0, DashCooldown - dt);
        foreach (var w in Weapons) w.Update(dt);
        for (int i = _echoes.Count - 1; i >= 0; i--)
        {
            var echo = _echoes[i] with { Remaining = _echoes[i].Remaining - dt };
            if (echo.Remaining <= 0) _echoes.RemoveAt(i);
            else _echoes[i] = echo;
        }

        if (IsKeyPressed(KeyboardKey.One)) Equip(WeaponKind.Sidearm);
        if (IsKeyPressed(KeyboardKey.Two)) Equip(WeaponKind.Smg);
        if (IsKeyPressed(KeyboardKey.Three)) Equip(WeaponKind.Shotgun);
        if (IsKeyPressed(KeyboardKey.R)) CurrentWeapon.Reload();
        float wheel = GetMouseWheelMove();
        if (wheel != 0)
        {
            int step = Math.Sign(wheel);
            for (int i = 1; i < 4; i++)
            {
                var target = (WeaponKind)(((int)Equipped + step * i + 12) % 3);
                if (Weapons[(int)target].Owned) { Equipped = target; break; }
            }
        }

        Vector2 worldMouse = GetMousePosition() + game.Camera.Target;
        Vector2 delta = worldMouse - (Center + new Vector2(0, -7));
        if (delta.LengthSquared() > 2) Aim = Vector2.Normalize(delta);
        float input = (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right) ? 1 : 0) -
                      (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left) ? 1 : 0);
        float speed = CombatAccelerator ? 372 : 327;

        // Slide is a real low hitbox; the feet remain planted at the same height.
        if (!IsDashing && !IsSliding && Grounded && input != 0 &&
            (IsKeyPressed(KeyboardKey.C) || IsKeyPressed(KeyboardKey.LeftControl)))
        {
            IsSliding = true;
            SlideTime = .55f;
            _slideDirection = input;
            _slideDustClock = 0;
            Position.Y += Height - SlideHeight;
            Velocity.X = _slideDirection * MathF.Max(MathF.Abs(Velocity.X), 560);
            game.Effects.Emit(new Vector2(Center.X, Position.Y + SlideHeight), 15, Palette.Muted, 115);
            game.Audio.Play("slide", .30f);
        }
        if (IsSliding)
        {
            SlideTime = MathF.Max(0, SlideTime - dt);
            float slideSpeed = 285 + 290 * Math.Clamp(SlideTime / .55f, 0, 1);
            Velocity.X = _slideDirection * slideSpeed;
            _slideDustClock += dt;
            if (_slideDustClock > .055f)
            {
                _slideDustClock = 0;
                game.Effects.Emit(new Vector2(Center.X - _slideDirection * 14, Position.Y + SlideHeight - 2), 3, Palette.Muted, 50);
            }
            if ((!Grounded || SlideTime <= 0) && CanStand(game)) StopSlide();
        }

        // Air or ground dash. A hard collision immediately cancels its travel.
        if (DashCooldown <= 0 && (IsKeyPressed(KeyboardKey.LeftShift) || IsKeyPressed(KeyboardKey.Q)))
        {
            if (IsSliding && CanStand(game)) StopSlide();
            if (!IsSliding)
            {
                _dashDirection = input != 0 ? input : Aim.X < 0 ? -1 : 1;
                DashTime = DashDuration;
                DashCooldown = DashMaxCooldown;
                Velocity.X = 870 * _dashDirection;
                Velocity.Y = 0;
                _trailClock = 0;
                game.Shake = MathF.Max(game.Shake, 3.5f);
                game.Effects.Ring(Center, Palette.Cyan, 10, 55, .25f);
                game.Audio.Play("dash", .42f);
            }
        }
        if (DashTime > 0)
        {
            DashTime = MathF.Max(0, DashTime - dt);
            Velocity.X = 870 * _dashDirection;
            Velocity.Y = 0;
            _trailClock += dt;
            if (_trailClock > .027f)
            {
                _trailClock = 0;
                if (_echoes.Count < 12) _echoes.Add(new DashEcho(Position, .22f));
                game.Effects.Emit(Center - new Vector2(_dashDirection * 17, 0), 3, Palette.Cyan, 90);
            }
        }
        else if (!IsSliding)
        {
            float targetX = input * speed;
            float accel = Grounded ? 2500 : 1670;
            Velocity.X = MoveTowards(Velocity.X, targetX, (input == 0 ? 2850 : accel) * dt);
        }
        RunPhase += MathF.Abs(Velocity.X) * dt * (IsSliding ? .025f : .048f);

        if (IsKeyPressed(KeyboardKey.Space) || IsKeyPressed(KeyboardKey.W)) _jumpBuffer = .13f;
        else _jumpBuffer = MathF.Max(0, _jumpBuffer - dt);
        if (Grounded) _coyoteTime = .115f;
        else _coyoteTime = MathF.Max(0, _coyoteTime - dt);

        if (_jumpBuffer > 0 && _coyoteTime > 0 && (!IsSliding || CanStand(game)))
        {
            if (IsSliding) StopSlide();
            DashTime = 0;
            Velocity.Y = -685;
            Grounded = false;
            _coyoteTime = 0;
            _jumpBuffer = 0;
            game.Effects.Emit(new Vector2(Center.X, Position.Y + CollisionHeight), 8, Palette.Cyan, 78);
            game.Audio.Play("jump", .32f);
        }
        if (IsKeyReleased(KeyboardKey.Space) && Velocity.Y < -225) Velocity.Y *= .54f;
        if (!IsDashing) Velocity.Y = MathF.Min(Velocity.Y + 1760 * dt, 950);

        float oldX = Position.X;
        Position.X = Math.Clamp(Position.X + Velocity.X * dt, 0, Level.Length - Width);
        foreach (var p in game.Level.Platforms)
        {
            if (p.OneWay || !CheckCollisionRecs(Bounds, p.Bounds)) continue;
            if (Velocity.X > 0 && oldX + Width <= p.Bounds.X + 3) Position.X = p.Bounds.X - Width;
            else if (Velocity.X < 0 && oldX >= p.Bounds.X + p.Bounds.Width - 3) Position.X = p.Bounds.X + p.Bounds.Width;
            else continue;
            Velocity.X = 0;
            DashTime = 0;
            if (IsSliding && CanStand(game)) StopSlide();
        }

        float oldTop = Position.Y;
        float oldBottom = oldTop + CollisionHeight;
        Position.Y += Velocity.Y * dt;
        _wasGrounded = Grounded;
        Grounded = false;
        foreach (var p in game.Level.Platforms)
        {
            bool horizontal = Position.X + Width > p.Bounds.X + 2 && Position.X < p.Bounds.X + p.Bounds.Width - 2;
            if (!horizontal) continue;
            if (Velocity.Y >= 0 && oldBottom <= p.Bounds.Y + 6 && Position.Y + CollisionHeight >= p.Bounds.Y)
            {
                Position.Y = p.Bounds.Y - CollisionHeight;
                Velocity.Y = 0;
                Grounded = true;
            }
            else if (Velocity.Y < 0 && !p.OneWay && oldTop >= p.Bounds.Y + p.Bounds.Height - 2 && Position.Y <= p.Bounds.Y + p.Bounds.Height)
            {
                Position.Y = p.Bounds.Y + p.Bounds.Height;
                Velocity.Y = 0;
            }
        }
        if (IsSliding && !Grounded && CanStand(game)) StopSlide();
        if (Grounded && !_wasGrounded)
        {
            LandPulse = 1f;
            game.Effects.Emit(new Vector2(Center.X, Position.Y + CollisionHeight - 3), 13, Palette.Muted, 95);
            game.Effects.Ring(new Vector2(Center.X, Position.Y + CollisionHeight), Palette.Cyan, 13, 65, .30f);
            game.Audio.Play("land", .21f);
        }

        if (Position.Y > 860)
        {
            Hurt(game, 22, Center, true);
            if (IsSliding) { IsSliding = false; SlideTime = 0; }
            DashTime = 0;
            Position = new Vector2(game.CheckpointX, 539);
            Velocity = Vector2.Zero;
            Grounded = true;
        }
        if (IsMouseButtonDown(MouseButton.Left) && !game.IsOverUpgradeStation && !IsDashing)
            CurrentWeapon.Shoot(game, GunTip, Aim);
    }

    private bool CanStand(Game game)
    {
        if (!IsSliding) return true;
        var stand = new Rectangle(Position.X, Position.Y - (Height - SlideHeight), Width, Height - 1);
        foreach (var p in game.Level.Platforms)
            if (!p.OneWay && CheckCollisionRecs(stand, p.Bounds)) return false;
        return true;
    }

    private void StopSlide()
    {
        if (!IsSliding) return;
        Position.Y -= Height - SlideHeight;
        IsSliding = false;
        SlideTime = 0;
    }

    public void Equip(WeaponKind kind)
    {
        if (Weapons[(int)kind].Owned) Equipped = kind;
    }

    public void Hurt(Game game, int damage, Vector2 impact, bool ignoreInvuln = false)
    {
        if (IsDead || (!ignoreInvuln && (Invulnerable > 0 || DashTime > .025f))) return;
        Health = Math.Max(0, Health - damage);
        Invulnerable = 1.05f;
        game.Shake = MathF.Max(game.Shake, 8f);
        game.RedFlash = .2f;
        game.Effects.Emit(impact, 19, Palette.Red, 200);
        game.Effects.Number(Center + new Vector2(0, -25), damage, Palette.Red);
        game.Audio.Play("hurt", .55f);
        if (Health == 0) game.Die();
    }

    public void Heal(int value) => Health = Math.Min(MaxHealth, Health + value);

    private static float MoveTowards(float current, float target, float delta)
    {
        if (current < target) return MathF.Min(current + delta, target);
        return MathF.Max(current - delta, target);
    }

    public void Draw(float time)
    {
        foreach (var echo in _echoes)
        {
            float opacity = Math.Clamp(echo.Remaining / .22f, 0, 1);
            Vector2 e = echo.Position + new Vector2(Width / 2, Height / 2);
            DrawRectangleRounded(new Rectangle(e.X - 12, e.Y - 21, 24, 38), .3f, 5, Palette.Alpha(Palette.Cyan, opacity * .14f));
            DrawCircleV(e - new Vector2(0, 17), 12, Palette.Alpha(Palette.Ice, opacity * .22f));
            Palette.Glow(e, 15, Palette.Cyan, opacity * .24f);
        }
        if (IsSliding) { DrawSlide(time); return; }
        if (Invulnerable > 0 && ((int)(Invulnerable * 15) % 3) == 0) return;
        float cx = Center.X, y = Position.Y;
        float run = Grounded ? MathF.Sin(RunPhase) * Math.Clamp(MathF.Abs(Velocity.X) / 290, 0, 1) : .28f;
        float lean = IsDashing ? _dashDirection * 5 : Math.Clamp(Velocity.X / 330f, -1, 1) * 2.3f;
        float face = Aim.X >= 0 ? 1 : -1;
        // Shadow and servo-assisted leg animation.
        DrawEllipse((int)cx, (int)(y + Height + 2), 28, 5, Palette.Alpha(Color.Black, .45f));
        if (LandPulse > 0) DrawCircleLines((int)cx, (int)(y + Height), (int)(28 + (1 - LandPulse) * 25), Palette.Alpha(Palette.Cyan, LandPulse * .5f));
        for (int leg = -1; leg <= 1; leg += 2)
        {
            float phase = run * leg;
            Vector2 hip = new(cx + leg * 6, y + 33);
            Vector2 knee = new(cx + leg * 8 + phase * 7, y + 44 + (Grounded ? MathF.Abs(phase) * 2 : -1));
            Vector2 foot = new(cx + leg * 8 + phase * 13, y + 55 - MathF.Max(phase, 0) * 5);
            Palette.Rail(hip, knee, 9, new Color(31, 50, 67, 255), Palette.Steel);
            Palette.Rail(knee, foot, 8, new Color(37, 61, 78, 255), Palette.Muted);
            DrawRectangleRounded(new Rectangle(foot.X - 7, foot.Y - 4, 17, 7), .28f, 4, Palette.Navy);
            DrawLineEx(foot + new Vector2(-4, 2), foot + new Vector2(10, 2), 2, Palette.Cyan);
        }

        // Jet-black armored torso, luminous reactor, shoulder pads.
        DrawPoly(new Vector2(cx, y + 26), 6, 21, 30, new Color(9, 18, 31, 255));
        DrawPoly(new Vector2(cx, y + 24), 6, 18, 30, new Color(45, 69, 90, 255));
        DrawPoly(new Vector2(cx, y + 24), 6, 14, 30, new Color(18, 32, 49, 255));
        DrawRectangle((int)cx - 13, (int)y + 16, 26, 3, Palette.Alpha(Palette.Cyan, .5f));
        Palette.Glow(new Vector2(cx, y + 26), 5, Palette.Cyan, .45f);
        DrawCircleV(new Vector2(cx, y + 26), 5.4f, Palette.Cyan);
        DrawCircleV(new Vector2(cx, y + 26), 2.3f, Palette.Ice);
        DrawLineEx(new Vector2(cx - 16 + lean, y + 20), new Vector2(cx - 12 + lean, y + 35), 3, Palette.Cyan);
        DrawLineEx(new Vector2(cx + 16 + lean, y + 20), new Vector2(cx + 12 + lean, y + 35), 3, Palette.Cyan);
        DrawRectangleRounded(new Rectangle(cx - 14, y + 35, 28, 5), .3f, 4, Palette.Steel);

        DrawCircleV(new Vector2(cx, y + 8), 14.2f, new Color(23, 38, 55, 255));
        DrawCircleV(new Vector2(cx, y + 8), 11.7f, new Color(50, 70, 88, 255));
        DrawRectangleRounded(new Rectangle(cx - 11, y + 4.5f, 22, 8), .4f, 6, new Color(4, 29, 43, 255));
        DrawRectangleRounded(new Rectangle(cx + face * 1 - 9, y + 6, 17, 4.5f), .4f, 6, Palette.Cyan);
        Palette.Glow(new Vector2(cx + face * 5, y + 8), 5f, Palette.Cyan, .3f);
        DrawLineEx(new Vector2(cx - 9, y - 3), new Vector2(cx + 7, y - 3), 2, Palette.Muted);

        Vector2 shoulder = new(cx + face * 12, y + 20);
        Vector2 elbow = shoulder + Aim * 14 + new Vector2(0, 3);
        Vector2 grip = new Vector2(cx, y + 21) + Aim * 23 - Aim * Recoil;
        Palette.Rail(shoulder, elbow, 11, Palette.Steel, Palette.Muted);
        Palette.Rail(elbow, grip, 8, new Color(22, 34, 49, 255), Palette.Steel);
        DrawCircleV(shoulder, 8, new Color(68, 92, 111, 255));
        DrawCircleV(shoulder, 4, Palette.Cyan);

        // Gun is aligned to mouse aim and differs by weapon model.
        float angle = MathF.Atan2(Aim.Y, Aim.X) * 180f / MathF.PI;
        float gunLen = Equipped switch { WeaponKind.Sidearm => 29, WeaponKind.Smg => 40, _ => 45 };
        Vector2 pivot = new Vector2(cx, y + 21) + Aim * (10 - Recoil);
        Vector2 barrel = pivot + Aim * gunLen;
        Palette.Rail(pivot, barrel, Equipped == WeaponKind.Shotgun ? 12 : 9, new Color(21, 33, 46, 255), CurrentWeapon.Color);
        DrawRectanglePro(new Rectangle(pivot.X + Aim.X * 9, pivot.Y + Aim.Y * 9, Equipped == WeaponKind.Shotgun ? 20 : 11, 5), new Vector2(0, 2.5f), angle, Palette.Steel);
        DrawCircleV(pivot, 6, Palette.Navy);
        Palette.Glow(barrel, 3, CurrentWeapon.Color, .5f);
        DrawCircleV(barrel, 3, CurrentWeapon.Color);
        if (IsDashing)
        {
            Vector2 origin = new(cx - _dashDirection * 9, y + 29);
            Palette.LightLine(origin, origin - new Vector2(_dashDirection * 34, 0), 2.4f, Palette.Cyan);
            DrawTriangle(origin, origin - new Vector2(_dashDirection * 21, -8), origin - new Vector2(_dashDirection * 21, 8), Palette.Alpha(Palette.Cyan, .45f));
        }
    }

    private void DrawSlide(float time)
    {
        float x = Center.X, feet = Position.Y + CollisionHeight;
        float dir = _slideDirection;
        DrawEllipse((int)x, (int)(feet + 2), 36, 6, Palette.Alpha(Color.Black, .55f));
        Palette.LightLine(new Vector2(x - dir * 30, feet - 7), new Vector2(x + dir * 15, feet - 7), 2, Palette.Cyan);
        // Low profile: one extended armored leg, a tucked leg, tilted torso and helmet.
        Palette.Rail(new Vector2(x - dir * 2, feet - 11), new Vector2(x + dir * 25, feet - 5), 10, Palette.Navy, Palette.Steel);
        DrawRectangleRounded(new Rectangle(x + dir * 26 - 7, feet - 9, 19, 9), .3f, 5, Palette.Steel);
        Palette.Rail(new Vector2(x - dir * 7, feet - 11), new Vector2(x - dir * 24, feet - 3), 10, Palette.Navy, Palette.Muted);
        DrawRectangleRounded(new Rectangle(x - 18, feet - 25, 35, 19), .3f, 6, new Color(36, 59, 79, 255));
        DrawRectangleRounded(new Rectangle(x - 13, feet - 23, 27, 15), .35f, 6, Palette.Navy);
        Palette.Glow(new Vector2(x, feet - 16), 6, Palette.Cyan, .45f);
        DrawCircleV(new Vector2(x, feet - 16), 4, Palette.Cyan);
        Vector2 head = new(x + dir * 18, feet - 23);
        DrawCircleV(head, 11, Palette.Steel);
        DrawCircleV(head, 8.5f, Palette.Navy);
        DrawRectangleRounded(new Rectangle(head.X - 9, head.Y - 3, 19, 5), .3f, 4, Palette.Cyan);
        Vector2 gun = head + new Vector2(dir * 14, 7);
        Palette.Rail(gun, gun + Aim * 27, 7, Palette.Navy, CurrentWeapon.Color);
        if (Invulnerable > 0) Palette.Glow(head, 11, Palette.Red, .2f);
    }
}
