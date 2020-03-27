using System;
using System.IO;
using System.Linq;

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
            Console.WriteLine("Load UJD-Table");
            UjdToken.LoadTree();
            Console.WriteLine("UJD-Table Loaded");
            Console.WriteLine("Tokenizer started");
            var tokens = Lexer.GetTokens(new StreamReader(File.OpenRead(args[0])));
            Console.WriteLine("Tokenizer finished");
            Console.WriteLine("Convert to Java code started");
            var jcode = string.Join("", tokens.Select(p => p.JToken.Select(x => x.Code)).SelectMany(p => p));
            Console.WriteLine("Convert to Java code done");
            // Schreiben in .java Datei
            // javaw.exe aufrufen
            Console.WriteLine("Compiled");
        }

        // [[1,2],[3,4]] => [1,2,3,4]

        private static void Error(string message)
        {
            Console.WriteLine(message);
            Environment.Exit(1);
        }
    }
}
