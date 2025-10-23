// Minimal JSON parser based on Unity's MiniJSON (MIT License).
// Source: https://gist.github.com/darktable/1411710

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace IFC.Systems.Util
{
    public static class MiniJSON
    {
        public static object Deserialize(string json)
        {
            if (json == null)
            {
                return null;
            }

            // Be forgiving about BOM/zero-width characters at the start of files
            // that can appear when editing JSON with various tools on Windows/macOS.
            // These characters cause the simple parser to return NONE on the first token.
            json = TrimLeadingControlChars(json);

            return Parser.Parse(json);
        }

        private static string TrimLeadingControlChars(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            int i = 0;
            while (i < s.Length)
            {
                char c = s[i];
                // Remove common problematic leading chars: BOM, zero-width space, non-breaking space
                if (c == '\uFEFF' || c == '\u200B' || c == '\u00A0')
                {
                    i++;
                    continue;
                }
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }
                break;
            }
            return i > 0 ? s.Substring(i) : s;
        }

        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }

        private sealed class Parser : IDisposable
        {
            private const string WORD_BREAK = "{}[],:\"";

            private StringReader _json;

            private Parser(string jsonString)
            {
                _json = new StringReader(jsonString);
            }

            public static object Parse(string jsonString)
            {
                using (var instance = new Parser(jsonString))
                {
                    return instance.ParseValue();
                }
            }

            public void Dispose()
            {
                _json.Dispose();
                _json = null;
            }

            private Dictionary<string, object> ParseObject()
            {
                var table = new Dictionary<string, object>();

                // ditch opening brace
                _json.Read();

                while (true)
                {
                    switch (NextToken)
                    {
                        case Token.NONE:
                            return null;
                        case Token.CURLY_CLOSE:
                            return table;
                        default:
                            // name
                            string name = ParseString();
                            if (name == null)
                            {
                                return null;
                            }

                            // :
                            if (NextToken != Token.COLON)
                            {
                                return null;
                            }
                            _json.Read();

                            // value
                            table[name] = ParseValue();
                            break;
                    }
                }
            }

            private List<object> ParseArray()
            {
                var array = new List<object>();

                // ditch opening bracket
                _json.Read();

                var parsing = true;
                while (parsing)
                {
                    Token nextToken = NextToken;

                    switch (nextToken)
                    {
                        case Token.NONE:
                            return null;
                        case Token.SQUARE_CLOSE:
                            _json.Read();
                            return array;
                        case Token.COMMA:
                            _json.Read();
                            break;
                        default:
                            object value = ParseValue();
                            array.Add(value);
                            break;
                    }
                }

                return array;
            }

            private object ParseValue()
            {
                Token nextToken = NextToken;
                switch (nextToken)
                {
                    case Token.STRING:
                        return ParseString();
                    case Token.NUMBER:
                        return ParseNumber();
                    case Token.CURLY_OPEN:
                        return ParseObject();
                    case Token.SQUARE_OPEN:
                        return ParseArray();
                    case Token.TRUE:
                        return true;
                    case Token.FALSE:
                        return false;
                    case Token.NULL:
                        return null;
                    default:
                        return null;
                }
            }

            private string ParseString()
            {
                var s = new StringBuilder();
                char c;

                _json.Read(); // ditch opening quote

                bool parsing = true;
                while (parsing)
                {
                    if (_json.Peek() == -1)
                    {
                        break;
                    }

                    c = NextChar;
                    switch (c)
                    {
                        case '"':
                            parsing = false;
                            break;
                        case '\\':
                            if (_json.Peek() == -1)
                            {
                                parsing = false;
                                break;
                            }

                            c = NextChar;
                            switch (c)
                            {
                                case '"':
                                case '\\':
                                case '/':
                                    s.Append(c);
                                    break;
                                case 'b':
                                    s.Append('\b');
                                    break;
                                case 'f':
                                    s.Append('\f');
                                    break;
                                case 'n':
                                    s.Append('\n');
                                    break;
                                case 'r':
                                    s.Append('\r');
                                    break;
                                case 't':
                                    s.Append('\t');
                                    break;
                                case 'u':
                                    var hex = new char[4];

                                    for (int i = 0; i < 4; i++)
                                    {
                                        hex[i] = NextChar;
                                    }

                                    s.Append((char)Convert.ToInt32(new string(hex), 16));
                                    break;
                            }
                            break;
                        default:
                            s.Append(c);
                            break;
                    }
                }

                return s.ToString();
            }

            private object ParseNumber()
            {
                string number = NextWord;

                if (number.IndexOf('.') == -1)
                {
                    long parsedInt;
                    long.TryParse(number, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedInt);
                    return parsedInt;
                }

                double parsedDouble;
                double.TryParse(number, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedDouble);
                return parsedDouble;
            }

            private void EatWhitespace()
            {
                while (_json.Peek() != -1)
                {
                    char c = (char)_json.Peek();
                    if (!char.IsWhiteSpace(c))
                    {
                        break;
                    }
                    _json.Read();
                }
            }

            private char NextChar => (char)_json.Read();

            private Token NextToken
            {
                get
                {
                    EatWhitespace();

                    if (_json.Peek() == -1)
                    {
                        return Token.NONE;
                    }

                    char c = (char)_json.Peek();
                    switch (c)
                    {
                        case '{':
                            return Token.CURLY_OPEN;
                        case '}':
                            _json.Read();
                            return Token.CURLY_CLOSE;
                        case '[':
                            return Token.SQUARE_OPEN;
                        case ']':
                            _json.Read();
                            return Token.SQUARE_CLOSE;
                        case ',':
                            return Token.COMMA;
                        case '"':
                            return Token.STRING;
                        case ':':
                            return Token.COLON;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '-':
                            return Token.NUMBER;
                    }

                    string word = NextWord;

                    switch (word)
                    {
                        case "false":
                            return Token.FALSE;
                        case "true":
                            return Token.TRUE;
                        case "null":
                            return Token.NULL;
                    }

                    return Token.NONE;
                }
            }

            private string NextWord
            {
                get
                {
                    var word = new StringBuilder();
                    while (_json.Peek() != -1)
                    {
                        char c = (char)_json.Peek();
                        if (char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1)
                        {
                            break;
                        }

                        word.Append(c);
                        _json.Read();
                    }
                    return word.ToString();
                }
            }

            private enum Token
            {
                NONE,
                CURLY_OPEN,
                CURLY_CLOSE,
                SQUARE_OPEN,
                SQUARE_CLOSE,
                COLON,
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL
            }
        }

        private sealed class Serializer
        {
            private StringBuilder _builder;

            private Serializer()
            {
                _builder = new StringBuilder();
            }

            public static string Serialize(object obj)
            {
                var instance = new Serializer();
                instance.SerializeValue(obj);
                return instance._builder.ToString();
            }

            private void SerializeValue(object value)
            {
                if (value == null)
                {
                    _builder.Append("null");
                }
                else if (value is string)
                {
                    SerializeString((string)value);
                }
                else if (value is bool)
                {
                    _builder.Append((bool)value ? "true" : "false");
                }
                else if (value is IList)
                {
                    SerializeArray((IList)value);
                }
                else if (value is IDictionary)
                {
                    SerializeObject((IDictionary)value);
                }
                else if (IsNumeric(value))
                {
                    SerializeNumber(Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture));
                }
                else
                {
                    SerializeString(value.ToString());
                }
            }

            private void SerializeObject(IDictionary obj)
            {
                bool first = true;
                _builder.Append('{');

                foreach (object e in obj.Keys)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                    }

                    SerializeString(e.ToString());
                    _builder.Append(':');
                    SerializeValue(obj[e]);

                    first = false;
                }

                _builder.Append('}');
            }

            private void SerializeArray(IList anArray)
            {
                _builder.Append('[');

                bool first = true;

                foreach (object obj in anArray)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                    }

                    SerializeValue(obj);
                    first = false;
                }

                _builder.Append(']');
            }

            private void SerializeString(string str)
            {
                _builder.Append('"');

                char[] charArray = str.ToCharArray();
                for (int i = 0; i < charArray.Length; i++)
                {
                    char c = charArray[i];
                    switch (c)
                    {
                        case '"':
                            _builder.Append("\\\"");
                            break;
                        case '\\':
                            _builder.Append("\\\\");
                            break;
                        case '\b':
                            _builder.Append("\\b");
                            break;
                        case '\f':
                            _builder.Append("\\f");
                            break;
                        case '\n':
                            _builder.Append("\\n");
                            break;
                        case '\r':
                            _builder.Append("\\r");
                            break;
                        case '\t':
                            _builder.Append("\\t");
                            break;
                        default:
                            if (c < ' ')
                            {
                                _builder.AppendFormat("\\u{0:X4}", (int)c);
                            }
                            else
                            {
                                _builder.Append(c);
                            }
                            break;
                    }
                }

                _builder.Append('"');
            }

            private void SerializeNumber(double number)
            {
                _builder.Append(number.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            }

            private static bool IsNumeric(object value)
            {
                return value is sbyte || value is byte || value is short || value is ushort ||
                       value is int || value is uint || value is long || value is ulong ||
                       value is float || value is double || value is decimal;
            }
        }
    }
}
