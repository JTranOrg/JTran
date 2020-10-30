using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using JTran;
using System.Reflection;

namespace JTran.Console
{
    /****************************************************************************/
    /****************************************************************************/
    class Program
    {
        /****************************************************************************/
        static void Main(string[] args)
        {
            if(args.Length == 0 || args[0] == "/help" || args[0] == "-help")
                ShowHelp();
            else
            { 
                var index     = 0;
                var transform = "";
                var source    = "";
                var output    = "";
                var includes = "";
                var documents = "";
                var config    = "";

                // Set up all the paths
                while((index+1) < args.Length)
                {
                    var arg = args[index];

                    if(arg == "/c" || arg == "-c")
                        config = args[++index];

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

                if(string.IsNullOrWhiteSpace(transform))
                { 
                    System.Console.WriteLine("No transform file specified.");
                    return;
                }

                if(string.IsNullOrWhiteSpace(source))
                { 
                    System.Console.WriteLine("No source file(s) specified.");
                    return;
                }

                if(string.IsNullOrWhiteSpace(output))
                { 
                    System.Console.WriteLine("No output path specified.");
                    return;
                }

                TransformFiles(config, transform, source, output, includes, documents);
             }
        }

        #region Private 

        /****************************************************************************/
        private static void TransformFiles(string configPath, string transform, string source, string output, string includes, string documents)
        {        
            var      sTransform  = "";
            string[] sourceFiles = null;
            Config   config      = null;

            if(!string.IsNullOrWhiteSpace(configPath))
            {
                try
                {
                    if(!configPath.Contains(Path.DirectorySeparatorChar))
                        configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), configPath);

                    var configSource = File.ReadAllText(configPath);

                    config = JsonConvert.DeserializeObject<Config>(configSource);

                    if(!string.IsNullOrWhiteSpace(includes))
                        config.IncludePath = includes;
                }
                catch(Exception ex)
                {
                    System.Console.WriteLine("Specified config file does not exist or is invalid: " + ex.Message);
                }            
            }

            try
            {
                sTransform = File.ReadAllText(transform);
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return;
            }

            try
            {
                var transformer  = new JTran.Transformer(sTransform, includeSource: config.IncludeRepository);
                var context      = config.ToContext(documents);

                // Single file
                if(!string.IsNullOrWhiteSpace(Path.GetExtension(source)))
                {
                    TransformFile(transformer, File.ReadAllText(source), output, context);
                    return;
                }

                sourceFiles = Directory.GetFiles(source);

                System.Console.WriteLine($"{sourceFiles.Length} source files found");

                if(sourceFiles.Length == 0)
                    return;

                foreach(var sourceFile in sourceFiles)
                {
                    TransformFile(transformer, File.ReadAllText(sourceFile), output, context);
                }
            }
            catch(Exception ex2)
            {
                System.Console.WriteLine(ex2.Message);
                return;
            }
        }

        /****************************************************************************/
        private static void TransformFile(JTran.Transformer transformer, string sourceFile, string output, TransformerContext context)
        {        
            var result =  transformer.Transform(sourceFile, context);
            var path   = output;

            if(string.IsNullOrWhiteSpace(Path.GetExtension(output)))
                path = Path.Combine(output, Path.GetFileName(sourceFile));

            File.WriteAllText(path, result);
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
        
        /****************************************************************************/
        /****************************************************************************/
        private class FileDocumentRepository : IDocumentRepository
        {
            private readonly string _path;

            /****************************************************************************/
            internal FileDocumentRepository(string path)
            {
                _path = path;
            }

            /****************************************************************************/
            public string GetDocument(string name)
            {
                var fullPath = Path.Combine(_path, name + ".json");

                return File.ReadAllText(fullPath);
            }
        }
        
        /****************************************************************************/
        /****************************************************************************/
        private class FileIncludeRepository : IDictionary<string, string>
        {
            private readonly string _path;

            /****************************************************************************/
            internal FileIncludeRepository(string path)
            {
                _path = path;
            }

            public string this[string key] 
            { 
                get { return File.ReadAllText(Path.Combine(_path, key)); }
                set { throw new NotSupportedException(); } 
             }

            public bool IsReadOnly => true;

            public bool ContainsKey(string key)
            {
                var path = Path.Combine(_path, key);

                return File.Exists(path);
            }

            #region NotSupported

            public ICollection<string> Keys => throw new NotSupportedException();

            public ICollection<string> Values => throw new NotSupportedException();

            public int Count => throw new NotSupportedException();


            public void Add(string key, string value)
            {
                throw new NotSupportedException();
            }

            public void Add(KeyValuePair<string, string> item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(KeyValuePair<string, string> item)
            {
                throw new NotSupportedException();
            }

            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                throw new NotSupportedException();
            }

            public bool Remove(string key)
            {
                throw new NotSupportedException();
            }

            public bool Remove(KeyValuePair<string, string> item)
            {
                throw new NotSupportedException();
            }

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        /****************************************************************************/
        /****************************************************************************/
        private class Config
        {
            public string               IncludePath      { get; set; }
            public List<DocumentSource> DocumentSources  { get; set; } = new List<DocumentSource>();

            public class DocumentSource
            {
                public string Name   { get; set; }
                public string Path   { get; set; }
            }

            public IDictionary<string, string> IncludeRepository => string.IsNullOrWhiteSpace(IncludePath) ? null : new FileIncludeRepository(IncludePath);

            public TransformerContext ToContext(string docPath)
            {
                if(!string.IsNullOrWhiteSpace(docPath))
                    this.DocumentSources.Add(new DocumentSource { Name = "all", Path = docPath } );

                if(this.DocumentSources.Count == 0 && string.IsNullOrWhiteSpace(docPath))
                    return null;

                var context = new TransformerContext();

                foreach(var docSource in this.DocumentSources)
                    context.DocumentRepositories[docSource.Name] = new FileDocumentRepository(docSource.Path);

                return context;
            }
        }

        #endregion
    }
}
