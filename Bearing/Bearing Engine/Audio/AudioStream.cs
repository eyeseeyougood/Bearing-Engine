using Silk.NET.OpenAL;

namespace Bearing;

public unsafe class AudioStream
{
	private uint buffer;

	public uint[] GetBuffers()
	{
		return new uint[] { buffer };
	}

	public void ReadWAV(Resource resource)
	{
		string fType = resource.GetFileType();
		if (fType != "wav")
		{
			Logger.LogError($"Failed to load audio file of type '{fType}' due to it not being supported.");
			return;
		}

		Stream? s = Resources.Open(resource);
		if (s == null)
		{
			Logger.LogError($"Failed to open file stream on resource: '{fType}', check that it is present in the resources folder.");
			return;
		}

		byte[] identifier = new byte[4];
		s.ReadExactly(identifier, 0, 4);

		byte[] fileSize = new byte[4];
		s.ReadExactly(fileSize, 0, 4);

		if (System.Text.Encoding.ASCII.GetString(identifier) != "RIFF")
		{
			Logger.LogError($"Could not load the resource '{resource.GetName()}' as it uses an invalid and could potentially be corrupted.");
			return;
		}

		s.Seek(4, SeekOrigin.Current);

		ushort audioFormat = 0;
		ushort channels = 0;
		uint sampleRate = 0;
		ushort bps = 0;
		byte[]? pcmData = null;
		
		while (s.Position < s.Length)
		{
			byte[] chunkIdBytes = new byte[4];
		    s.ReadExactly(chunkIdBytes, 0, 4);
		    string chunkId = System.Text.Encoding.ASCII.GetString(chunkIdBytes);

		    byte[] chunkSizeBytes = new byte[4];
		    s.ReadExactly(chunkSizeBytes, 0, 4);
		    uint chunkSize = BitConverter.ToUInt32(chunkSizeBytes, 0);

		    if (chunkId == "fmt ")
		    {
		        byte[] fmtData = new byte[chunkSize];
		        s.ReadExactly(fmtData, 0, (int)chunkSize);

		        audioFormat = BitConverter.ToUInt16(fmtData, 0);
		        channels = BitConverter.ToUInt16(fmtData, 2);
		        sampleRate = BitConverter.ToUInt32(fmtData, 4);
		        bps = BitConverter.ToUInt16(fmtData, 14);
		    }
		    else if (chunkId == "data")
		    {
		        pcmData = new byte[chunkSize];
		        s.Read(pcmData, 0, (int)chunkSize);
		        break;
		    }
		    else
		    {
		        s.Seek(chunkSize, SeekOrigin.Current);
		    }
		}

		BufferFormat bufferFormat = BufferFormat.Stereo16;
		bool invalidBufferFormat = false;
		if (channels == 2)
		{
			if (bps == 8)
			{
				bufferFormat = BufferFormat.Stereo8;
			}
			else if (bps != 16)
			{
				invalidBufferFormat = true;
			}
		}
		else if (channels == 1)
		{
			if (bps == 16)
			{
				bufferFormat = BufferFormat.Mono16;
			}
			else if (bps == 8)
			{
				bufferFormat = BufferFormat.Mono8;
			}
			else
			{
				invalidBufferFormat = true;
			}
		}
		else
		{
			invalidBufferFormat = true;
		}

		if (invalidBufferFormat || audioFormat != 1)
		{
			Logger.LogError($"Could not load the resource '{resource.GetName()}' as it uses an unsupported format: bf={bufferFormat} & af={audioFormat}");
			return;
		}


		// buffer data
		buffer = AudioManager.CreateBuffer();

		fixed (byte* ptr = pcmData)
		{
		    AudioManager.BufferData(buffer, bufferFormat, ptr, pcmData.Length, (int)sampleRate);
		}

		s.Dispose();
	}

	public void Dispose()
	{
		AudioManager.GetAL().DeleteBuffer(buffer);
	}
}