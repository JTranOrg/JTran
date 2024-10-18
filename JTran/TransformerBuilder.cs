/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: TransformerBuilder.cs					    		        
 *        Class(es): TransformerBuilder				         		            
 *          Purpose: Builds a transformer               
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 1 Oct 2024                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using JTran.Collections;
using JTran.Streams;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    public class TransformerBuilder
    {
        private string?                             _transform;
        private Stream?                             _stream;
        private readonly Dictionary<string, string> _includes   = new();
        private readonly List<object>               _extensions = new();
        private TransformerContext?                 _context;
        private readonly ArgumentsProvider          _arguments  = new();

        /****************************************************************************/
        private TransformerBuilder(string transform)
        { 
            _transform = transform;
        }

        /****************************************************************************/
        private TransformerBuilder(Stream stream)
        { 
            _stream = stream;
        }

        /****************************************************************************/
        public static TransformerBuilder FromString(string transform)
        {
            return new TransformerBuilder(transform);
        }

        /****************************************************************************/
        public static TransformerBuilder FromStream(Stream stream)
        {
            return new TransformerBuilder(stream);
        }

        /****************************************************************************/
        public TransformerBuilder AddExtension(object library)
        {
            _extensions.Add(library);

            return this;
        }

        /****************************************************************************/
        public TransformerBuilder AddDocumentRepository(string name, IDocumentRepository repo)
        {
            if(_context == null)
            { 
                _context = new();

                _context.DocumentRepositories = new Dictionary<string, IDocumentRepository>();
            }

            _context.DocumentRepositories.Add(name, repo);

            return this;
        }

        /****************************************************************************/
        public TransformerBuilder AddArguments(IReadOnlyDictionary<string, object> args)
        {
            _arguments.Add(args);

            return this;
        }

        /****************************************************************************/
        public TransformerBuilder AllowDeferredLoading(bool allow)
        {
            _context = _context ?? new();

            _context.AllowDeferredLoading = allow;

            return this;
        }

        /****************************************************************************/
        public TransformerBuilder OnOutputArgument(Action<string, object> action)
        {
            _context = _context ?? new();

            _context.OnOutputArgument = action;

            return this;
        }

        /****************************************************************************/
        public TransformerBuilder AddInclude(string name, string includeSource)
        {
            _includes.Add(name, includeSource);

            return this;
        }

        /****************************************************************************/
        public TransformerBuilder AddInclude(string name, Func<string> includeSource)
        {
            _includes.Add(name, includeSource());

            return this;
        }

        /****************************************************************************/
        public async Task<TransformerBuilder> AddIncludeAsync(string name, Func<Task<string>> includeSource)
        {
            _includes.Add(name, await includeSource());

            return this;
        }

        /****************************************************************************/
        public ITransformer<T> Build<T>()
        {
            if(_context == null && _arguments == null)
            { 
                if(_transform != null)
                    return new Transformer<T>(_transform, _extensions, _includes);
              
                return new Transformer<T>(_stream, _extensions, _includes);
            }

            _context = _context ?? new();

            _context.Arguments = _arguments;

            if(_transform != null)
                return new InternalTransformer<T>(_transform, _extensions, _includes, _context);

            return new InternalTransformer<T>(_stream, _extensions, _includes, _context);
        }

        #region Private

        /****************************************************************************/
        /****************************************************************************/
        private class InternalTransformer<T> : ITransformer<T>
        {
            private readonly TransformerContext _context;
            private readonly ITransformer<T> _transformer;

            /****************************************************************************/
            internal InternalTransformer(string transform, IEnumerable? extensions, IDictionary<string, string>? includes, TransformerContext context)
            {
                _transformer = new Transformer<T>(transform, extensions, includes);

                _context = context;
            }

            /****************************************************************************/
            internal InternalTransformer(Stream stream, IEnumerable? extensions, IDictionary<string, string>? includes, TransformerContext context)
            {
                _transformer = new Transformer<T>(stream, extensions, includes);

                _context = context;
            }

            internal TransformerContext Context => _context;

            #region ITransformer

            /****************************************************************************/
            public string Transform(string data, TransformerContext? context = null)
            {
                var newContext = CombinedContext(context);
                var result     = _transformer.Transform(data, newContext);

                PostTransform(newContext, context);

                return result;
            }

            /****************************************************************************/
            public void Transform(object input, Stream output, TransformerContext? context = null)
            {
                var newContext = CombinedContext(context);

                _transformer.Transform(input, output, newContext);
                PostTransform(newContext, context);
            }

            /****************************************************************************/
            public void Transform(object input, IStreamFactory output, TransformerContext? context = null)
            {
                var newContext = CombinedContext(context);

                _transformer.Transform(input, output, newContext);
                PostTransform(newContext, context);
            }

            /****************************************************************************/
            public void Transform(IEnumerable list, string? listName, Stream output, TransformerContext? context = null)
            {
                var newContext = CombinedContext(context);

                _transformer.Transform(list, listName, output, newContext);

                PostTransform(newContext, context);
            }

            #endregion

            #region Private

            /****************************************************************************/
            private void PostTransform(TransformerContext? combinedContext, TransformerContext? context)
            {
                if(context != null && _context != null)
                    context.SetOutputArguments(combinedContext);
            }

            /****************************************************************************/
            private TransformerContext? CombinedContext(TransformerContext? context)
            { 
                if (context == null) 
                    return _context;

                if(_context == null)
                    return context;

                var newContext = new TransformerContext(_context);

                // Combine arguments
                if(context.Arguments != null && context.Arguments.Any())
                { 
                    if(_context != null && _context.Arguments.Any())
                    { 
                        var args = new ArgumentsProvider();

                        args.Add(context.Arguments);
                        args.Add(_context.Arguments);

                        newContext.Arguments = args;
                    }
                    else
                        newContext.Arguments = context.Arguments;
                }
                else
                    newContext.Arguments = _context.Arguments;

                return newContext;
            }

            #endregion
        }
           
        #endregion
    }
}
