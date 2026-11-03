using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

internal enum WeaponKind { Sidearm, Smg, Shotgun }

internal sealed class Weapon
{
    public WeaponKind Kind { get; }
    public string Name => Kind switch { WeaponKind.Sidearm => "V9 SIDEARM", WeaponKind.Smg => "VIPER SMG", _ => "RAVAGER SHOTGUN" };
    public Color Color => Kind switch { WeaponKind.Sidearm => Palette.Cyan, WeaponKind.Smg => Palette.Orange, _ => Palette.Red };
    public int MagazineSize => Kind switch { WeaponKind.Sidearm => 999, WeaponKind.Smg => 36, _ => 7 };
    public float FireInterval => Kind switch { WeaponKind.Sidearm => .22f, WeaponKind.Smg => .082f, _ => .76f };
    public float ReloadDuration => Kind switch { WeaponKind.Sidearm => 0f, WeaponKind.Smg => 1.18f, _ => 1.65f };
    public int BaseDamage => Kind switch { WeaponKind.Sidearm => 16, WeaponKind.Smg => 7, _ => 12 };
    public bool HasInfiniteAmmo => Kind == WeaponKind.Sidearm;
    public bool Owned { get; set; }
    public int Magazine { get; private set; }
    public int Reserve { get; private set; }
    public float Cooldown { get; private set; }
    public float ReloadTimer { get; private set; }
    public bool Reloading => ReloadTimer > 0;

    public Weapon(WeaponKind kind)
    {
        Kind = kind;
        Owned = kind == WeaponKind.Sidearm;
        Magazine = MagazineSize;
        Reserve = kind switch { WeaponKind.Sidearm => 0, WeaponKind.Smg => 216, _ => 49 };
    }

    public void Update(float dt)
    {
        Cooldown = MathF.Max(0, Cooldown - dt);
        if (ReloadTimer <= 0) return;
        ReloadTimer -= dt;
        if (ReloadTimer > 0) return;
        ReloadTimer = 0;
        var amount = Math.Min(MagazineSize - Magazine, Reserve);
        Magazine += amount;
        Reserve -= amount;
    }

    public void Reload()
    {
        if (HasInfiniteAmmo || Reloading || Magazine == MagazineSize || Reserve == 0) return;
        ReloadTimer = ReloadDuration;
    }

    public bool Shoot(Game game, Vector2 origin, Vector2 direction)
    {
        if (!Owned || Cooldown > 0 || Reloading) return false;
        if (!HasInfiniteAmmo && Magazine <= 0) { Reload(); return false; }
        if (!HasInfiniteAmmo) Magazine--;
        Cooldown = FireInterval * (game.Player.CombatAccelerator ? .78f : 1f);
        int damage = BaseDamage + (game.Player.OverchargedRounds ? 5 : 0);
        if (Kind == WeaponKind.Shotgun)
        {
            for (int i = 0; i < 8; i++)
            {
                var spread = (i - 3.5f) * .075f + (float)(game.Random.NextDouble() - .5) * .038f;
                game.Projectiles.Add(new Projectile(origin, Rotate(direction, spread) * 930f, damage, true, Color, .42f, 2.8f));
            }
            game.Player.Recoil = 14f;
            game.Shake = MathF.Max(game.Shake, 9f);
            game.Effects.Emit(origin, 18, Color, 170f);
            game.Audio.Play("shotgun", .62f);
        }
        else
        {
            float spread = Kind == WeaponKind.Smg ? (float)(game.Random.NextDouble() - .5f) * .11f : 0f;
            game.Projectiles.Add(new Projectile(origin, Rotate(direction, spread) * (Kind == WeaponKind.Smg ? 1160f : 1270f), damage, true, Color, Kind == WeaponKind.Smg ? .7f : .95f, Kind == WeaponKind.Smg ? 2.7f : 3.5f));
            game.Player.Recoil = Kind == WeaponKind.Smg ? 3.5f : 6f;
            game.Shake = MathF.Max(game.Shake, Kind == WeaponKind.Smg ? 1.3f : 2.4f);
            game.Effects.Emit(origin, Kind == WeaponKind.Smg ? 5 : 8, Color, 95f);
            game.Audio.Play(Kind == WeaponKind.Smg ? "smg" : "pistol", Kind == WeaponKind.Smg ? .23f : .4f);
        }
        game.MuzzlePosition = origin;
        game.MuzzleColor = Color;
        game.MuzzleFlash = Kind == WeaponKind.Shotgun ? .14f : .095f;
        if (Magazine == 0 && Reserve > 0) Reload();
        return true;
    }

    public void AddAmmo(int count) { if (!HasInfiniteAmmo) Reserve += count; }

    public static Vector2 Rotate(Vector2 value, float angle)
    {
        float c = MathF.Cos(angle), s = MathF.Sin(angle);
        return new Vector2(value.X * c - value.Y * s, value.X * s + value.Y * c);
    }
}
