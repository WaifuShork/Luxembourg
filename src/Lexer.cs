using System.Collections.Generic;
using Luxembourg.Enums;

namespace Luxembourg
{
    public class Lexer
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new();

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;
        
        private static readonly Dictionary<string, TokenType> _keywords = new()
        {
            { "and", TokenType.And },
            { "class", TokenType.Class },
            { "else", TokenType.Else },
            { "false", TokenType.False },
            { "for", TokenType.For },
            { "procedure", TokenType.Procedure },
            { "if", TokenType.If },
            { "nil", TokenType.Nil },
            { "or", TokenType.Or },
            { "print", TokenType.Print },
            { "return", TokenType.Return },
            { "base", TokenType.Base },
            { "this", TokenType.This },
            { "true", TokenType.True },
            { "var", TokenType.Var },
            { "while", TokenType.While },
        };
        
        public Lexer(string source)
        {
            _source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }
            
            _tokens.Add(new(TokenType.EndOfFile, "", null, _line));
            return _tokens;
        }

        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        public void ScanToken()
        {
            var current = Advance();
            switch (current)
            {
                case '(':
                    AddToken(TokenType.OpenParen);
                    break;
                case ')':
                    AddToken(TokenType.CloseParen);
                    break;
                case '{':
                    AddToken(TokenType.OpenBrace);
                    break;
                case '}':
                    AddToken(TokenType.CloseBrace);
                    break;
                case ',':
                    AddToken(TokenType.Comma);
                    break;
                case '.':
                    AddToken(TokenType.Dot);
                    break;
                case '-':
                    AddToken(TokenType.Minus);
                    break;
                case '+':
                    AddToken(TokenType.Plus);
                    break;
                case ';':
                    AddToken(TokenType.Semicolon);
                    break;
                case '*':
                    AddToken(Match('*') ? TokenType.StarStar : TokenType.Star);
                    break;
                case '!':
                    AddToken(Match('=') ? TokenType.BangEquals : TokenType.Bang);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;
                
                case ' ':
                case '\r':
                case '\t':
                    // Handle whitespace
                    // Advance();
                    break;
                
                case '\n':
                    _line++;
                    break;
                
                case '"':
                    String();
                    break;
                
                case 'o':
                    if (Match('r'))
                    {
                        AddToken(TokenType.Or);
                    }
                    break;
                
                default:
                    if (IsDigit(current))
                    {
                        Number();
                    }
                    else if (IsAlpha(current))
                    {
                        Identifier();
                    }
                    else
                    {
                        Lux.Error(_line, "Unexpected character.");
                    }
                    break;
            }
        }
        
        private void Identifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            var text = Extensions.Substring(_source, _start, _current);
            if (!_keywords.TryGetValue(text, out var type))
            {
                type = TokenType.Identifier;
            }

            AddToken(type);
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void Number()
        {
            while (IsDigit(Peek()))
            {
                Advance();
            }

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            AddToken(TokenType.Number, double.Parse(Extensions.Substring(_source, _start, _current)));
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length)
            {
                return '\0';
            }

            return _source[_current + 1];
        }
        

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n')
                {
                    _line++;
                }

                Advance();
            }

            if (IsAtEnd())
            {
                Lux.Error(_line, "Unterminated string.");
                return;
            }

            Advance();

            // checked
            // This gets the value of the string without the "quotations"
            var value = Extensions.Substring(_source, _start + 1, _current - 1);
            AddToken(TokenType.String, value);
        }

        private char Peek()
        {
            if (IsAtEnd())
            {
                return '\0';
            }

            return _source[_current];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd())
            {
                return false;
            }

            if (_source[_current] != expected)
            {
                return false;
            }

            _current++;
            return true;
        }

        private char Advance()
        {
            return _source[_current++];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            var text = Extensions.Substring(_source, _start, _current);
            _tokens.Add(new(type, text, literal, _line));
        }
    }
}