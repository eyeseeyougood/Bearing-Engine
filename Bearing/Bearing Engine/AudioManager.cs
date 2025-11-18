using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.Enumeration;

namespace Bearing;

public static class AudioManager
{
    private static ALContext? alc;
    private static AL? al;
    private static unsafe Device* device;
    private static unsafe Context* alContext;

    public static unsafe void Init()
    {
        alc = ALContext.GetApi(true);
        al = AL.GetApi(true);

        string deviceSpecifier = "";
        if (alc.IsExtensionPresent(null, "ALC_ENUMERATION_EXT"))
        {
            alc.TryGetExtension<Enumeration>(null, out Enumeration enumeration);
            foreach (var d in enumeration.GetStringList(GetEnumerationContextStringList.DeviceSpecifiers))
            {
                deviceSpecifier = d;
            }
        }

        device = alc.OpenDevice(deviceSpecifier);
        Context* ctx = alc.CreateContext(device, null);
        alc.MakeContextCurrent(ctx);
        Console.WriteLine("AudioManager initialised");
    }

    public static unsafe void Play(Resource resource, float volume = 1f)
    {
        if (alc == null || al == null)
            throw new Exception("Audio not initialized. Call Audio.Init() first.");

        byte[] wavData = Resources.ReadAllBytes(resource);
        int channels, sampleRate;
        byte[] pcm = LoadWave(wavData, out channels, out sampleRate, out BufferFormat format);

        uint buffer = al.GenBuffer();
        fixed (byte* pcmptr = pcm)
        {
            al.BufferData(buffer, format, pcmptr, pcm.Length, sampleRate);
        }

        uint source = al.GenSource();
        al.SetSourceProperty(source, SourceFloat.Gain, volume);
        al.SetSourceProperty(source, SourceInteger.Buffer, (int)buffer);

        al.SourcePlay(source);

        int state;
        do
        {
            al.GetSourceProperty(source, GetSourceInteger.SourceState, out state);
        } while ((SourceState)state == SourceState.Playing);

        al.DeleteSource(source);
        al.DeleteBuffer(buffer);
    }

    ///<summary>
    ///Loads a Wav File directly into a byte array. WARNING!!! This is a bad method of doing this, large files will likely cause the program to crash due to running out of memory.
    ///</summary>
    private static byte[] LoadWave(byte[] file, out int channels, out int sampleRate, out BufferFormat format)
    {
        using var mem = new MemoryStream(file);
        using var br = new BinaryReader(mem);

        br.ReadBytes(22);
        channels = br.ReadInt16();
        sampleRate = br.ReadInt32();
        br.ReadBytes(6);
        int bitsPerSample = br.ReadInt16();

        while (br.ReadInt32() != 0x61746164)
            br.ReadInt32();

        int dataSize = br.ReadInt32();
        byte[] data = br.ReadBytes(dataSize);

        if (channels == 1 && bitsPerSample == 8) format = BufferFormat.Mono8;
        else if (channels == 1 && bitsPerSample == 16) format = BufferFormat.Mono16;
        else if (channels == 2 && bitsPerSample == 8) format = BufferFormat.Stereo8;
        else format = BufferFormat.Stereo16;

        return data;
    }

    public static unsafe void Cleanup()
    {
        if (alc != null)
        {
            alc.DestroyContext(alContext);
            alc.CloseDevice(device);
        }
    }
}