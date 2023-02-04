using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace JTran.Project
{
    /****************************************************************************/
    /****************************************************************************/
    public class CompiledProject
    { 
        public string                     Name            { get; set; }
        public string                     Transform       { get; set; }
        public string                     SourcePath      { get; set; }
        public string                     Destinations    { get; set; }
        public Dictionary<string, string> Includes        { get; set; }
        public Dictionary<string, string> Documents       { get; set; }
        public List<object>               Extensions      { get; set; } = new List<object>();

        /****************************************************************************/
        public static CompiledProject Load(Project project, Action<Exception> onError)
        {
            var compiled = new CompiledProject();

            compiled.Name       = project.Name;
            compiled.Transform  = File.ReadAllText(project.TransformPath);
            compiled.SourcePath = project.SourcePath;

            compiled.Documents  = LoadFiles(project.DocumentPaths, onError);
            compiled.Includes   = LoadFiles(project.IncludePaths, onError);

            if(project.ExtensionPaths != null)
            { 
                foreach(var extensionPath in project.ExtensionPaths)
                {
                    try
                    { 
                        var files = Directory.GetFiles(extensionPath);

                        foreach(var path in files)
                        { 
                            var types = Assembly.LoadFile(path).GetTypes();

                            compiled.Extensions.AddRange(types);
                        }
                    }
                    catch(Exception ex)
                    {
                        onError(ex);
                    }
                }
            }

            return compiled;
        }

        /****************************************************************************/
        public void Run(Stream output)
        {
             var transformer = new Transformer(this.Transform, this.Extensions, this.Includes);
             var context = new TransformerContext();

             using var source = File.OpenRead(this.SourcePath);

             transformer.Transform(source, output, context);
        }

        /****************************************************************************/
        public void TransformFile(string sourcePath, string destinationPath)
        {
             var transformer = new Transformer(this.Transform, this.Extensions, this.Includes);
             var context = new TransformerContext();

             using var source = File.OpenRead(this.SourcePath);
             using var output = File.OpenWrite(destinationPath);

             transformer.Transform(source, output, context);
        }

        #region Private

        /****************************************************************************/
        private static Dictionary<string, string> LoadFiles(Dictionary<string, string> paths, Action<Exception> onError)
        {
            if(paths == null)
                return null;

            var result = new Dictionary<string, string>();
 
            foreach(var kv in paths)
            {
                try
                { 
                    var files = Directory.GetFiles(kv.Value);

                    foreach(var file in files)
                    { 
                        var data = File.ReadAllText(file);

                        result.Add(kv.Key + "." + Path.GetFileNameWithoutExtension(file), data);
                    }
                }
                catch(Exception ex)
                {
                    onError(ex);
                }
            }
          
            return result;
        }

        #endregion
    }
}
