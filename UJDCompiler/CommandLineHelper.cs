using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Win32;

namespace UJDCompiler
{
    internal partial class Program
    {

        [Argument(0, "output directory", "The folder where the .java, .class and .jar files will be created"), Required,
         DirectoryExists]
        public string JavaOutputDirectory { get; }

        [Argument(1, "jar filename", "The name of the .jar file"), Required]
        public string JarFilename { get; }

        [Option(Description = "Suppress output")]
        public bool Quiet { get; }

        [Option(Description =
             "The files to be compiled.Wildcards allowed"), Required]
        public string[] InputFiles { get; private set; }

        [Option("-j|--keep-java", Description = "Keep the .java files?")]
        public bool KeepJavaFiles { get; }

        [Option("-c|--keep-class", Description = "Keep the .class files?")]
        public bool KeepClassFiles { get; }

        [Option("-n|--no-jar", Description = "Do not build .jar?")]
        public bool NoJarBuild { get; }

        [Option("-d <FILE>", Description = "Override the DialectLookup file"), FileExists]
        public string DialectLookupFile => "DialectLookup";

        [Option("-ds <CHAR>", Description = "Override the Separator char")]
        public char DialectLookupSeparator => '0';

        [Option("-l|--left-char <CHAR>", Description = "The left char in the Tree")]
        public char LeftChar => ' ';

        [Option("-r|--right-char <CHAR>", Description = "The right char in the Tree")]
        public char RightChar => '\t';

        [Option("-jc|--javac <PATH>", Description = "The javac path"), FileExists]
        public string JavacPath { get; private set; }

        [Option("-jar <PATH>", Description = "The java-jar path"), FileExists]
        public string JarPath { get; private set; }

        public static Program Attr { get; private set; }
        private void Init()
        {
            //Set JavawPath & JarPath
            var javaDir = GetJavaDir();
            if (javaDir == null) return;
            javaDir = Path.Combine(javaDir, "bin");
            if (JavacPath == null) JavacPath = Path.Combine(javaDir, "javac");
            if (JarPath   == null) JarPath   = Path.Combine(javaDir, "jar");
            
            InputFiles = InputFiles.SelectMany(p => Directory.GetFiles(Path.GetDirectoryName(p), Path.GetFileName(p)))
                                   .ToArray();

            Attr = this;

            if (JavacPath != null && (JarPath != null || NoJarBuild)) return;
            Console.Error.WriteLine("Could not determine java path. Please provide the javac Path and the jar Path.");
            Environment.Exit(1);
        }

        private string GetJavaDir()
        {
            var home = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (home != null) return home;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;
            const string javaKey = "SOFTWARE\\JavaSoft\\JDK";

            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                                           .OpenSubKey(javaKey);
            var       currentVersion = baseKey.GetValue("CurrentVersion").ToString();
            using var homeKey        = baseKey.OpenSubKey(currentVersion);
            return homeKey?.GetValue("JavaHome")?.ToString();
        }
    }
}