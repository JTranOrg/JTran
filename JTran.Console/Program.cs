using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using JTranProject = JTran.Project.Project;

using JTran.Common;
using JTran.Project;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Diagnostics;
using System.Linq.Expressions;

[assembly: InternalsVisibleTo("JTran.Console.UnitTests")]

namespace JTran.Console
{
    /****************************************************************************/
    /****************************************************************************/
    public class Program
    {
        /****************************************************************************/
        internal static async Task<int> Main(string[] args)
        {
            var rtnCode = await InternalMain(args);

            return rtnCode;
        }

        #region Private 

        /****************************************************************************/
        private static async Task<int> InternalMain(string[] args)
        {
            if(args.Length == 0 || args[0] == "/help" || args[0] == "-help")
            { 
                ShowHelp();
                return 0;
            }

            var index             = 0;
            var transform         = "";
            var source            = "";
            var output            = "";
            var includes          = "";
            var documents         = "";
            var extensionPath     = "";
            var transformParams   = "";
            var argumentProvider  = "";
            JTranProject? project = null;
            var projectPath       = "";
            bool split            = false;
            bool serially         = false;

            // Set up all the paths
            while(index < args.Length)
            {
                var arg = args[index];

                if(index < args.Length)
                { 
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

                    if(arg == "/tp" || arg == "-tp")
                        transformParams = args[++index];

                    if(arg == "/e" || arg == "-e")
                        extensionPath = args[++index];

                    if(arg == "/a" || arg == "-a")
                        argumentProvider = args[++index];

                    if(arg == "/se" || arg == "-se")
                       serially = true;
                }

                if(arg == "/m" || arg == "-m")
                    split = true;

                ++index;
            }

            if(!string.IsNullOrWhiteSpace(projectPath))
            { 
                if(!File.Exists(projectPath))
                { 
                    WriteError("No transform file specified.");
                    return 1;
                }

                using Stream file = File.OpenRead(projectPath);

                try
                { 
                    project = file.ToObject<JTranProject>();
                }
                catch(JsonParseException)
                {
                    WriteError("Project file is not a valid json file");
                    return 1;
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

            if(!string.IsNullOrWhiteSpace(transformParams))
                project.AddArguments(transformParams);

            if(!string.IsNullOrWhiteSpace(extensionPath))
                project.ExtensionPaths.Add(extensionPath);

            if(!string.IsNullOrWhiteSpace(argumentProvider))
                AddArgumentProvider(project, argumentProvider);

            if(split)
                project.SplitOutput = true;

            if(string.IsNullOrWhiteSpace(project.TransformPath))
            { 
                WriteError("No transform file specified.");
                return 1;
            }

            if(string.IsNullOrWhiteSpace(project.SourcePath))
            { 
                WriteError("No source file(s) specified.");
                return 1;
            }

            if(string.IsNullOrWhiteSpace(project.DestinationPath))
            { 
                WriteError("No output path specified.");
                return 1;
            }

            return await TransformFiles(project, serially);
        }

        /****************************************************************************/
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

        /****************************************************************************/
        private static Dictionary<string, string> NormalizePaths(Dictionary<string, string> paths)
        {
            foreach(var kv in paths)
                paths[kv.Key] = NormalizePath(kv.Value);

            return paths;
        }

        /****************************************************************************/
        private static async Task<int> TransformFiles(JTranProject project, bool serially)
        {        
            project.DestinationPath = NormalizePath(project.DestinationPath);
            project.SourcePath      = NormalizePath(project.SourcePath);
            project.TransformPath   = NormalizePath(project.TransformPath);
            project.IncludePaths    = NormalizePaths(project.IncludePaths);
            project.DocumentPaths   = NormalizePaths(project.DocumentPaths);

            try
            {
                var compiledProj = await CompiledProject.Load(project, (ex)=> WriteError(ex.Message));

                // Single file (no wildcards)
                if(!project.SourcePath.Contains("*") && !project.SourcePath.Contains("?"))
                {
                    System.Console.WriteLine($"Transforming file {project.SourcePath} to {project.DestinationPath}");

                    await compiledProj.Run();

                    return 0;
                }

                // Get all the files that match the wildcard specification
                var sourceFiles = Directory.GetFiles(Path.GetDirectoryName(project.SourcePath)!, Path.GetFileName(project.SourcePath));

                System.Console.WriteLine($"{sourceFiles.Length} source files found");

                if(sourceFiles.Length == 0)
                    return 0;

                await compiledProj.TransformFiles(sourceFiles, sourceFiles.Select( sourceFile=>
                {
                    return project.DestinationPath;

                }).ToArray(),
                null,
                (msg)=> System.Console.WriteLine(msg),
                (msg)=> WriteError(msg),
                serially);

                return 0;
            }
            catch(Exception ex2)
            {
                WriteError(ex2.Message);
                return 1;
            }
        }

        /****************************************************************************/
        private static void ShowHelp()
        {
            System.Console.WriteLine("-help -- Show this help screen");
            System.Console.WriteLine("");
            System.Console.WriteLine("-p -- Specify a project file");
            System.Console.WriteLine("-t -- Specify a transform. Must point to a single file.");
            System.Console.WriteLine("-s -- Specify a source document. Can include wildcards. Each source file will be transformed using the transform specified above.");
            System.Console.WriteLine("-o -- Specify an output document or folder.");
            System.Console.WriteLine("-i -- Specify an include folder.");
            System.Console.WriteLine("-include -- Specify an include folder.");
            System.Console.WriteLine("-d -- Specify a documents folder.");
            System.Console.WriteLine("-m -- Specify multiple (split) output.");
            System.Console.WriteLine("-a -- Specify an arguments provider.");
        }

        /****************************************************************************/
        private static void WriteError(string text)
        {
            WriteLine(text, ConsoleColor.Red);
        }

        /****************************************************************************/
        private static void WriteLine(string text, ConsoleColor clr)
        {
            var defaultColor = System.Console.ForegroundColor;

            System.Console.ForegroundColor = clr;

          #if DEBUG
            System.Diagnostics.Debug.WriteLine("...");
            System.Diagnostics.Debug.WriteLine(text);
            System.Diagnostics.Debug.WriteLine("...");
          #else
             System.Console.WriteLine(text);
          #endif

            System.Console.ForegroundColor = defaultColor;
        }

        /****************************************************************************/
        private static void AddArgumentProvider(JTran.Project.Project project, string providerStr)
        {
            var parts = providerStr.Split("::");
            
            if(parts.Length < 2) 
            { 
                WriteError("Arguments provider string must be in the format: <dll_path::class_name>");
                return;
            }

            try
            {
                var assembly  = Assembly.LoadFile(parts[0]);
                var classType = assembly.GetType(parts[1])!;
                var obj       = Activator.CreateInstance(classType);

                if(obj is IDictionary<string, object> dict)
                    project.ArgumentProviders.Add(dict);

            }
            catch(Exception ex) 
            {
                WriteError(ex.Message);
                return;
            }
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
