using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (Array.Exists(args, value => value.Equals("--self-test", StringComparison.OrdinalIgnoreCase)))
        {
            Environment.ExitCode = SelfTests.Run();
            return;
        }
        SetConfigFlags(ConfigFlags.Msaa4xHint);
        InitWindow(Game.ScreenWidth, Game.ScreenHeight, "NEON BREACH // Extraction Protocol");
        SetExitKey((KeyboardKey)0); // Escape pauses instead of closing the application.
        SetTargetFPS(60);
        using (var game = new Game()) game.Run();
        CloseWindow();
    }
}
