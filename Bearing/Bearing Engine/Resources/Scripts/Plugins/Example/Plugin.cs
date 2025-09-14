using Bearing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class ExamplePlugin : Plugin
{
    public ExamplePlugin()
    {
        displayName = "Example Plugin";
        description = "A simple plugin showcasing how to make use of the plugin system.";
        author = "eyeseeyougood";
        version = "1.0.1";
        releaseDate = "2025-09-14";
        // link can be used for any relevant link (e.g: a link to the download or a donation page or even the author's socials)
        link = "https://github.com/eyeseeyougood";
    }

    [APIMethod]
    public void ExampleFunction(params string[] optional)
    {
        Logger.Log("Example function running from example plugin!");

        foreach (string parameter in optional)
        {
            Logger.Log(parameter);
        }
    }
}