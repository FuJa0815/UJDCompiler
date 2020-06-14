using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace UJDCompiler
{
    public class Program
    {

        private static void Main(string[] args) =>
            CommandLineApplication.Execute<CommandLine>(args);

        // ReSharper disable UnusedMember.Local
        internal static void Compile()
        {
            RunTask("Compile started", "Compiled", () =>
            {
                RunTask("Load UJD-Table", "UJD-Table Loaded",
                        () => UjdToken.LoadTree(CommandLine.Attr.DialectLookupFile, CommandLine.Attr.DialectLookupSeparator));
                var sb = new StringBuilder();
                foreach (var file in CommandLine.Attr.InputFiles)
                {
                    var tokens = RunTask("Tokenizer started", "Tokenizer finished",
                                         () => Lexer.GetTokens(new StreamReader(File.OpenRead(file))));
                    var jCode = RunTask("Convert to Java code started", "Convert to Java code done",
                                        () =>
                                            string.Join("",
                                                        tokens.Select(p => p.JToken.Select(x => x.Code))
                                                              .SelectMany(p => p)));
                    var jPath = GetJavaFromUjdPath(file);
                    sb.Append(jPath + " ");
                    RunTask("Write .java file", ".java file written",
                            () =>
                                File.WriteAllText(jPath,
                                                  jCode));
                }


                RunTask("Running javac", "javac done", () =>
                            Process.Start(CommandLine.Attr.JavacPath,
                                          sb + "-d " + CommandLine.Attr.JavaOutputDirectory)?.WaitForExit());
                RunTask("Building jar", "jar built", () =>
                {
                    var output = Path.Combine(CommandLine.Attr.JavaOutputDirectory, "bin");
                    Directory.CreateDirectory(output);
                    output = Path.Combine(output, CommandLine.Attr.JarFilename.Replace(".jar", "") + ".jar");
                    Process.Start(new ProcessStartInfo(CommandLine.Attr.JarPath,
                                                       "cvf " + output + " *")
                                      {RedirectStandardOutput = CommandLine.Attr.Quiet})
                          ?.WaitForExit();
                }, !CommandLine.Attr.NoJarBuild);
                RunTask("Deleting .java files",                       ".java files deleted", () =>
                            DeleteAll(CommandLine.Attr.JavaOutputDirectory, "*.java"), !CommandLine.Attr.KeepJavaFiles);
                RunTask("Deleting .class files",                       ".class files deleted", () =>
                            DeleteAll(CommandLine.Attr.JavaOutputDirectory, "*.class"), !CommandLine.Attr.KeepClassFiles);
            });
        }

        private static void DeleteAll(string path, string wildcard)
        {
            foreach (var file in new DirectoryInfo(path).EnumerateFiles(wildcard)) file.Delete();
        }

        private static string GetJavaFromUjdPath(string file) =>
            Path.Combine(CommandLine.Attr.JavaOutputDirectory, Path.GetFileNameWithoutExtension(file)) + ".java";

        private static void RunTask(string pre, string post, Action action, bool run = true)
        {
            if (run)
                RunTask<object>(pre, post, () =>
                {
                    action();
                    return null;
                });
        }

        private static T RunTask<T>(string pre, string post, Func<T> action, bool run = true)
        {
            if (!run) return default;
            if (!CommandLine.Attr.Quiet) Console.WriteLine(pre);
            var v = action();
            if (!CommandLine.Attr.Quiet) Console.WriteLine(post);
            return v;
        }
    }
}