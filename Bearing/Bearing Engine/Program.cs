using Bearing;

namespace Bearing;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting Bearing Engine");

        using (Game game = new Game(800, 600, "Bearing Engine"))
        {
            game.Run();
        }
    }
}