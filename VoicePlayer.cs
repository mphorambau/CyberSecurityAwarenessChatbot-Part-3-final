using System.Media;

namespace CyberSecurityAwarenessChatbot;

/// <summary>Plays the Part 1 recorded WAV greeting when Assets/greeting.wav is supplied.</summary>
internal static class VoicePlayer
{
    private static SoundPlayer? _player;
    public static bool TryPlayGreeting()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "greeting.wav");
        if (!File.Exists(path)) return false;
        try { _player = new SoundPlayer(path); _player.Load(); _player.Play(); return true; }
        catch { return false; }
    }
}