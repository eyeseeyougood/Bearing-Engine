using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Bearing;

public static class AudioManager
{
    private static WaveOutEvent output;

    private static WaveMixerStream32 mixer;

    public static void Init()
    {
        output = new WaveOutEvent();
        mixer = new WaveMixerStream32() { AutoStop = false };

        output.Init(mixer);
        output.Play();
    }

    public static WaveChannel32 Play(string resource, float volume)
    {
        AudioFileReader reader = new AudioFileReader(resource);
        WaveChannel32 channel = new WaveChannel32(reader, volume, 0);
        mixer.AddInputStream(channel);
        return channel;
    }

    public static void Stop()
    {
        output.Stop();
    }

    public static void Cleanup()
    {
        output?.Dispose();
    }

    public static bool IsPlaying()
    {
        return output.PlaybackState == PlaybackState.Playing;
    }
}
