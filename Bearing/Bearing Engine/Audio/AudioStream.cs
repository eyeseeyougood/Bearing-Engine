using Silk.NET.OpenAL;

namespace Bearing;

public unsafe class AudioStream
{
	private uint[] buffers = new uint[0];
	private BufferFormat bufferFormat;
	private uint sampleRate;
	private long dataPos = -1;

	private Stream? s;
	private bool endOfStream = false;

	public uint[] GetBuffers()
	{
		return buffers;
	}

	public void CreateBuffers()
	{
		buffers = new uint[] {
			AudioManager.CreateBuffer(),
			AudioManager.CreateBuffer(),
			AudioManager.CreateBuffer(),
			AudioManager.CreateBuffer(),
			AudioManager.CreateBuffer(),
			AudioManager.CreateBuffer(),
			AudioManager.CreateBuffer()
		};

		// buffer new data

		foreach (uint buffer in buffers)
		{
			FillBuffer(buffer);
		}
	}

	public void ResetBuffers()
	{
		foreach (uint buffer in buffers)
		{
			FillBuffer(buffer);
		}
	}

	public void ResetPosition()
	{
		s.Position = dataPos;
		endOfStream = false;
	}

	public bool IsEndOfStream()
	{
		return endOfStream;
	}

	public void FillBuffer(uint buffer)
	{
		byte[] temp = new byte[4096];
		int? bytesRead = s?.Read(temp, 0, 4096);
		fixed (byte* ptr = temp)
		{
			if (bytesRead.HasValue)
			{
				int writeCount = bytesRead.Value;
				AudioManager.BufferData(buffer, bufferFormat, ptr, writeCount, (int)sampleRate);
			}
		}
		if (bytesRead == 0)
			endOfStream = true;
	}

	public void ReadWAV(Resource resource)
	{
		string fType = resource.GetFileType();
		if (fType != "wav")
		{
			Logger.LogError($"Failed to load audio file of type '{fType}' due to it not being supported.");
			return;
		}

		s = Resources.Open(resource);
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
		sampleRate = 0;
		ushort bps = 0;
		
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
		    	dataPos = s.Position;
		        break;
		    }
		    else
		    {
		        s.Seek(chunkSize, SeekOrigin.Current);
		    }
		}

		bufferFormat = BufferFormat.Stereo16;
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
		CreateBuffers();
	}

	public void Dispose()
	{
		foreach (uint buffer in buffers)
		{
			AudioManager.GetAL().DeleteBuffer(buffer);
		}
		s.Dispose();
	}
}