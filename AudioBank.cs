using Raylib_cs;
using static Raylib_cs.Raylib;

namespace NeonBreach;

// Optional original WAVs. If the machine has no audio device the game still works.
internal sealed class AudioBank : IDisposable
{
    private readonly Dictionary<string, Sound> _sounds = new();
    private bool _ready;

    public void Initialize()
    {
        try
        {
            InitAudioDevice();
            _ready = IsAudioDeviceReady();
            if (!_ready) return;
            foreach (string effect in new[] { "pistol", "smg", "shotgun", "hit", "hurt", "enemy", "jump", "land", "pickup", "explosion", "ui", "dash", "slide" })
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Assets", effect + ".wav");
                if (File.Exists(path)) _sounds[effect] = LoadSound(path);
            }
        }
        catch { _ready = false; } // Audio never gates gameplay.
    }

    public void Play(string effect, float volume)
    {
        if (!_ready || !_sounds.TryGetValue(effect, out var sound)) return;
        SetSoundVolume(sound, volume);
        PlaySound(sound);
    }

    public void Dispose()
    {
        if (!_ready) return;
        foreach (var sound in _sounds.Values) UnloadSound(sound);
        _sounds.Clear();
        CloseAudioDevice();
        _ready = false;
    }
}
