using System;
using System.IO;

namespace UJDCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
                Error("Invalid argument length!\nUsage: UJDCompiler.exe [filename]");
            if (!File.Exists(args[0]))
                Error("The supplied file was not found");
            Console.WriteLine("Compile started");
            Console.WriteLine("Tokenizer started");
            var tokens = Lexer.GetTokens(new StreamReader(File.OpenRead(args[0])));
            Console.WriteLine("Tokenizer finished");
            Console.WriteLine("Compiled");
        }

        private static void Error(string message)
        {
            Console.WriteLine(message);
            Environment.Exit(1);
        }
    }
}
