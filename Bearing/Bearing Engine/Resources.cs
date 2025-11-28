using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class Resource : IMetadata
{
    public object[] metadata { get; set; } = new object[1];
    public string fullpath { get; set; }

    public static Resource FromPath(string path, string type = "unknown")
    {
        Resource r = new Resource();
        r.fullpath = path;
        r.metadata[0] = type;
        return r;
    }

    public string GetFileType()
    {
        return fullpath.Split('.').Last().ToLower();
    }

    public string GetName(bool includeExt = true)
    {
        string result = fullpath.Split('/').Last();

        if (!includeExt)
            result = result.Split('.').First();

        return result;
    }

    public static Resource GetModel(string name, bool isEngineData = false)
    {
        string pref = isEngineData ? "EngineData" : "Resources";
        return FromPath($"./{pref}/Model/" + name, "model");
    }

    public static Resource GetMusic(string name, bool isEngineData = false)
    {
        string pref = isEngineData ? "EngineData" : "Resources";
        return FromPath($"./{pref}/Audio/Music/" + name, "music");
    }

    public static Resource GetSFX(string name, bool isEngineData = false)
    {
        string pref = isEngineData ? "EngineData" : "Resources";
        return FromPath($"./{pref}/Audio/SFX/" + name, "sfx");
    }

    public static Resource GetAudio(string name, bool isEngineData = false)
    {
        string pref = isEngineData ? "EngineData" : "Resources";
        return FromPath($"./{pref}/Audio/" + name, "audio");
    }

    public static Resource GetShader(string name, bool isEngineData = false)
    {
        string pref = isEngineData ? "EngineData" : "Resources";
        return FromPath($"./{pref}/Shaders/" + name, "shader");
    }

    public static Resource GetTexture(string name, bool isEngineData = false)
    {
        string pref = isEngineData ? "EngineData" : "Resources";
        return FromPath($"./{pref}/Textures/" + name, "texture");
    }
}

public static class Resources
{
    public static Stream? Open(Resource resource)
    {
        string? processedName = resource.fullpath[0] == '.' ? new string(resource.fullpath.Skip(1).ToArray()) : resource.fullpath;
        processedName = processedName.Replace("/", ".");
        processedName = "Bearing." + (processedName[0] == '.' ? new string(processedName.Skip(1).ToArray()) : processedName);

        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(processedName);

        return stream;
    }

    public static string ReadAllText(Resource resource)
    {
        var stream = Open(resource);

        byte[] buffer = new byte[4096];
        int read;
        var sb = new StringBuilder();

        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            sb.Append(Encoding.UTF8.GetString(buffer), 0, read);
        }

        string result = sb.ToString();

        stream.DisposeAsync();

        return result;
    }

    public static byte[] ReadAllBytes(Resource resource)
    {
        var stream = Open(resource);

        byte[] buffer = new byte[4096];
        int read;
        List<byte> result = new List<byte>();

        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            result.AddRange(buffer);
        }

        stream.DisposeAsync();

        return result.ToArray();
    }

    public static string[] GetFiles(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string[] all = assembly.GetManifestResourceNames();

        List<string> result = new List<string>();

        foreach (string res in all)
        {
            if (res.StartsWith(path))
            {
                result.Add(res);
            }
        }

        return result.ToArray();
    }

    public static string[] GetFiles(string path, string ext)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string[] all = assembly.GetManifestResourceNames();

        List<string> result = new List<string>();

        foreach (string res in all)
        {
            string nRes = res.Replace(".","/").Replace("Bearing", ".");
            string nExt = nRes.Split('/',StringSplitOptions.RemoveEmptyEntries).Last();
            nRes = string.Join("/", nRes.Split('/').SkipLast(1))+"."+nExt;
            if (nRes.StartsWith(path) && nExt == ext.Replace(".","").ToLower())
            {
                result.Add(nRes);
            }
        }

        return result.ToArray();
    }
}