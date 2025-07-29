using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Bearing;

public static class AudioManager
{
    private static WaveOutEvent output;

    private static WaveMixerStream32 mixer;

    private static List<WaveChannel32> channels = new List<WaveChannel32>();

    public static void Init()
    {
        output = new WaveOutEvent() { DesiredLatency = 50 };
        mixer = new WaveMixerStream32() { AutoStop = false };

        output.Init(mixer);
        output.Play();
    }

    public static void Tick()
    {
        int id = 0;
        int distortion = 0;
        foreach (WaveChannel32 c in channels.ToList())
        {
            if (c.Position >= c.Length)
            {
                c.Dispose();
                mixer.RemoveInputStream(c);
                channels.RemoveAt(id - distortion);
                distortion++;
            }
            id++;
        }
    }

    public static WaveChannel32 Play(Resource resource, float volume)
    {
        var reader = new BearingAudioReader(resource);

        IWaveProvider provider = reader;
        if (reader.WaveFormat.SampleRate != mixer.WaveFormat.SampleRate || // fixes formatting incase it doesnt match the mixer XD
            reader.WaveFormat.Channels != mixer.WaveFormat.Channels)
        {
            provider = new MediaFoundationResampler(reader, mixer.WaveFormat)
            {
                ResamplerQuality = 60
            };
        }

        TimeSpan duration = reader.TotalTime;

        int bps = mixer.WaveFormat.AverageBytesPerSecond;
        long resampledLength = (long)(bps * duration.TotalSeconds);

        var waveStream = new WaveProviderToWaveStream(provider, resampledLength);

        var channel = new WaveChannel32(waveStream) { Volume = volume };
        channels.Add(channel);
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

    private class WaveProviderToWaveStream : WaveStream
    {
        private readonly IWaveProvider sourceProvider;
        private long position;

        private long _length;

        public WaveProviderToWaveStream(IWaveProvider sourceProvider, long length)
        {
            this.sourceProvider = sourceProvider;
            _length = length;
        }

        public override WaveFormat WaveFormat => sourceProvider.WaveFormat;

        public override long Length => _length;

        public override long Position
        {
            get => position;
            set => position = value; // IWaveProvider cannot set position, so this is just here to prevent errors with compatibility XDDDDDD
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = sourceProvider.Read(buffer, offset, count);
            position += read;
            return read;
        }
    }
}