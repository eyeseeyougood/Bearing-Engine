using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

/// <summary>
/// Bearing Engine's version of the AudioFileReader class from NAudio designed to work with resources instead of files
/// </summary>
public class BearingAudioReader : WaveStream, ISampleProvider
{
    private WaveStream readerStream;

    private readonly SampleChannel sampleChannel;

    private readonly int destBytesPerSample;

    private readonly int sourceBytesPerSample;

    private readonly long length;

    private readonly object lockObject;

    public string FileName { get; }

    public override WaveFormat WaveFormat => sampleChannel.WaveFormat;

    public override long Length => length;

    public override long Position
    {
        get
        {
            return SourceToDest(readerStream.Position);
        }
        set
        {
            lock (lockObject)
            {
                readerStream.Position = DestToSource(value);
            }
        }
    }

    public float Volume
    {
        get
        {
            return sampleChannel.Volume;
        }
        set
        {
            sampleChannel.Volume = value;
        }
    }

    /// <summary>
    /// Bearing Engine's version of the AudioFileReader class from NAudio designed to work with resources instead of files
    /// </summary>
    public BearingAudioReader(Resource resource)
    {
        lockObject = new object();
        FileName = resource.fullpath;
        CreateReaderStream(resource);
        sourceBytesPerSample = readerStream.WaveFormat.BitsPerSample / 8 * readerStream.WaveFormat.Channels;
        sampleChannel = new SampleChannel(readerStream, forceStereo: false);
        destBytesPerSample = 4 * sampleChannel.WaveFormat.Channels;
        length = SourceToDest(readerStream.Length);
    }

    private void CreateReaderStream(Resource resource)
    {
        if (resource.fullpath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            readerStream = new WaveFileReader(Resources.Open(resource));
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }
        }
        else if (resource.fullpath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                readerStream = new Mp3FileReader(Resources.Open(resource));
            }
            else
            {
                readerStream = new MediaFoundationReader(resource.fullpath);
            }
        }
        else if (resource.fullpath.EndsWith(".aiff", StringComparison.OrdinalIgnoreCase) || resource.fullpath.EndsWith(".aif", StringComparison.OrdinalIgnoreCase))
        {
            readerStream = new AiffFileReader(Resources.Open(resource));
        }
        else
        {
            readerStream = new MediaFoundationReader(resource.fullpath);
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        WaveBuffer waveBuffer = new WaveBuffer(buffer);
        int count2 = count / 4;
        return Read(waveBuffer.FloatBuffer, offset / 4, count2) * 4;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        lock (lockObject)
        {
            return sampleChannel.Read(buffer, offset, count);
        }
    }

    private long SourceToDest(long sourceBytes)
    {
        return destBytesPerSample * (sourceBytes / sourceBytesPerSample);
    }

    private long DestToSource(long destBytes)
    {
        return sourceBytesPerSample * (destBytes / destBytesPerSample);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && readerStream != null)
        {
            readerStream.Dispose();
            readerStream = null;
        }

        base.Dispose(disposing);
    }
}