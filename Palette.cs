using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace NeonBreach;

internal static class Palette
{
    public static readonly Color Void = new(7, 12, 23, 255);
    public static readonly Color Navy = new(15, 23, 39, 255);
    public static readonly Color Steel = new(40, 56, 76, 255);
    public static readonly Color Cyan = new(65, 239, 248, 255);
    public static readonly Color Ice = new(194, 252, 255, 255);
    public static readonly Color Orange = new(255, 164, 57, 255);
    public static readonly Color Red = new(255, 81, 95, 255);
    public static readonly Color Purple = new(165, 117, 250, 255);
    public static readonly Color Gold = new(255, 210, 93, 255);
    public static readonly Color White = new(230, 240, 250, 255);
    public static readonly Color Muted = new(119, 145, 163, 255);

    public static Color Alpha(Color color, float alpha) => Fade(color, Math.Clamp(alpha, 0, 1));

    public static void Glow(Vector2 point, float radius, Color color, float strength = 1f)
    {
        DrawCircleV(point, radius * 3.6f, Alpha(color, .035f * strength));
        DrawCircleV(point, radius * 2.3f, Alpha(color, .065f * strength));
        DrawCircleV(point, radius * 1.45f, Alpha(color, .13f * strength));
        DrawCircleV(point, radius, Alpha(color, .35f * strength));
    }

    public static void LightLine(Vector2 a, Vector2 b, float width, Color color)
    {
        DrawLineEx(a, b, width * 3.8f, Alpha(color, .09f));
        DrawLineEx(a, b, width * 1.9f, Alpha(color, .23f));
        DrawLineEx(a, b, width, color);
    }

    public static void Panel(Rectangle r, Color edge)
    {
        DrawRectangleRounded(r, .08f, 6, new Color(12, 21, 36, 244));
        DrawRectangleRoundedLinesEx(r, .08f, 6, 1.5f, Alpha(edge, .6f));
        DrawRectangle((int)r.X + 14, (int)r.Y + 1, (int)r.Width - 28, 2, Alpha(edge, .9f));
    }

    public static void Label(string text, float x, float y, int fontSize, Color color)
    {
        DrawText(text, (int)x, (int)y, fontSize, color);
    }

    public static void Centered(string text, float cx, float y, int size, Color color)
    {
        DrawText(text, (int)(cx - MeasureText(text, size) / 2f), (int)y, size, color);
    }

    public static void Rail(Vector2 a, Vector2 b, float thickness, Color fill, Color highlight)
    {
        DrawLineEx(a + new Vector2(2, 3), b + new Vector2(2, 3), thickness + 3, Alpha(Color.Black, .55f));
        DrawLineEx(a, b, thickness, fill);
        DrawLineEx(a - new Vector2(0, thickness * .19f), b - new Vector2(0, thickness * .19f), Math.Max(1.3f, thickness * .12f), highlight);
    }

    public static bool Button(Rectangle area, string label, Color accent, bool enabled = true)
    {
        var hover = enabled && CheckCollisionPointRec(GetMousePosition(), area);
        DrawRectangleRounded(area, .12f, 8, hover ? Alpha(accent, .25f) : new Color(21, 36, 53, 242));
        DrawRectangleRoundedLinesEx(area, .12f, 8, hover ? 3 : 1.5f, Alpha(accent, hover ? 1f : .6f));
        if (hover) DrawRectangle((int)area.X + 12, (int)(area.Y + area.Height - 4), (int)area.Width - 24, 2, accent);
        Centered(label, area.X + area.Width / 2, area.Y + (area.Height - 24) / 2, 24, enabled ? Palette.White : Palette.Muted);
        return hover && IsMouseButtonPressed(MouseButton.Left);
    }
}
