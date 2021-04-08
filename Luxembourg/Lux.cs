using System;
using System.Collections.Generic;
using System.IO;

namespace Luxembourg
{
    public class Lux
    {
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;
        private static readonly Interpreter _interpreter = new();

        public static void RunFile(string path)
        {
            var file = File.ReadAllText(path);
            Run(file);
            if (_hadError)
            {
                System.Environment.Exit(1);
            }

            if (_hadRuntimeError)
            {
                System.Environment.Exit(1);
            }
        }

        public static void RunPrompt()
        {
            while (true)
            {
                Console.Write("» ");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    break;
                }

                if (input.StartsWith('#'))
                {
                    ParseCommands(input);
                    continue;
                }

                Run(input);
                _hadError = false;
            }
        }

        private static void ParseCommands(string input)
        {
            var text = input.Substring(1, input.Length - 1);

            switch (text)
            {
                case "help":
                    PrintHelp();
                    break;
                case "cls":
                    Console.Clear();
                    break;
            }
        }

        private static void PrintHelp()
        {
            Console.Out.WriteLine("This should be where help happens");
        }

        public static void Run(string source)
        {
            var scanner = new Lexer(source);
            var tokens = scanner.ScanTokens();

            var parser = new Parser(tokens);
            var statement = parser.Parse();

            if (_hadError)
            {
                return;
            } 
            
            _interpreter.Interpret(statement);
            // Console.Out.WriteLine(new AstPrinter().Print(statement));
            // PrettyPrint(tokens);
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message} \n[Line {error.Token.Line}]");
            _hadRuntimeError = true;
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[Line {line}] Error {where}: {message}");
            _hadError = true;
        }

        public static void PrettyPrint(List<Token> tokens)
        {
            foreach (var token in tokens)
            {
                Console.Out.WriteLine($"[ {token.Type} : {token.Lexeme} ]");
            }
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EndOfFile)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }
    }
}