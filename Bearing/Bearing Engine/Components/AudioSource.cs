using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Bearing;

public class AudioSource : Component
{
    public Resource resource { get; set; } = new Resource();
    public bool playOnLoad { get; set; }
    public float volume { get; set; }

    private List<WaveChannel32> channels = new List<WaveChannel32>();

    public override void Cleanup()
    {
        int c = channels.Count;
        for (int i = 0; i < c; i++)
        {
            channels[0]?.Dispose();
            channels.RemoveAt(0);
        }
    }

    public void ForcePlay()
    {
        //channels.Add(AudioManager.Play(resource, volume));
    }

    public override void OnLoad()
    {
        if (playOnLoad)
            ForcePlay();
    }

    public override void OnTick(float dt)
    {
    }
}