using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Win32;

namespace UJDCompiler
{
    public class CommandLine
    {
        public static CommandLine Attr { get; private set; }
        public CommandLine()
        {
            Attr = this;
        }

        

        [Argument(0, "output directory", "The folder where the .java, .class and .jar files will be created"), Required,
         DirectoryExists]
        public string JavaOutputDirectory { get; private set; }

        [Argument(1, "jar filename", "The name of the .jar file"), Required]
        public string JarFilename { get; private set; }

        [Option(Description = "Suppress output")]
        public bool Quiet { get; private set; }

        [Option(Description =
                       "The files to be compiled. Wildcards allowed. Can be supplied multiple times."), Required]
        public string[] InputFiles { get; private set; }

        [Option("-j|--keep-java", Description = "Keep the .java files?")]
        public bool KeepJavaFiles { get; private set; }

        [Option("-c|--keep-class", Description = "Keep the .class files?")]
        public bool KeepClassFiles { get; private set; }

        [Option("-n|--no-jar", Description = "Do not build .jar?")]
        public bool NoJarBuild { get; private set; }

        [Option("-d <FILE>", Description = "Override the DialectLookup file"), FileExists]
        public string DialectLookupFile { get; } = "DialectLookup";

        [Option("-ds <CHAR>", Description = "Override the Separator char")]
        public char DialectLookupSeparator { get; } = '0';

        [Option("-l|--left-char <CHAR>", Description = "The left char in the Tree")]
        public char LeftChar { get; } = ' ';

        [Option("-r|--right-char <CHAR>", Description = "The right char in the Tree")]
        public char RightChar { get; } = '\t';

        [Option("-jc|--javac <PATH>", Description = "The javac path"), FileExists]
        public string JavacPath { get; private set; }

        [Option("-jar <PATH>", Description = "The java-jar path"), FileExists]
        public string JarPath { get; private set; }
        private void Init()
        {
            var javaDir = GetJavaDir();
            if (javaDir == null) return;
            javaDir = Path.Combine(javaDir, "bin");
            if (JavacPath == null) JavacPath = Path.Combine(javaDir, "javac");
            if (JarPath   == null) JarPath   = Path.Combine(javaDir, "jar");

            InputFiles = InputFiles.SelectMany(p => Directory.GetFiles(Path.GetDirectoryName(p), Path.GetFileName(p)))
                                   .ToArray();

            if (JavacPath != null && (JarPath != null || NoJarBuild)) return;
            Console.Error.WriteLine("Could not determine java path. Please provide the javac Path and the jar Path.");
            Environment.Exit(1);
        }

        private string GetJavaDir()
        {
            var home = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (home != null) return home;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;
            const string javaKey = "SOFTWARE\\JavaSoft\\";

            var reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var baseKey = reg.OpenSubKey(javaKey + "JAR")??reg.OpenSubKey(javaKey + "Java Development Kit");

            var       currentVersion = baseKey.GetValue("CurrentVersion").ToString();
            using var homeKey        = baseKey.OpenSubKey(currentVersion);
            return homeKey?.GetValue("JavaHome")?.ToString();
        }

        private void OnExecute()
        {
            Init();
            Program.Compile();
        }
    }
}