using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
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

    private enum TokenType { EOS, Identifier, Object, LBracket, RBracket, LCurly, RCurly };
    
    private class Token()
    {
        public static readonly Token EOS = new Token() { type = TokenType.EOS };

        public TokenType type;

        public object value = "";
    }

    private static List<Token> Tokenise(string data)
    {
        List<Token> result = new List<Token>();

        StringBuilder sb = new StringBuilder();

        int i = 0;

        Action TokeniseIdentifier = () => {
            sb.Clear();
            while (data[i] != ')' && data[i] != ':')
            {
                sb.Append(data[i]);
                i++;
            }
        };

        Action TokeniseObject = () => {
            result.Add(new Token() { type = TokenType.LBracket });
            i++;

            TokeniseIdentifier();

            if (sb.Length == 0)
                throw new Exception($"Failed to tokenise BST! Expected 'Identifier', got '{data[i]}' instead!");

            result.Add(new Token() { type = TokenType.Identifier, value = sb.ToString() });
            i--;
        };

        Action TokeniseNumber = () => {
            sb.Clear();

            bool isFloat = false;
            while (char.IsDigit(data[i]) || data[i] == '.')
            {
                if (data[i] == '.')
                    isFloat = true;

                sb.Append(data[i]);
                i++;
            }
            result.Add(new Token() { type = TokenType.Object, value = isFloat ? float.Parse(sb.ToString()) : int.Parse(sb.ToString()) });
            i--;
        };

        Action TokeniseString = () => {
            sb.Clear();

            i++;
            while (data[i] != '"')
            {
                sb.Append(data[i]);
                i++;
            }
            result.Add(new Token() { type = TokenType.Object, value = sb.ToString() });
        };

        Action TokeniseValue = () => {
            switch (data[i])
            {
                case '(':
                    TokeniseObject();
                    break;
                case '"': TokeniseString(); break;
                default:
                    if (char.IsDigit(data[i]))
                    {
                        TokeniseNumber();
                        break;
                    }
                    break;
            }
        };

        Action TokeniseParameters = () => {
            sb.Clear();

            result.Add(new Token() { type = TokenType.LCurly });

            i++;
            while (data[i] != '}')
            {
                if (data[i] == ',')
                {
                    i++;
                    continue;
                }

                TokeniseIdentifier();
                if (sb.Length == 0)
                    throw new Exception($"Failed to tokenise BST! Expected 'Identifier', got '{data[i]}' instead!");
                result.Add(new Token() { type = TokenType.Identifier, value = sb.ToString() });
                i++;
                TokeniseValue();
                i++;
            }

            result.Add(new Token() { type = TokenType.RCurly });
        };

        for (;i < data.Length; i++)
        {
            char c = data[i];

            switch (c)
            {
                case '(':
                    TokeniseObject();
                    break;
                case ')':
                    result.Add(new Token() { type = TokenType.RBracket });
                    break;
                case '{':
                    TokeniseParameters();
                    break;
                case '}':
                    result.Add(new Token() { type = TokenType.RCurly });
                    break;
                case ',': continue;
                case '"': TokeniseString(); break;
                default:
                    if (char.IsDigit(c))
                    {
                        TokeniseNumber();
                        break;
                    }

                    switch (result[^1].type)
                    {
                        case TokenType.LCurly:
                            sb.Clear();
                            sb.Append(c);
                            while (data[i+1] != '}')
                            {
                                sb.Append(data[i+1]);
                                i++;
                            }

                            result.Add(new Token() { type = TokenType.Identifier, value = sb.ToString() });
                            break;
                    }
                    break;
            }
        }

        return result;
    }

    private class TokenStream()
    {
        private List<Token> tokens = new List<Token>();
        private int currentIndex;

        public static TokenStream FromResource(Resource resource)
        {
            TokenStream ts = new TokenStream();

            ts.tokens = Tokenise(Resources.ReadAllText(resource).Replace("\n","").Replace("\t",""));

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

    private static object ParseValue(TokenStream ts)
    {
        switch (ts.Peek().type)
        {
            case TokenType.LBracket:
                return ParseObject(ts);
            default:
                return ts.Consume().value;
        }
    }

    private static object ParseObject(TokenStream ts)
    {
        Logger.Log("parsing object!!");

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

        object? result = Activator.CreateInstance(objType, constructorParams.ToArray());

        if (result is null)
            throw new Exception($"Something went wrong creating an object of type: {dataType}, have you checked the namespace???");

        // parameters
        while (ts.Peek().type != TokenType.RCurly)
        {
            string prop = (string)ts.Expect(TokenType.Identifier).value;
            object value = ts.Expect(TokenType.Object).value;

            objType.GetProperty(prop)?.SetValue(result, value);
        }

        ts.Consume();

        return result;
    }
}