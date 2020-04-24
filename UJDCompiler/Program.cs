using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace UJDCompiler
{
    public partial class Program
    {
        private static void Main(string[] args) =>
            CommandLineApplication.Execute<Program>(args);

        // ReSharper disable UnusedMember.Local
        private void OnExecute()
        {
            Init();
            RunTask("Compile started", "Compiled", () =>
            {
                RunTask("Load UJD-Table", "UJD-Table Loaded",
                        () => UjdToken.LoadTree(DialectLookupFile, DialectLookupSeparator));
                var sb = new StringBuilder();
                foreach (var file in InputFiles)
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
                            Process.Start(JavacPath,
                                          sb + "-d " + JavaOutputDirectory)?.WaitForExit());
                RunTask("Building jar", "jar built", () =>
                {
                    var output = Path.Combine(JavaOutputDirectory, "bin");
                    Directory.CreateDirectory(output);
                    output = Path.Combine(output, JarFilename.Replace(".jar", "") + ".jar");
                    Process.Start(new ProcessStartInfo(JarPath,
                                                       "cvf " + output + " *")
                                      {RedirectStandardOutput = Quiet})
                          ?.WaitForExit();
                }, !NoJarBuild);
                RunTask("Deleting .java files",                       ".java files deleted", () =>
                            DeleteAll(JavaOutputDirectory, "*.java"), !KeepJavaFiles);
                RunTask("Deleting .class files",                       ".class files deleted", () =>
                            DeleteAll(JavaOutputDirectory, "*.class"), !KeepClassFiles);
            });
        }

        private void DeleteAll(string path, string wildcard)
        {
            foreach (var file in new DirectoryInfo(path).EnumerateFiles(wildcard)) file.Delete();
        }

        private string GetJavaFromUjdPath(string file) =>
            Path.Combine(JavaOutputDirectory, Path.GetFileNameWithoutExtension(file)) + ".java";

        private void RunTask(string pre, string post, Action action, bool run = true)
        {
            if (run)
                RunTask<object>(pre, post, () =>
                {
                    action();
                    return null;
                });
        }

        private T RunTask<T>(string pre, string post, Func<T> action, bool run = true)
        {
            if (!run) return default;
            if (!Quiet) Console.WriteLine(pre);
            var v = action();
            if (!Quiet) Console.WriteLine(post);
            return v;
        }
    }
}