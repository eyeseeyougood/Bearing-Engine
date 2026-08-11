using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenAL;

namespace Bearing;

public class AudioSource : Component
{
    private Resource? _resource = null;
    public Resource? resource
    {
        get
        {
            return _resource;
        }
        set
        {
            if (value is null)
            {
                _resource = null;
                return;
            }

            bool same = _resource?.fullpath == value.fullpath;
            _resource = value;

            if (!same)
            {
                InitSource();
            }
        }
    }
    public bool playOnLoad { get; set; } = false;
    public bool loop { get; set; } = false;

    private float _volume = 1.0f;
    public float volume
    {
        get{
            return _volume;
        }
        set{
            _volume = value;
            AudioManager.GetAL().SetSourceProperty(source, SourceFloat.Gain, _volume);
        }
    }

    private float _pitch = 1.0f;
    public float pitch
    {
        get{
            return _pitch;
        }
        set{
            _pitch = value;
            AudioManager.GetAL().SetSourceProperty(source, SourceFloat.Pitch, _pitch);
        }
    }

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
        InitSource();
        

        AudioManager.PlaySource(source);
    }

    private int prevProcessed = 0;
    public void UpdateBuffers()
    {
        if (stream == null)
            return;

        AudioManager.GetAL().GetSourceProperty(source, GetSourceInteger.BuffersProcessed, out int count);

        if (count > 0)
        {
            uint[] processed = new uint[count];
            AudioManager.GetAL().SourceUnqueueBuffers(source, processed);

            if (!stream.IsEndOfStream())
            {
                foreach (uint processedBuffer in processed)
                {
                    stream.FillBuffer(processedBuffer);
                }

                AudioManager.EnqueueBuffers(source, processed);
            }
        }
        else
        {
            if (stream.IsEndOfStream() && prevProcessed != 0)
            {
                AudioManager.StopSource(source);
                stream.Dispose();
                stream = null;
            }
        }

        prevProcessed = count;
    }

    public override void OnLoad()
    {
        InitSource();

        if (playOnLoad)
            Play();
    }

    private void InitSource()
    {
        if (resource == null)
            return;

        Cleanup();

        stream = new AudioStream();
        stream.ReadWAV(resource);

        source = AudioManager.CreateSource();
        AudioManager.GetAL().SetSourceProperty(source, SourceFloat.Pitch, pitch);
        AudioManager.GetAL().SetSourceProperty(source, SourceFloat.Gain, volume);
        AudioManager.GetAL().SetSourceProperty(source, SourceBoolean.Looping, loop);
        
        AudioManager.EnqueueBuffers(source, stream.GetBuffers());
    }

    public override void OnTick(float dt)
    {
        UpdateBuffers();
    }
}