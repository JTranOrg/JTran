/***************************************************************************
 *                                                                          
 *    JTran.Project - Consolidates all of the attributes necessary to do a transform  							                    
 *                                                                          
 *        Namespace: JTran.Project							            
 *             File: CompiledProject.cs					    		        
 *        Class(es): CompiledProject			         		            
 *          Purpose: A compiled project                 
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 11 Apr 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using JTran.Collections;
using JTran.Streams;

namespace JTran.Project
{
    /****************************************************************************/
    /****************************************************************************/
    public class CompiledProject
    { 
        public string                              Name               { get; set; } = "";
        public string                              Transform          { get; set; } = "";
        public string                              SourcePath         { get; set; } = "";
        public string                              Destinations       { get; set; } = "";
        public bool                                SplitOutput        { get; set; } = false;
        public Dictionary<string, string>?         Includes           { get; set; }
        public Dictionary<string, string>?         Documents          { get; set; }
        public List<object>                        Extensions         { get; set; } = new();
                                                   
        private Dictionary<string, object>?        Arguments          { get; set; }
        private List<IReadOnlyDictionary<string, object>>  ArgumentProviders  { get; set; } = [];

        /****************************************************************************/
        public static async Task<CompiledProject> Load(Project project, Action<Exception> onError)
        {
            var compiled = new CompiledProject();
            var task     = File.ReadAllTextAsync(project.TransformPath);

            compiled.Name               = project.Name;
            compiled.SourcePath         = project.SourcePath;
            compiled.Destinations       = project.DestinationPath;
                                        
            compiled.Documents          = project.DocumentPaths;
            compiled.Includes           = LoadFiles(project.IncludePaths, onError, true);
            compiled.Arguments          = project.Arguments;
            compiled.ArgumentProviders  = project.ArgumentProviders;
            compiled.SplitOutput        = project.SplitOutput;

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

            await Task.WhenAll(task);

            compiled.Transform = task.Result;

            return compiled;
        }

        /****************************************************************************/
        public void Run(Stream output, IReadOnlyDictionary<string, object>? args = null)
        {
             var transformer = new Transformer(this.Transform, this.Extensions, this.Includes);
             var context     = CreateContext(args);

             using var source = File.OpenRead(this.SourcePath);

             transformer.Transform(source, output, context);
        }

        /****************************************************************************/
        public Task Run(IReadOnlyDictionary<string, object>? args = null)
        {
            var transformer = new Transformer(this.Transform, this.Extensions, this.Includes);
            var args2       = new Dictionary<string, object> { { "DestinationPath", this.Destinations } };
            var context     = CreateContext(args, args2);

            using var source = File.OpenRead(this.SourcePath);

            if(this.SplitOutput)
            { 
                var output = new DeferredFileStreamFactory(this.Destinations); 

                context.OnOutputArgument = (key, value)=>
                {
                    if(key == "FileName" && value != null)
                    { 
                       output.CurrentStream!.FileName = value.ToString();
                    }
                };
    
                transformer.Transform(source, output, context);
            }
            else
            {
                using var output = new DeferredFileStream(this.Destinations);

                context.OnOutputArgument = (key, value)=>
                {
                    if(key == "FileName")
                    { 
                       output.FileName = value.ToString();
                   }
                };

                transformer.Transform(source, output, context);
            }

            return Task.CompletedTask;
        }

        /****************************************************************************/
        public Task TransformFile(string sourcePath, string destinationPath, IReadOnlyDictionary<string, object>? args, Action<string> onSuccess, Action<string> onError)
        {
             return TransformFiles(new string[] { sourcePath }, new string[] { destinationPath }, args, onSuccess, onError);
        }

        /****************************************************************************/
        public async Task TransformFiles(string[] sourcePaths, string[] destinationPaths, IReadOnlyDictionary<string, object>? args, Action<string> onSuccess, Action<string> onError, bool serially = false)
        {
             var transformer = new Transformer(this.Transform, this.Extensions, this.Includes);
             var tasks       = new List<Task>();

             for(var i = 0; i < sourcePaths.Length; ++i)
             { 
                var args2 = new Dictionary<string, object>();

                args2.Add("SourceIndex", i+1);
                args2.Add("DestinationPath", destinationPaths[i]);

                if(serially)
                    TransformFile(transformer, sourcePaths[i], destinationPaths[i], args, args2, onSuccess, onError);
                else
                    tasks.Add(TransformFileAsync(transformer, sourcePaths[i], destinationPaths[i], args, args2, onSuccess, onError));
             }

             if(!serially)
                await Task.WhenAll(tasks);
        }

        #region Private

        /****************************************************************************/
        private Task TransformFileAsync(Transformer transformer, string sourcePath, string destinationPath, IReadOnlyDictionary<string, object>? args, IReadOnlyDictionary<string, object>? args2, Action<string> onSuccess, Action<string> onError)
        { 
            return Task.Run( ()=> 
            { 
                try
                { 
                    using var output = new DeferredFileStream(destinationPath);
                    using var source = File.OpenRead(sourcePath);
                    var context      = CreateContext(args, args2, (name, value)=> 
                    {
                        if(name == "FileName")
                            output.FileName = value.ToString();
                    });

                    transformer.Transform(source, output, context);

                    onSuccess($"Transforming file {sourcePath} to {output.FileName}");
                }
                catch (Exception ex) 
                { 
                    onError(ex.Message);
                }
            });
        }

        /****************************************************************************/
        private void TransformFile(Transformer transformer, string sourcePath, string destinationPath, IReadOnlyDictionary<string, object>? args, IReadOnlyDictionary<string, object>? args2, Action<string> onSuccess, Action<string> onError)
        { 
            try
            { 
                using var output = new DeferredFileStream(destinationPath);
                using var source = File.OpenRead(sourcePath);
                var context      = CreateContext(args, args2, (name, value)=> 
                {
                    if(name == "FileName")
                        output.FileName = value.ToString();
                });

                transformer.Transform(source, output, context);

                onSuccess($"Transforming file {sourcePath} to {output.FileName}");
            }
            catch (Exception ex) 
            { 
                onError(ex.Message);
            }
        }

        /****************************************************************************/
        private TransformerContext CreateContext(IReadOnlyDictionary<string, object>? args, IReadOnlyDictionary<string, object>? args2 = null, Action<string, object>? onOutputVariable = null)
        { 
            IDictionary<string, IDocumentRepository>? docRepositories = null;

            if(this.Documents != null && this.Documents.Any())
            {
                docRepositories = new Dictionary<string, IDocumentRepository>();

                foreach(var kv in this.Documents) 
                {
                    docRepositories.Add(kv.Key, new FileDocumentRepository(kv.Value));
                }
            }

            var providers = new ArgumentsProvider();

            // These will be evaluated first
            if(args != null && args.Any()) 
                providers.Add(args!);

            if(args2 != null && args2.Any()) 
                providers.Add(args2!);

            // These will be evaluated second
            if(this.Arguments != null && this.Arguments.Any()) 
                providers.Add(this.Arguments!);

            // These will be evaluated last
            if(this.ArgumentProviders != null && this.ArgumentProviders.Any()) 
                foreach(var providerArgs in this.ArgumentProviders)
                    providers.Add(providerArgs);

            return new TransformerContext { DocumentRepositories = docRepositories, Arguments = providers, OnOutputArgument = onOutputVariable };
        }

        /****************************************************************************/
        private static Dictionary<string, string> LoadFiles(Dictionary<string, string> paths, Action<Exception> onError, bool loadWithExtension = false)
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
                        var data     = File.ReadAllText(file);
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        var key      = string.IsNullOrWhiteSpace(kv.Key) ? fileName : $"{kv.Key}.{fileName}";

                        result.Add(key, data);

                        if(loadWithExtension)
                        { 
                            fileName = Path.GetFileName(file);
                            key      = string.IsNullOrWhiteSpace(kv.Key) ? fileName : $"{kv.Key}.{fileName}";

                            result.Add(key, data);
                        }
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
