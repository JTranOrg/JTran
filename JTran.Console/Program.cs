using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using JTranProject = JTran.Project.Project;

using JTran.Extensions;
using JTran.Project;

namespace JTran.Console
{
    /****************************************************************************/
    /****************************************************************************/
    public class Program
    {
        /****************************************************************************/
        static void Main(string[] args)
        {
            if(args.Length == 0 || args[0] == "/help" || args[0] == "-help")
                ShowHelp();
            else
            { 
                var index         = 0;
                var transform     = "";
                var source        = "";
                var output        = "";
                var includes      = "";
                var documents     = "";
                JTranProject project   = null;
                var projectPath   = "";

                // Set up all the paths
                while((index+1) < args.Length)
                {
                    var arg = args[index];

                    if(arg == "/p" || arg == "-p")
                        projectPath = args[++index];

                    if(arg == "/t" || arg == "-t")
                        transform = args[++index];

                    if(arg == "/s" || arg == "-s")
                        source = args[++index];

                    if(arg == "/o" || arg == "-o")
                        output = args[++index];

                    if(arg == "/i" || arg == "-i" || arg == "/include" || arg == "-include")
                        includes = args[++index];

                    if(arg == "/d" || arg == "-d")
                        documents = args[++index];

                    ++index;
                }

                if(!string.IsNullOrWhiteSpace(projectPath))
                { 
                    if(!File.Exists(projectPath))
                    { 
                        WriteError("No transform file specified.");
                        return;
                    }

                    var json = File.ReadAllText(projectPath);

                    try
                    { 
                        project = json.ToObject<JTranProject>();
                    }
                    catch(JsonParseException)
                    {
                        WriteError("Project file is not a valid json file");
                        return;
                    }

                    if(string.IsNullOrWhiteSpace(transform))
                        transform = project.TransformPath;

                    if(string.IsNullOrWhiteSpace(source))
                        source = project.SourcePath;

                    if(string.IsNullOrWhiteSpace(output))
                        output = project.DestinationPath;
                }
                else
                    project = new JTranProject();

                if(!string.IsNullOrWhiteSpace(transform))
                    project.TransformPath = transform;

                if(!string.IsNullOrWhiteSpace(source))
                    project.SourcePath = source;

                if(!string.IsNullOrWhiteSpace(output))
                    project.DestinationPath = output;

                if(!string.IsNullOrWhiteSpace(documents))
                    project.DocumentPaths.Add("", documents);

                if(!string.IsNullOrWhiteSpace(includes))
                    project.IncludePaths.Add("", includes);

                if(string.IsNullOrWhiteSpace(project.TransformPath))
                { 
                    WriteError("No transform file specified.");
                    return;
                }

                if(string.IsNullOrWhiteSpace(project.SourcePath))
                { 
                    WriteError("No source file(s) specified.");
                    return;
                }

                if(string.IsNullOrWhiteSpace(project.DestinationPath))
                { 
                    WriteError("No output path specified.");
                    return;
                }

                TransformFiles(project);
             }
        }

        #region Private 

        private static string NormalizePath(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
                return null;

            // Probably a full path
            if(path.Contains(":"))
                return path;

            var location = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin");

            return Path.Combine(location, path);
        }

        private static Dictionary<string, string> NormalizePaths(Dictionary<string, string> paths)
        {
            foreach(var kv in paths)
                paths[kv.Key] = NormalizePath(kv.Value);

            return paths;
        }

        /****************************************************************************/
        private static void TransformFiles(JTranProject project)
        {        
            project.DestinationPath = NormalizePath(project.DestinationPath);
            project.SourcePath      = NormalizePath(project.SourcePath);
            project.TransformPath   = NormalizePath(project.TransformPath);
            project.IncludePaths    = NormalizePaths(project.IncludePaths);
            project.DocumentPaths   = NormalizePaths(project.DocumentPaths);

            try
            {
                var compiledProj = CompiledProject.Load(project, (ex)=> System.Console.Write(ex.Message));

                // Single file
                if(!string.IsNullOrWhiteSpace(Path.GetExtension(project.SourcePath)))
                {
                    using var output = File.OpenWrite(project.DestinationPath);

                    compiledProj.Run(output);

                    return;
                }

                var sourceFiles = Directory.GetFiles(project.SourcePath);

                System.Console.WriteLine($"{sourceFiles.Length} source files found");

                if(sourceFiles.Length == 0)
                    return;

                foreach(var sourceFile in sourceFiles)
                {
                    var dest = project.DestinationPath;

                    if(string.IsNullOrWhiteSpace(Path.GetExtension(dest)))
                        dest = Path.Combine(dest, Path.GetFileName(sourceFile));

                    compiledProj.TransformFile(sourceFile, dest);
                }
            }
            catch(Exception ex2)
            {
                System.Console.WriteLine(ex2.Message);
                return;
            }
        }

        /****************************************************************************/
        private static void ShowHelp()
        {
            System.Console.WriteLine("/help -- Show this help screen");
            System.Console.WriteLine("");
            System.Console.WriteLine("/c -- Specify a config file");
            System.Console.WriteLine("/t -- Specify a transform. Must point to a single file.");
            System.Console.WriteLine("/s -- Specify a source document. Can include wildcards. Each source file will be transformed using the transform specified above.");
            System.Console.WriteLine("/o -- Specify an output document.");
            System.Console.WriteLine("/i -- Specify an include folder.");
            System.Console.WriteLine("/include -- Specify an include folder.");
            System.Console.WriteLine("/d -- Specify a documents folder.");
        }

        private static void WriteError(string text)
        {
            System.Console.WriteLine(text, ConsoleColor.Red);
        }

        private static void WriteLine(string text, ConsoleColor clr)
        {
            var defaultColor = System.Console.ForegroundColor;

            System.Console.ForegroundColor = clr;
            System.Console.WriteLine(text);
            System.Console.ForegroundColor = defaultColor;
        }

        #endregion
    }

    #region Extensions

    internal static class Extensions
    {
        internal static string SubstringBefore(this string val, string before)
        {
            var index = val.IndexOf(before);

            if(index == -1)
                return val;

            return val.Substring(0, index);
        }    
    }

    #endregion
}
