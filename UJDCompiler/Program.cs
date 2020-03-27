using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UJDCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
                Error("Invalid argument length!\nUsage: UJDCompiler.exe [filename]... [dest jar] [javac path]");
            var files = args[..^2].SelectMany(p => Directory.GetFiles(Path.GetDirectoryName(p), Path.GetFileName(p)));
            var jfiles = files.Select(GetJavaFromUjdPath);
            var destFolder = args[^2];
            var outputJarPath = Path.Combine(destFolder, "bin", Path.GetFileNameWithoutExtension(args[^2])+".jar");
            var javacPath = args[^1];
            var jarPath = Path.Combine(Path.GetDirectoryName(args[^1]), "jar");
            Directory.CreateDirectory(Path.Combine(destFolder, "bin"));

            RunTask("Compile started", "Compiled", () =>
            {
                RunTask("Load UJD-Table", "UJD-Table Loaded", UjdToken.LoadTree);
                foreach (var file in files)
                {
                    var tokens = RunTask("Tokenizer started", "Tokenizer finished",
                                         () => Lexer.GetTokens(new StreamReader(File.OpenRead(file))));
                    var jcode = RunTask("Convert to Java code started", "Convert to Java code done",
                                        () =>
                                            string.Join("",
                                                        tokens.Select(p => p.JToken.Select(x => x.Code))
                                                              .SelectMany(p => p)));
                    RunTask("Write .java file", ".java file written",
                            () =>
                                File.WriteAllText(GetJavaFromUjdPath(file),
                                                  jcode));
                } 

                RunTask("Running javaw", "javaw done", () =>
                            Process.Start(javacPath, string.Join(" ", jfiles) + " -d "+destFolder)?.WaitForExit());
                RunTask("Building jar", "jar built", () =>
                            Process.Start(jarPath, "cvf "+ outputJarPath + " " + Path.Combine(destFolder, "*.class"))?.WaitForExit());
            });
            // Schreiben in .java Datei
            // javac.exe aufrufen
        }

        private static string GetJavaFromUjdPath(string file) =>
            Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".java";

        private static void RunTask(string pre, string post, Action action) =>
            RunTask<object>(pre, post, () =>
            {
                action();
                return null;
            });

        private static T RunTask<T>(string pre, string post, Func<T> action)
        {
            Console.WriteLine(pre);
            var v = action();
            Console.WriteLine(post);
            return v;
        }

        private static void Error(string message)
        {
            Console.WriteLine(message);
            Environment.Exit(1);
        }
    }
}
