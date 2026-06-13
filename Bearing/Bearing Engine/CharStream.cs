namespace Bearing;

public class CharStream
{
    private string chars = "";
    private int currentIndex = 0;

    public CharStream(string data)
    {
        chars = data;
    }

    public char Peek(bool doIgnore = false, char ignore = ' ')
    {
        if (doIgnore)
            while (Peek() == ignore)
                Consume();

        char result = chars[^1];

        if (currentIndex < chars.Length)
            result = chars[currentIndex];

        return result;
    }

    public char Consume()
    {
        char result = Peek();

        if (currentIndex >= chars.Length)
        {
            throw new Exception("Attempt to consume nothing :( RAN OUT OF CHARACTERS!!!");
        }

        currentIndex++;

        return result;
    }

    public char Expect(char c, bool doIgnore = false, char ignore = ' ')
    {
        if (doIgnore)
            while (Peek() == ignore)
                Consume();

        char @char = Peek();

        if (@char != c)
            throw new Exception($"! Something went wrong at index: {currentIndex} in CharStream ! Expected '{c}' but got '{@char}'");

        return Consume();
    }
}