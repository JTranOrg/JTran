/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: CompiledTransform.cs					    		        
 *        Class(es): CompiledTransform			         		            
 *          Purpose: Compiles and evaluates tranformations                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using JTran.Expressions;
using JTran.Json;
using JTran.Common;
using JTran.Parser;

using JTranParser = JTran.Parser.ExpressionParser;
using System.Diagnostics;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class CompiledTransform : TContainer
    {
        private readonly Dictionary<string, CompiledTransform> _loadedIncludes = new();
        private readonly IDictionary<string, string>? _includeSource;

        internal static IReadOnlyList<bool> SingleFalse     { get; } = new List<bool>() { false };
        internal static IReadOnlyList<bool> SingleTrue      { get; } = new List<bool>() { true };
        internal static IReadOnlyList<bool> FalseTrue       { get; } = new List<bool>() { false, true };
        internal static IReadOnlyList<bool> FalseFalseTrue  { get; } = new List<bool>() { false, false, true };
        internal static IReadOnlyList<bool> TrueFalse       { get; } = new List<bool>() { true, false};

        private bool _outputArray = false;

        /****************************************************************************/
        internal protected CompiledTransform(IDictionary<string, string>? includeSource)
        {
            _includeSource = includeSource;
            this.CompiledTransform = this;
        }

        internal string Name { get; set; } = "_root";

        #region Compile
        
        /****************************************************************************/
        internal static CompiledTransform Compile(string source, IDictionary<string, string>? includeSource = null, bool isInclude = false, TContainer? parent = null)
        {
            using var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(source));

            return Compile(stream, includeSource, isInclude, parent);
        }

        /****************************************************************************/
        internal static CompiledTransform Compile(Stream source, IDictionary<string, string>? includeSource = null, bool isInclude = false, TContainer? parent = null)
        {
            using var parser = new Json.Parser(new JTranModelBuilder(includeSource, isInclude, parent));
            var result = parser.Parse(source, false) as CompiledTransform;

            return result!;
        }

        /*****************************************************************************/
        internal void LoadInclude(string fileName, TContainer? parent, long lineNumber)
        {
            if(_includeSource == null)
                throw new Transformer.SyntaxException($"#include: file not found: {fileName}") { LineNumber = lineNumber};

            fileName = fileName.ToLower();

            if(!_loadedIncludes.ContainsKey(fileName))
            {
                if(!_includeSource.ContainsKey(fileName))   
                    throw new Transformer.SyntaxException($"#include: file not found: {fileName}") { LineNumber = lineNumber};

                var include = _includeSource[fileName];

                try
                { 
                    var newTransform = Compile(include, _includeSource, true, parent);

                    newTransform.Name = fileName;

                     _loadedIncludes.Add(fileName, newTransform);
                }
                catch(Exception ex)
                {
                    throw new Transformer.SyntaxException($"#include: error loading file: {fileName}", ex) { LineNumber = lineNumber};
                }
            }

            // Get the compiled include file
            var includedTransform = _loadedIncludes[fileName];

            // Add the included file's children to this object's child list
            this.Children.AddRange(includedTransform.Children);

            return;
        }

        internal override TToken CreateObject(ICharacterSpan name, object? previous, long lineNumber)
        {
            var result = base.CreateObject(name, previous, lineNumber);

            if(result is TBaseArray array && array.IsOutputArray)
                _outputArray = true;

            return result;
        }

        #endregion

        #region Transform 

        /****************************************************************************/
        internal void Transform(Stream input, Stream output, TransformerContext? context, ExtensionFunctions? extensionFunctions)
        {           
            using(var writer = new JsonStreamWriter(output))
            { 
                Transform(input, writer, context, extensionFunctions);
            }

            return;
        }            
        
        /****************************************************************************/
        internal string Transform(string data, TransformerContext? context, ExtensionFunctions? extensionFunctions)
        {
            using var input = new MemoryStream(UTF8Encoding.UTF8.GetBytes(data));
            var output = new JsonStringWriter();

            Transform(input, output, context, extensionFunctions);
    
            var result = output.ToString();

            return result;
        }

        /****************************************************************************/
        internal void Transform(IEnumerable list, string? listName, Stream output, TransformerContext? context, ExtensionFunctions? extensionFunctions)
        {
            object data = list;

            if(listName != null)
            { 
                var jobj = new JsonObject(null); 

                jobj[CharacterSpan.FromString(listName)] = list;
                data = jobj;
            }

            using(var writer = new JsonStreamWriter(output))
            { 
                TransformObject(data, writer, context, extensionFunctions);
            }
    
            return;
        }

        /****************************************************************************/
        internal void Transform(IEnumerable list, Stream output, TransformerContext? context, ExtensionFunctions? extensionFunctions)
        {
            Transform(list, null, output, context, extensionFunctions);
        }

        #endregion
        
        /****************************************************************************/
        internal static IList<IExpression> ParseElementParams(string elementName, ICharacterSpan source, IReadOnlyList<bool> isExplicitParam) 
        {
            if(!source.StartsWith(elementName))
                throw new Transformer.SyntaxException("Error in parsing element parameters");

            var result  = new List<IExpression>();

            // Check for no params
            var checkStr = source.Substring(elementName.Length);

            if(!checkStr.IsNullOrWhiteSpace(1, checkStr.Length - 2))
            { 
                var compiler = new Compiler();
                var token    = Precompile(checkStr);
                var i = 0;
                IEnumerable<Token> tokens = token;

                if(token.Type != Token.TokenType.CommaDelimited)
                    tokens = new [] { token };
                
                foreach(var child in tokens)
                {
                    if(IsExplicitParam(isExplicitParam, i))
                        result.Add(new Value(child));
                    else
                        result.Add(compiler.InnerCompile(new Token[] { child }));

                    ++i;
                }
            }

            return result;
        }
        
        #region Private 

        /****************************************************************************/
        private static Token Precompile(ICharacterSpan source) 
        {
            var parser = new JTranParser();
            var tokens = parser.Parse(source);

            return Precompiler.Precompile(tokens);
        }

        /****************************************************************************/
        private void Transform(Stream data, IJsonWriter output, TransformerContext? context, ExtensionFunctions? extensionFunctions)
        {
            using var parser = new Json.Parser(new JsonModelBuilder());
            var result = parser.Parse(data, context?.AllowDeferredLoading ?? true);

            TransformObject(result, output, context, extensionFunctions);
    
            return;
        }                

        /****************************************************************************/
        private void TransformObject(object data, IJsonWriter output, TransformerContext? context, ExtensionFunctions? extensionFunctions)
        {
            var newContext = new ExpressionContext(data, "__root", context, extensionFunctions, templates: this.Templates, functions: this.Functions);

            if(!_outputArray)
                output.StartObject();

                var t = data.GetType().FullName;

            this.Evaluate(output, newContext, f=> f());

            if(!_outputArray)
                output.EndObject();
    
            return;
        }        

        /****************************************************************************/
        private static bool IsExplicitParam(IReadOnlyList<bool> isExplicitParam, int index)
        {
            if(isExplicitParam == null || !isExplicitParam.Any())
                return false;

            if(index >= isExplicitParam.Count)
                return isExplicitParam.Last();

            return isExplicitParam[index];
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class IncludedTransform : CompiledTransform
    {
        /****************************************************************************/
        internal IncludedTransform(IDictionary<string, string>? includeSource) : base(includeSource)
        {
        }

        /****************************************************************************/
        internal protected override TTemplate AddTemplate(TTemplate template)
        {
            TContainer parent = this;

            while(parent is IncludedTransform)
                parent = parent.Parent!;

            parent.Templates!.Add(template.Name, template);

            return template;
        }

        /****************************************************************************/
        internal protected override TFunction AddFunction(TFunction function)
        {
            TContainer parent = this;

            while(parent is IncludedTransform)
                parent = parent.Parent!;

            parent.Functions!.Add(function.Name, function);

            return function;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal interface IPropertyCondition
    {
        object EvaluatedValue { get; }
        bool   If             { get; }
        void   Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap);
    }    

    /****************************************************************************/
    /****************************************************************************/
    internal static class TokenExtensions
    {
        internal static bool EvaluateConditionals(this IList<TToken> tokens, ExpressionContext context, out object output)
        {
            foreach(IPropertyCondition child in tokens)
            {
                child.Evaluate(null, context, null);

                if(child.If)
                {
                    output = child.EvaluatedValue;
                    return true;
                }
            }

            output = null;
            return false;
        }
    }

    #region Values

    /****************************************************************************/
    /****************************************************************************/
    internal interface IValue
    {
        object Evaluate(ExpressionContext context);
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class SimpleValue : IValue
    {
        private object? _value;

        /****************************************************************************/
        internal SimpleValue(object? value)
        {
            _value = value;
        }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            return _value;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IStringValue
    {
        object? Value { get; }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class StringValue : IStringValue
    {
        private readonly string _val;

        internal StringValue(string val)
        {
            _val = val;
        }

        public object? Value => _val;

        public override string ToString()
        {
            return _val;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class NumberValue : IValue
    {
        private decimal _value;

        /****************************************************************************/
        internal NumberValue(decimal val)
        {
            _value = val;
        }

        /****************************************************************************/       
        public object Evaluate(ExpressionContext context)
        {
            return _value;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class ExpressionValue : IValue 
    {
        private IExpression _expression;

        /****************************************************************************/
        internal ExpressionValue(ICharacterSpan value)
        {
            _expression = Compiler.Compile(value);
        }

        /****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            return _expression?.Evaluate(context);
        }
    }

    #endregion
}
