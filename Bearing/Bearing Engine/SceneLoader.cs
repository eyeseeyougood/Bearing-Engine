using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bearing;

public static class SceneLoader
{
    #region Old File Type Parsing

    private struct PresetRef
    {
        public List<PresetLineRef> lineRefs = new List<PresetLineRef>();
        public string presetName = "";
        public int startIndex = 0;
        public int length = 0;

        public PresetRef() { }
    }

    private struct PresetLineRef
    {
        public string replacement = "";
        public string newValue = "";

        public PresetLineRef() { }
    }

    public static void Tick() { }

    [Obsolete]
    public static GameObject LegacyLoadFromFile(string filepath, bool initialise = true)
    {
        string data = Resources.ReadAllText(Resource.FromPath(filepath));

        while (data.Contains("#PRESET("))
            data = LegacyPreprocess(data); // stuff like presets

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { new TransformConverter(), new ComponentConverter(), new ColliderConverter(), new RBConverter() }
        };

        GameObject root = JsonConvert.DeserializeObject<GameObject>(data, settings);

        if (initialise)
            root.Load();

        return root;
    }

    [Obsolete]
    public static GameObject LegacyLoadFromRealFile(string filepath, bool initialise = true)
    {
        string data = File.ReadAllText(filepath);

        while (data.Contains("#PRESET("))
            data = LegacyPreprocess(data); // stuff like presets

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { new TransformConverter(), new ComponentConverter(), new ColliderConverter(), new RBConverter() }
        };

        GameObject root = JsonConvert.DeserializeObject<GameObject>(data, settings);

        if (initialise)
            root.Load();

        return root;
    }

    [Obsolete]
    /// <summary>
    /// If you find urself looking through the implementation of this function, gl.
    /// </summary>
    /// <param name="data">The json data to preprocess to parse the preset syntax</param>
    /// <returns></returns>
    private static string LegacyPreprocess(string data)
    {
        /*
                #PRESET(button)
                [
                    hello!!!, = This button was made with a preset!|
                ],*/ // --- important note about the syntax here, it doesnt require quotes around the thing ur changing
                     // --- but it always needs the comma as that marks the end of the thing ur changing and marks start of replacement
        bool onPreset = false;
        bool onName = false;
        bool onReplacement = false;
        bool onNewValue = false;
        bool gotName = false;
        bool waitingOnValue = false;

        char prevChar = ' ';

        List<PresetRef> presets = new List<PresetRef>();

        StringBuilder presetName = new StringBuilder();
        StringBuilder replacement = new StringBuilder();
        StringBuilder newValue = new StringBuilder();

        string cleaned = string.Join("",
    data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
        .Select(line => line.TrimStart()));

        // parsing
        onPreset = false;
        prevChar = ' ';
        foreach (char c in cleaned)
        {
            if (c == '#')
            {
                onPreset = true;

                presets.Add(new PresetRef()); 

                prevChar = c;
                continue;
            }

            if (!onPreset)
            {
                prevChar = c;
                continue;
            }

            if (c == '(' && !gotName)
            {
                onName = true;

                presetName.Clear();

                prevChar = c;
                continue;
            }

            if (c == ')' && !gotName)
            {
                onName = false;
                gotName = true;

                PresetRef pres = presets[presets.Count - 1];
                pres.presetName = presetName.ToString();
                presets[presets.Count - 1] = pres;

                prevChar = c;
                continue;
            }

            if (c == '|')
            {
                onNewValue = false;

                PresetRef pres = presets[presets.Count - 1];
                PresetLineRef lref = pres.lineRefs[pres.lineRefs.Count - 1];
                lref.newValue = newValue.ToString();
                pres.lineRefs[pres.lineRefs.Count-1] = lref;
                presets[presets.Count - 1] = pres;

                prevChar = c;
                continue;
            }

            if (onName)
            {
                presetName.Append(c);
                prevChar = c;
                continue;
            }

            if (c == ']' && prevChar == '[')
            {
                onNewValue = false;
                onPreset = false;

                PresetRef pres = presets[presets.Count - 1];
                pres.lineRefs.Clear();
                presets[presets.Count - 1] = pres;

                onName = false;
                waitingOnValue = false;
                onReplacement = false;
                gotName = false;

                prevChar = c;
                continue;
            }

            if (onReplacement)
            {
                if (c == ',')
                {
                    onReplacement = false;
                    waitingOnValue = true;
                    prevChar = c;

                    PresetRef pres = presets[presets.Count - 1];
                    PresetLineRef lref = pres.lineRefs[pres.lineRefs.Count - 1];
                    lref.replacement = replacement.ToString();
                    pres.lineRefs[pres.lineRefs.Count - 1] = lref;
                    presets[presets.Count - 1] = pres;

                    newValue.Clear();
                    continue;
                }

                replacement.Append(c);
                prevChar = c;
                continue;
            }

            if (waitingOnValue)
            {
                if (c != ' ' && c != '=')
                {
                    waitingOnValue = false;
                    onNewValue = true;
                    newValue.Append(c);
                    prevChar = c;
                    continue;
                }
            }

            if (c == ']' && prevChar == '|')
            {
                onNewValue = false;

                PresetRef pres = presets[presets.Count - 1];
                PresetLineRef lref = pres.lineRefs[pres.lineRefs.Count - 1];
                lref.newValue = newValue.ToString();
                pres.lineRefs[pres.lineRefs.Count - 1] = lref;
                presets[presets.Count-1] = pres;

                onName = false;
                waitingOnValue = false;
                onReplacement = false;
                gotName = false;

                onPreset = false;
                prevChar = c;
                continue;
            }

            if (prevChar == '|')
            {
                // if this triggers, because above it c == ']' exists
                // we know that c != ] and so we dont need to put that in the condition XD
                // jank ik ^^^ XDD

                onReplacement = true;

                PresetRef pres = presets[presets.Count - 1];
                pres.lineRefs.Add(new PresetLineRef());
                presets[presets.Count - 1] = pres;

                prevChar = c;

                replacement.Clear();
                replacement.Append(c);
                continue;
            }

            if (onNewValue)
            {
                newValue.Append(c);
                prevChar = c;
                continue;
            }

            if (c == '[')
            {
                onReplacement = true;

                PresetRef pres = presets[presets.Count - 1];
                pres.lineRefs.Add(new PresetLineRef());
                presets[presets.Count - 1] = pres;

                prevChar = c;

                replacement.Clear();
                continue;
            }
        }

        // first find start and length of preset snippet
        int pid = -1;
        int index = 0;
        foreach (char c in data)
        {
            if (c == '#')
            {
                pid++;
                PresetRef pres = presets[pid];
                pres.startIndex = index;
                presets[pid] = pres;
                onPreset = true;
            }

            if (c == ']' && (prevChar == '|' || prevChar == '[') && onPreset)
            {
                PresetRef pres = presets[pid];
                pres.length = index - pres.startIndex + 1; // +1 accounts for the last bracket
                presets[pid] = pres;
                onPreset = false;
            }

            if (c != ' ' && c != '\n' && c != '\r' && c != '\t')
                prevChar = c;

            index++;
        }

        // replacing text with parsed data
        string replaced = data;
        int distortion = 0;
        foreach (PresetRef p in presets)
        {
            string presetData = Resources.ReadAllText(Resource.FromPath($"./Resources/Scene/{p.presetName}.preset"));

            string cleanedPreset = string.Join("",
        presetData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Select(line => line.TrimStart()));

            cleanedPreset = string.Join("",cleanedPreset.Skip(1).SkipLast(1));

            foreach (PresetLineRef lr in p.lineRefs)
            {
                cleanedPreset = cleanedPreset.Replace(lr.replacement, lr.newValue);
            }

            replaced = replaced.Remove(p.startIndex+distortion, p.length);
            replaced = replaced.Insert(p.startIndex+distortion, cleanedPreset);

            distortion += cleanedPreset.Length - p.length;
        }

        return replaced.Trim('\uFEFF');
    }

    #endregion

    private enum TokenType { EOS, Identifier, Object, LBracket, RBracket, LCurly, RCurly, LSquare, RSquare };
    
    private class Token()
    {
        public static readonly Token EOS = new Token() { type = TokenType.EOS };

        public TokenType type;

        public object value = "";
    }

    private static string Preprocess(string data)
    {
        StringBuilder result = new StringBuilder();

        StringBuilder sb = new StringBuilder();

        for (int loc = 0; loc < data.Length; loc++)
        {
            if (data[loc] == '#')
            {
                loc++;
                do
                {
                    sb.Append(data[loc]);
                    loc++;
                }
                while (data[loc] != '#');

                result.Insert(result.Length, Preprocess(Resources.ReadAllText(Resource.FromPath($"./Resources/Scene/{sb.ToString()}")).Replace("\n","").Replace("\t","")));
                int startLocation = loc;
                sb.Clear();
            }
            if (data[loc] != '#')
                result.Append(data[loc]);
        }

        return result.ToString();
    }

    private static Token TokeniseIdentifier(CharStream cs, string endChars, bool allowEmpty = false)
    {
        StringBuilder sb = new StringBuilder();

        while (!endChars.Contains(cs.Peek()))
        {
            sb.Append(cs.Consume());
        }

        if (sb.Length == 0 && !allowEmpty)
            throw new Exception("Failed to tokenise identifier! Early exit.");

        return new Token() { type = TokenType.Identifier, value = sb.ToString().TrimStart().TrimEnd() };
    }

    private static Token TokeniseString(CharStream cs)
    {
        StringBuilder sb = new StringBuilder();

        cs.Expect('"', doIgnore: true);

        while (cs.Peek() != '"')
        {
            sb.Append(cs.Consume());
        }

        cs.Consume(); // go past the "

        return new Token() { type = TokenType.Object, value = sb.ToString() };
    }

    private static Token TokeniseNumber(CharStream cs)
    {
        StringBuilder sb = new StringBuilder();

        bool isFloat = false;

        while (char.IsDigit(cs.Peek()) || cs.Peek() == '-' || cs.Peek() == '.')
        {
            if (cs.Peek() == '.')
            {
                isFloat = true;
            }
            
            sb.Append(cs.Consume());
        }

        object newValue = 0;
        
        if (isFloat) { newValue = float.Parse(sb.ToString()); }
        else { newValue = int.Parse(sb.ToString()); }

        return new Token() { type = TokenType.Object, value = newValue };
    }

    private static List<Token> TokeniseList(CharStream cs)
    {
        List<Token> result = new List<Token>();
        cs.Expect('[', true);
        result.Add(new Token() { type = TokenType.LSquare });

        while (cs.Peek(true) != ']')
        {
            result.AddRange(TokeniseValue(cs));

            if (cs.Peek(true) == ',')
                cs.Consume();
        }

        cs.Expect(']', true);
        result.Add(new Token() { type = TokenType.RSquare });

        return result;
    }

    private static List<Token> TokeniseValue(CharStream cs)
    {
        List<Token> result = new List<Token>();

        switch (cs.Peek(true))
        {
            case '"':
                result.Add(TokeniseString(cs));
                break;
            case '(':
                result.AddRange(TokeniseObject(cs));
                break;
            case '[':
                result.AddRange(TokeniseList(cs));
                break;
            case 't':
                cs.Expect('t', true);
                cs.Expect('r');
                cs.Expect('u');
                cs.Expect('e');
                result.Add(new Token(){ type = TokenType.Object, value = true });
                break;
            case 'T':
                cs.Expect('T', true);
                cs.Expect('R');
                cs.Expect('U');
                cs.Expect('E');
                result.Add(new Token(){ type = TokenType.Object, value = true });
                break;
            case 'f':
                cs.Expect('f', true);
                cs.Expect('a');
                cs.Expect('l');
                cs.Expect('s');
                cs.Expect('e');
                result.Add(new Token(){ type = TokenType.Object, value = false });
                break;
            case 'F':
                cs.Expect('F', true);
                cs.Expect('A');
                cs.Expect('L');
                cs.Expect('S');
                cs.Expect('E');
                result.Add(new Token(){ type = TokenType.Object, value = false });
                break;
            default:
                if (char.IsDigit(cs.Peek(true)) || cs.Peek(true) == '-')
                {
                    result.Add(TokeniseNumber(cs));
                }
                break;
        }

        return result;
    }

    private static List<Token> TokeniseParemeter(CharStream cs)
    {
        List<Token> result = new List<Token>();

        result.Add(TokeniseIdentifier(cs, ":", false));


        cs.Consume(); // get past the :

        result.AddRange(TokeniseValue(cs));

        return result;
    }

    private static List<Token> TokeniseObject(CharStream cs)
    {
        List<Token> result = new List<Token>();

        cs.Expect('(', doIgnore: true);
        result.Add(new Token() { type = TokenType.LBracket });

        result.Add(TokeniseIdentifier(cs, ":)"));

        char c = cs.Consume();

        if (c == ':')
        {
            while (cs.Peek() != ')')
            {
                if (cs.Peek() == ',')
                {
                    cs.Consume();
                    continue;
                }

                result.AddRange(TokeniseValue(cs));
            }

            cs.Consume(); // get passed the )
        }
        
        result.Add(new Token() { type = TokenType.RBracket });

        cs.Expect('{', doIgnore: true);
        result.Add(new Token() { type = TokenType.LCurly });

        while (cs.Peek(true) != '}')
        {
            result.AddRange(TokeniseParemeter(cs));

            if (cs.Peek() == ',')
            {
                cs.Consume();
            }
        }

        cs.Expect('}', doIgnore: true);
        result.Add(new Token() { type = TokenType.RCurly });

        return result;
    }

    private static List<Token> Tokenise(string data)
    {
        List<Token> result = new List<Token>();

        StringBuilder sb = new StringBuilder();

        CharStream cs = new CharStream(data);

        result = TokeniseObject(cs);

        return result;
    }

    private class TokenStream()
    {
        private List<Token> tokens = new List<Token>();
        private int currentIndex;

        private static void LogTokens(List<Token> tokens)
        {
            int c = 0;
            foreach (Token t in tokens)
            {
                c++;
            }
        }

        public static TokenStream FromResource(Resource resource)
        {
            TokenStream ts = new TokenStream();

            ts.tokens = Tokenise(Preprocess(Resources.ReadAllText(resource).Replace("\n","").Replace(""+(char)13, "").Replace("\t","")));

            LogTokens(ts.tokens);

            return ts;
        }

        public Token Peek()
        {
            Token result = tokens[^1];

            if (currentIndex < tokens.Count)
                result = tokens[currentIndex];

            return result;
        }

        public Token Consume()
        {
            Token result = Peek();

            if (currentIndex >= tokens.Count)
            {
                result = Token.EOS;
            }

            currentIndex++;

            return result;
        }

        public Token Expect(TokenType type)
        {
            Token token = Peek();

            if (token.type != type)
                throw new Exception($"! Failed to parse scene tree at token {currentIndex} ! Expected '{type}' but got '{token.type}'");

            return Consume();
        }
    }

    private static List<object> ParseList(TokenStream ts)
    {
        List<object> result = new List<object>();

        ts.Expect(TokenType.LSquare);

        while (ts.Peek().type != TokenType.RSquare)
        {
            result.Add(ParseValue(ts));
        }

        ts.Consume();

        return result;
    }

    private static object ParseValue(TokenStream ts)
    {
        switch (ts.Peek().type)
        {
            case TokenType.LBracket:
                return ParseObject(ts);
            case TokenType.LSquare:
                return ParseList(ts);
            default:
                return ts.Consume().value;
        }
    }

    private static object ParseObject(TokenStream ts)
    {
        ts.Expect(TokenType.LBracket);

        string dataType = (string)ts.Expect(TokenType.Identifier).value;

        List<object> constructorParams = new List<object>();

        while (ts.Peek().type != TokenType.RBracket)
        {
            constructorParams.Add(ParseValue(ts));
        }

        ts.Expect(TokenType.RBracket);
        ts.Expect(TokenType.LCurly);

        Type objType = Type.GetType(dataType);
        if (objType is null)
            objType = Type.GetType("Bearing."+dataType);
        if (objType is null)
            objType = Type.GetType("OpenTK.Mathematics."+dataType+", OpenTK.Mathematics");
        if (objType is null)
            objType = Type.GetType("BulletSharp."+dataType+", BulletSharp");

        if (objType is null)
            Logger.LogError("Could not find type: " + dataType);

        object? result = Activator.CreateInstance(objType, constructorParams.ToArray());

        if (result is null)
            throw new Exception($"Something went wrong creating an object of type: {dataType}, have you checked the namespace???");

        // parameters
        while (ts.Peek().type != TokenType.RCurly)
        {
            string prop = (string)ts.Expect(TokenType.Identifier).value;
            object value = ParseValue(ts);

            Type propType = objType.GetProperty(prop)?.PropertyType;

            if (value.GetType() == typeof(List<object>))
            {
                // if it's a list then we have to convert the type from the generic List<object> to the type that the property expects

                Type generic = propType.GetGenericArguments()[0];

                Type newListType = typeof(List<>).MakeGenericType(generic);
                IList? newList = (IList?)Activator.CreateInstance(newListType);

                if (newList is null)
                    throw new Exception($"Failed to create a list of type: {newListType.FullName}");

                foreach (object val in (List<object>)value)
                {
                    newList.Add(val);
                }

                value = newList;
            }

            objType.GetProperty(prop)?.SetValue(result, value);
        }

        ts.Consume();

        return result;
    }

    public static GameObject Load(Resource resource)
    {
        return (GameObject)ParseObject(TokenStream.FromResource(resource));
    }
}