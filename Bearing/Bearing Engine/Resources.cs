using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class Resource
{
    public string fullpath { get; set; }

    public static Resource FromPath(string path)
    {
        Resource r = new Resource();
        r.fullpath = path;
        return r;
    }

    public static Resource GetModel(string name)
    {
        return FromPath("./Resources/Models/"+name);
    }

    public static Resource GetAudio(string name)
    {
        return FromPath("./Resources/Audio/" + name);
    }

    public static Resource GetShader(string name)
    {
        return FromPath("./Resources/Shaders/" + name);
    }

    public static Resource GetEngineShader(string name)
    {
        return FromPath("./EngineData/Shaders/" + name);
    }

    public static Resource GetTexture(string name)
    {
        return FromPath("./Resources/Textures/" + name);
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
}