using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenAL;

namespace Bearing;

public class AudioSource : Component
{
    public Resource resource { get; set; } = new Resource();
    public bool playOnLoad { get; set; } = false;
    public bool loop { get; set; } = false;
    public float volume { get; set; } = 1.0f;
    public float pitch { get; set; } = 1.0f;

    public AudioStream? stream;
    private uint source;

    public override void Cleanup()
    {
        if (stream != null)
            stream.Dispose();
        if (AudioManager.GetAL().IsSource(source))    
            AudioManager.GetAL().DeleteSource(source);
    }

    public void Play()
    {
        Cleanup();

        stream = new AudioStream();
        stream.ReadWAV(resource);

        source = AudioManager.CreateSource();
        AudioManager.GetAL().SetSourceProperty(source, SourceFloat.Pitch, pitch);
        AudioManager.GetAL().SetSourceProperty(source, SourceFloat.Gain, volume);
        AudioManager.GetAL().SetSourceProperty(source, SourceBoolean.Looping, loop);

        AudioManager.EnqueueBuffers(source, stream.GetBuffers());

        AudioManager.PlaySource(source);
    }

    public override void OnLoad()
    {
        if (playOnLoad)
            Play();
    }

    public override void OnTick(float dt) {}
}