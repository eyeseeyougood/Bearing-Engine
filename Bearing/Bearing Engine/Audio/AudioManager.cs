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

// TODO: FIX - AudioManager currently caches audio and files are read in entirety ... So this doesnt work for large audio files and is really inefficient
// Should use streaming instead
public static class AudioManager
{
    private static ALContext? alc;
    private static AL? al;
    private static unsafe Device* device;
    private static unsafe Context* alContext;

    public static bool cacheAudio = true;
    private static Dictionary<Resource, byte[]> wavCache = new Dictionary<Resource, byte[]>();

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

    public static uint CreateBuffer()
    {
        uint handle = al.GenBuffer();
        return handle;
    }

    public static unsafe void BufferData(uint handle, BufferFormat format, void* data, int size, int sampleRate)
    {
        al.BufferData(handle, format, data, size, sampleRate);
    }

    public static uint CreateSource()
    {
        uint handle = al.GenSource();
        return handle;
    }

    public static AL? GetAL()
    {
        return al;
    }

    public static void EnqueueBuffers(uint source, uint[] buffers)
    {
        al.SourceQueueBuffers(source, buffers);
    }

    public static void PlaySource(uint source)
    {
        al.SourcePlay(source);
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