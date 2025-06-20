using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Bearing;

public class AudioSource : Component
{
    public string resource { get; set; }
    public bool playOnLoad { get; set; }
    public float volume { get; set; }

    public AudioSource() { resource = ""; }

    private WaveChannel32 channel;

    public override void Cleanup()
    {
        channel?.Dispose();
    }

    public override void OnLoad()
    {
        if (playOnLoad)
            channel = AudioManager.Play(resource, volume);
    }

    public override void OnTick(float dt)
    {
    }
}