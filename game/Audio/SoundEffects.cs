using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Audio;

// Plays at the player's volume setting whatever volume the server asks for, and skips a sound when every channel is
// busy, as the current client does.
public partial class SoundEffects : Node
{
    private const string ClickFile = "click";
    private const float MaxVolume = 10;
    private const int PannerEffect = 0;

    private readonly Dictionary<string, AudioStreamWav?> loaded = [];
    private string soundFolder = "";

    public bool Enabled { get; set; }

    public int Volume { get; set; }

    public void Start(string dataPath, bool enabled, int volume)
    {
        soundFolder = Path.Combine(dataPath, DataLayout.Sounds);
        Enabled = enabled;
        Volume = volume;
    }

    public void PlayClickFor(byte[] packet)
    {
        if (packet.Length > 0 && ClickSound.Plays((ClientCommand)packet[0]))
        {
            Play(ClickFile, 0);
        }
    }

    public void PlayServerSound(SoundRequest request) =>
        Play(request.Number.ToString(CultureInfo.InvariantCulture), request.Pan);

    private void Play(string name, int serverPan)
    {
        if (!Enabled || Volume <= 0)
        {
            return;
        }

        var channel = GetChildren().OfType<AudioStreamPlayer>().FirstOrDefault(player => !player.Playing);
        if (channel == null || Load(name) is not { } sound)
        {
            return;
        }

        var (pan, volumeScale) = SoundPan.Panner(serverPan);
        var panner = (AudioEffectPanner)AudioServer.GetBusEffect(AudioServer.GetBusIndex(channel.Bus), PannerEffect);
        panner.Pan = pan;
        channel.VolumeDb = Mathf.LinearToDb(Volume / MaxVolume * volumeScale);
        channel.Stream = sound;
        channel.Play();
    }

    private AudioStreamWav? Load(string name)
    {
        if (loaded.TryGetValue(name, out var known))
        {
            return known;
        }

        var path = Path.Combine(soundFolder, name + ".wav");
        var sound = File.Exists(path) ? AudioStreamWav.LoadFromFile(path) : null;
        if (sound == null)
        {
            GD.PushWarning($"Sound {name} couldn't be loaded from {path}");
        }

        loaded[name] = sound;
        return sound;
    }
}
