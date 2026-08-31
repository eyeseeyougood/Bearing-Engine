using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public abstract class Resource : IMetadata
{
    public object[] metadata { get; set; } = new object[0];
    public string fullpath { get; set; } = "";

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

    public override string ToString()
    {
        return fullpath;
    }
}

public class EmbeddedResource : Resource
{
    public static EmbeddedResource FromPath(string path, string type = "unknown")
    {
        EmbeddedResource r = new EmbeddedResource();
        r.fullpath = path;
        r.AddMeta(type);
        return r;
    }

    public string GetShortPath()
    {
        string prefix = fullpath.StartsWith("./Resources/") ? "res/" : "eng/";
        string following = fullpath.Replace(prefix == "res/" ? "./Resources/" : "./EngineData/", "");

        return prefix + following;
    }

    private static string GetPrefix(string name)
    {
        string result = "Resources";

        if (name.StartsWith("eng/"))
        {
            result = "EngineData";
        }

        return result;
    }

    private static string ProcessResourceName(string name)
    {
        string result = name;

        if (name.StartsWith("res/"))
        {
            result = name.Substring(4);
        }

        if (name.StartsWith("eng/"))
        {
            result = name.Substring(4);
        }

        return result;
    }

    public static Resource GetModel(string name)
    {
        string pref = GetPrefix(name);
        string processedName = ProcessResourceName(name);
        return FromPath($"./{pref}/Models/" + processedName, "model");
    }

    public static Resource GetMusic(string name)
    {
        string pref = GetPrefix(name);
        string processedName = ProcessResourceName(name);
        return FromPath($"./{pref}/Audio/Music/" + processedName, "music");
    }

    public static Resource GetSFX(string name)
    {
        string pref = GetPrefix(name);
        string processedName = ProcessResourceName(name);
        return FromPath($"./{pref}/Audio/SFX/" + processedName, "sfx");
    }

    public static Resource GetAudio(string name)
    {
        string pref = GetPrefix(name);
        string processedName = ProcessResourceName(name);
        return FromPath($"./{pref}/Audio/" + processedName, "audio");
    }

    public static Resource GetShader(string name)
    {
        string pref = GetPrefix(name);
        string processedName = ProcessResourceName(name);
        return FromPath($"./{pref}/Shaders/" + processedName, "shader");
    }

    public static Resource GetTexture(string name)
    {
        string pref = GetPrefix(name);
        string processedName = ProcessResourceName(name);
        return FromPath($"./{pref}/Textures/" + processedName, "texture");
    }
}

public class ExternalResource : Resource
{
    public static ExternalResource FromPath(string path, string type = "unknown")
    {
        ExternalResource r = new ExternalResource();
        r.fullpath = path;
        r.AddMeta(type);
        return r;
    }
}

public static class Resources
{
    public static Stream? Open(Resource resource)
    {
        if (resource is EmbeddedResource)
        {
            string? processedName = resource.fullpath[0] == '.' ? new string(resource.fullpath.Skip(1).ToArray()) : resource.fullpath;
            processedName = processedName.Replace("/", ".");
            processedName = "Bearing." + (processedName[0] == '.' ? new string(processedName.Skip(1).ToArray()) : processedName);

            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream? stream = assembly.GetManifestResourceStream(processedName);

            return stream;
        }

        // external resource
        return File.Open(resource.fullpath, FileMode.Open);
    }

    public static string ReadAllText(Resource resource)
    {
        var stream = Open(resource);

        if (stream is null)
            throw new Exception("Attempt to call ReadAllText() on an invalid resource! Resource: " + resource);

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

        if (stream is null)
            throw new Exception("Attempt to call ReadAllBytes() on an invalid resource! Resource: " + resource);

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

    public static string[] GetEmbeddedFiles(string path)
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

    public static string[] GetEmbeddedFiles(string path, string ext)
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