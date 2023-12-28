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
using System.Diagnostics;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using JTran.Extensions;
using JTran.Expressions;
using JTran.Json;
using JTran.Parser;

using JTranParser = JTran.Parser.Parser;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class CompiledTransform : TContainer
    {
        private readonly IDictionary<string, bool> _loadedIncludes = new Dictionary<string, bool>();
        private readonly IDictionary<string, string> _includeSource;

        /****************************************************************************/
        internal protected CompiledTransform(IDictionary<string, string> includeSource)
        {
            _includeSource = includeSource;
            this.CompiledTransform = this;
        }

        #region Compile
        
        /****************************************************************************/
        internal static CompiledTransform Compile(string source, IDictionary<string, string>? includeSource = null)
        {
            using var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(source));

            return Compile(stream, includeSource);
        }

        /****************************************************************************/
        internal static CompiledTransform Compile(Stream source, IDictionary<string, string>? includeSource = null)
        {
            var parser = new Json.Parser(new JTranModelBuilder(includeSource));
            var result = parser.Parse(source) as TContainer;

            return result as CompiledTransform;
        }

        /*****************************************************************************/
        internal void LoadInclude(string fileName)
        {
            if(_includeSource == null)
                throw new Transformer.SyntaxException($"#include: file not found: {fileName}");

            fileName = fileName.ToLower();

            if(!_loadedIncludes.ContainsKey(fileName))
            {
                if(!_includeSource.ContainsKey(fileName))   
                    throw new Transformer.SyntaxException($"#include: file not found: {fileName}");

                var include = _includeSource[fileName];

                try
                { 
                    Compile(this, include.JTranToExpando());
                }
                catch(Exception ex)
                {
                    throw new Transformer.SyntaxException($"#include: error loading file: {fileName}");
                }

                _loadedIncludes.Add(fileName, true);
            }
        }
        
        #endregion

        #region Transform 

        /****************************************************************************/
        internal void Transform(Stream input, Stream output, TransformerContext context, ExtensionFunctions extensionFunctions)
        {           
            using(var writer = new JsonStreamWriter(output))
            { 
                Transform(input, writer, context, extensionFunctions);
            }

            return;
        }            
        
        /****************************************************************************/
        internal string Transform(string data, TransformerContext context, ExtensionFunctions extensionFunctions)
        {
            using var input = new MemoryStream(UTF8Encoding.UTF8.GetBytes(data));
            var output = new JsonStringWriter();

            Transform(input, output, context, extensionFunctions);
    
            var result = output.ToString();

            return result;
        }

        /****************************************************************************/
        internal void Transform(IEnumerable list, string listName, Stream output, TransformerContext context, ExtensionFunctions extensionFunctions)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            expando[listName] = list;

            using(var writer = new JsonStreamWriter(output))
            { 
                TransformObject(expando, writer, context, extensionFunctions);
            }
    
            return;
        }

        #endregion
        
        /****************************************************************************/
        internal static IList<IExpression> ParseElementParams(string elementName, string source, IList<bool> isExplicitParam) 
        {
            var result  = new List<IExpression>();

            source = source.Trim();

            if(!source.StartsWith("#" + elementName))
                throw new Transformer.SyntaxException("Error in parsing element parameters");

            // Check for no params
            var checkStr = source.Substring(("#" + elementName).Length);

            if(checkStr.Length != 0 && !string.IsNullOrWhiteSpace(checkStr.Substring(1, checkStr.Length - 2)))
            { 
                var parser  = new JTranParser();
                var tokens  = parser.Parse(source);
                var tokens2 = Precompiler.Precompile(tokens);

                var start = 1;
                var end   = tokens2.Count;

                if(tokens2.Count > 1 && tokens2[1].Value == "(")
                {
                    ++start;
                    --end;
                }

                for(var i = start; i < end; i += 2)
                {
                    var token = tokens2[i];

                    var compiler = new Compiler();

                    if(token is ExpressionToken exprToken)
                        result.Add(compiler.InnerCompile(exprToken.Children));
                    else if(IsExplicitParam(isExplicitParam, i - 2))
                        result.Add(new Value(token));
                    else
                        result.Add(compiler.InnerCompile(new Token[] { token }));
                }
            }

            return result;
        }

        #region Private 

        /****************************************************************************/
        private void Transform(Stream data, IJsonWriter output, TransformerContext context, ExtensionFunctions extensionFunctions)
        {
            var parser  = new Json.Parser(new JsonModelBuilder());
            var expando = parser.Parse(data) as ExpandoObject;

            expando.SetParent();

            TransformObject(expando, output, context, extensionFunctions);
    
            return;
        }                

        /****************************************************************************/
        private void TransformObject(object data, IJsonWriter output, TransformerContext context, ExtensionFunctions extensionFunctions)
        {
            var newContext = new ExpressionContext(data, "__root", context, extensionFunctions, templates: this.Templates, functions: this.Functions);

            output.StartObject();
            this.Evaluate(output, newContext, f=> f());
            output.EndObject();
    
            return;
        }        

        /****************************************************************************/
        private static bool IsExplicitParam(IList<bool> isExplicitParam, int index)
        {
            if(isExplicitParam == null || isExplicitParam.Count == 0)
                return false;

            if(index >= isExplicitParam.Count)
                return isExplicitParam.Last();

            return isExplicitParam[index];
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TMessage : TToken
    {
        private IValue _message;

        /****************************************************************************/
        internal TMessage(object val)
        {
            _message = CreateValue(val);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var msg = _message.Evaluate(context);

            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TInclude : TToken
    {
        private readonly IExpression _expression;
        private readonly IValue _name;
        private readonly IDictionary<string, string> _properties;

        /****************************************************************************/
        internal TInclude(string name, string val) 
        {
            _name = name == null ? null : CreateValue(name);

            var parms = CompiledTransform.ParseElementParams("include", val, new List<bool> { false, true } );

            if(parms.Count == 0)
                throw new Transformer.SyntaxException("Missing expression for #include");

            _expression = parms[0];

            _properties = parms.Skip(1).Select( s=> s.ToString()).ToDictionary( k=> k, v=> v);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            var newScope = _expression.Evaluate(context);
            var name     = _name?.Evaluate(context)?.ToString();

            wrap( ()=>
            { 
                if(newScope is ExpandoObject expObject)
                { 
                    var properties = expObject.Where( kv=> _properties.ContainsKey(kv.Key));

                    if(properties.Count() > 0)
                    { 
                        if(name != null)
                            writer.WriteContainerName(name);

                        writer.StartObject();

                        foreach(var kv in properties)
                            writer.WriteProperty(kv.Key, kv.Value);

                        writer.EndObject();

                        return;
                    }
                }

                if(name != null)
                    writer.WriteProperty(name, null);
            });
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TExclude : TToken
    {
        private readonly IExpression _expression;
        private readonly IValue _name;
        private readonly IDictionary<string, string> _properties;

        /****************************************************************************/
        internal TExclude(string name, string val) 
        {
            _name = name == null ? null : CreateValue(name);

            var parms = CompiledTransform.ParseElementParams("exclude", val, new List<bool> { false, true } );

            if(parms.Count == 0)
                throw new Transformer.SyntaxException("Missing expression for #exclude");

            _expression = parms[0];

            _properties = parms.Skip(1).Select( s=> s.ToString()).ToDictionary( k=> k, v=> v);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            var newScope = _expression.Evaluate(context);
            var name     = _name?.Evaluate(context)?.ToString();

            wrap( ()=>
            { 
                if(newScope is ExpandoObject expObject)
                { 
                    var properties = expObject.Where( kv=> !_properties.ContainsKey(kv.Key));

                    if(properties.Count() > 0)
                    { 
                        if(name != null)
                            writer.WriteContainerName(name);

                        writer.StartObject();

                        foreach(var kv in properties)
                            writer.WriteProperty(kv.Key, kv.Value);

                        writer.EndObject();

                        return;
                    }
                }

                if(name != null)
                    writer.WriteProperty(name, null);
            });
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TBind : TContainer
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TBind(string name) 
        {
            var parms = CompiledTransform.ParseElementParams("bind", name, new List<bool> {false} );

            _expression = parms[0];
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var newScope   = _expression.Evaluate(context);
            var newContext = new ExpressionContext(data: newScope, parentContext: context, templates: this.Templates, functions: this.Functions);

            base.Evaluate(output, newContext, wrap);
        }
    }

    #region if/else

    /****************************************************************************/
    /****************************************************************************/
    internal class TIf : TContainer
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TIf(string name) : this(name, "#if(") 
        {
        }

        /****************************************************************************/
        internal protected TIf(string name, string elementName) 
        {
            name = name.Substring(elementName.Length);

            _expression = Compiler.Compile(name.Substring(0, name.Length - 1));
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(_expression.EvaluateToBool(context))
            { 
                context.PreviousCondition = true;
                base.Evaluate(output, new ExpressionContext(context.Data, context, templates: this.Templates, functions: this.Functions), wrap);
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TElseIf : TIf
    {
        /****************************************************************************/
        internal TElseIf(string name) : base(name, "#elseif(")
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
                base.Evaluate(output, context, wrap);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TElse : TContainer
    {
        /****************************************************************************/
        internal TElse(string name) 
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
                base.Evaluate(output, context, wrap);
        }
    }

    #endregion

    /****************************************************************************/
    /****************************************************************************/
    internal interface IPropertyCondition
    {
        object EvaluatedValue { get; }
        bool   If             { get; }
        void   Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap);
    }    
      
    #region map

    /****************************************************************************/
    /****************************************************************************/
    internal class TMap : TContainer, IPropertyCondition
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TMap(string name) : this(name, "#map(") 
        {
        }

        /****************************************************************************/
        public object EvaluatedValue { get; set; }
        public bool   If             => true;

        /****************************************************************************/
        internal protected TMap(string name, string elementName) 
        {
            name = name.Substring(elementName.Length);

            _expression = Compiler.Compile(name.Substring(0, name.Length - 1));
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var val = _expression.Evaluate(context);
            var newContext = new ExpressionContext(val, context);

            foreach(IPropertyCondition mapItem in this.Children)
            {
                mapItem.Evaluate(null, newContext, null);

                if(mapItem.If)
                {
                    this.EvaluatedValue = mapItem.EvaluatedValue;
                    break;
                }
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TMapItem : TProperty, IPropertyCondition
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TMapItem(string name, object val) : this(name, val, "#mapitem") 
        {
        }

        /****************************************************************************/
        internal protected TMapItem(string name, object val, string elementName) : base(name, val)  
        {
            name = name.Substring(elementName.Length);

            if(!string.IsNullOrWhiteSpace(name))
                _expression = Compiler.Compile(name.Substring(1, name.Length - 2));
        }

        public object EvaluatedValue { get; set; }
        public bool   If             { get; set; } = false;

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            this.If = false;

            if(_expression != null)
            {
                var val = _expression.Evaluate(context);

                if(val is bool bVal)
                {   
                    if(!bVal)
                        return;
                }
                else if(context.Data.CompareTo(val, out _) != 0)
                    return;
            }

            this.EvaluatedValue = this.Value.Evaluate(context);
            this.If = true;
        }
    }

    #endregion

    #region Property if/else

    /****************************************************************************/
    /****************************************************************************/
    internal class TPropertyIf : TProperty, IPropertyCondition
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TPropertyIf(string name, object val) : this(name, val, "#if(") 
        {
        }

        public object EvaluatedValue { get; set; }
        public bool   If             { get; set; } = false;

        /****************************************************************************/
        internal protected TPropertyIf(string name, object val, string elementName) : base(name, val) 
        {
            name = name.Substring(elementName.Length);

            _expression = Compiler.Compile(name.Substring(0, name.Length - 1));
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(_expression.EvaluateToBool(context))
            { 
                this.If = true;
                this.EvaluatedValue = this.Value.Evaluate(context);
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TPropertyElseIf : TPropertyIf
    {
        /****************************************************************************/
        internal TPropertyElseIf(string name, object val) : base(name, val, "#elseif(")
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
            { 
                base.Evaluate(output, context, wrap);
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TPropertyElse : TProperty, IPropertyCondition
    {
        /****************************************************************************/
        internal TPropertyElse(string name, object val)  : base(name, val)
        {
        }

        public object EvaluatedValue { get; set; }
        public bool   If             { get; set; } = false;

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
            { 
                this.If = true;
                this.EvaluatedValue = this.Value.Evaluate(context);
            }
        }
    }

    #endregion

    /****************************************************************************/
    /****************************************************************************/
    internal class TForEach : TContainer
    {
        private readonly IExpression _expression;
        private readonly IExpression _name;

        /****************************************************************************/
        internal TForEach(string name) 
        {
            var parms = CompiledTransform.ParseElementParams("foreach", name, new List<bool> {false, true} );

            if(parms.Count < 1)
                throw new Transformer.SyntaxException("Missing expression for #foreach");

            _expression = parms[0];
            _name       = parms.Count > 1 ? parms[1] : null;
        }

        /****************************************************************************/
        internal protected TForEach(string expression, bool expr) 
        {
            _expression = Compiler.Compile(expression);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var result = _expression.Evaluate(context);

            if(result == null)
                return;

            if(result is IEnumerable<object> tryList && !tryList.Any())
                return;

            var arrayName = _name == null ? null : _name.Evaluate(context)?.ToString()?.Trim();
            
            arrayName = string.IsNullOrWhiteSpace(arrayName) ? null : arrayName;

            if(arrayName != null && !(result is IEnumerable<object>))
                result = new List<object> { result };

            // If the result of the expression is an array
            if(result is IEnumerable<object> list && list.Any())
            {       
                wrap( ()=> 
                { 
                    if(arrayName != null && arrayName != "{}")
                        output.WriteContainerName(arrayName);

                    if(arrayName != null && arrayName != "{}")
                        output.StartArray();
                
                    EvaluateChildren(output, arrayName, list, context);

                    if(arrayName != null && arrayName != "{}")
                        output.EndArray();
                });
            }
            else 
            {
                // Not an array. Just treat it as a bind
                base.Evaluate(output, new ExpressionContext(result, context, templates: this.Templates, functions: this.Functions), wrap);
            }
        }

        /****************************************************************************/
        private void EvaluateChildren(IJsonWriter output, string arrayName, IEnumerable<object> list, ExpressionContext context)
        {
            try
            { 
                foreach(var childScope in list)
                { 
                    if(EvaluateChild(output, arrayName, childScope, context))
                        break;
                }
            }
            catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        /****************************************************************************/
        private bool EvaluateChild(IJsonWriter output, string arrayName, object childScope, ExpressionContext context)
        {
            var newContext = new ExpressionContext(childScope, context, templates: this.Templates, functions: this.Functions);
            var bBreak = false;

            try
            { 
                // First test if the child will ever write anything at all
                base.Evaluate(new JsonTestWriter(), newContext, (fnc)=> fnc());
                    
                // Will never write anything, move on to next child
                return false;
            }
            catch(JsonTestWriter.HaveOutput)
            {
                // Will write, continue with output below
            }

            // Clear out any variables created during test run above
            newContext.ClearVariables();

            base.Evaluate(output, newContext, (fnc)=> 
            {
                if(arrayName != null)
                    output.StartObject();

                try
                { 
                    fnc();
                }
                catch(Break)
                {
                    bBreak = true;
                }

                if(arrayName != null)
                    output.EndObject();
            }); 

            return bBreak;
        }

        /****************************************************************************/
        private void EvaluateChildren_old(IJsonWriter output, string arrayName, ICollection list, ExpressionContext context)
        {
            var bBreak = false;
            var numChildren = list.Count;

          #if DEBUG
            var index = 0;
          #endif

            foreach(var childScope in list)
            { 
                var parentArray = output.InArray && arrayName != null;
                var newContext = new ExpressionContext(childScope, context, templates: this.Templates, functions: this.Functions);

              #if DEBUG
                ++index;
              #endif

                try
                { 
                    // First test if the child will ever write anything at all
                    base.Evaluate(new JsonTestWriter(), newContext, (fnc)=> fnc());
                    
                    // Will never write anything, move on to next child
                    continue;
                }
                catch(JsonTestWriter.HaveOutput)
                {
                    // Will write, continue with output below
                }

                // Clear out any variables created during test run above
                newContext.ClearVariables();

                base.Evaluate(output, newContext, (fnc)=> 
                {
                    if(arrayName != null)
                        output.StartObject();

                    try
                    { 
                        fnc();
                    }
                    catch(Break)
                    {
                        bBreak = true;
                    }

                    if(arrayName != null)
                        output.EndObject();
                }); 

                if(bBreak)
                    break;
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TForEachGroup : TContainer
    {
        private readonly IExpression _expression;
        private readonly IExpression _name;
        private readonly IExpression _groupBy;

        /****************************************************************************/
        internal TForEachGroup(string name) 
        {
            name = name.Substring("#foreachgroup(".Length);

            var parms = name.Substring(0, name.Length - 1).Split(new char[] { ',' }); // ??? Really need to parse this

            _expression = Compiler.Compile(parms[0]);
            _groupBy    = parms.Length > 1 ? new Value(parms[1]) : null;
            _name       = parms.Length > 2 ? new Value(parms[2]) : null;
        }

        /****************************************************************************/
        internal protected TForEachGroup(string expression, bool expr) 
        {
            _expression = Compiler.Compile(expression);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var result = _expression.Evaluate(context);

            // If the result of the expression is an array
            if(!(result is IEnumerable<object> list))
            { 
                list = new List<object> { result };
            }

            // Get the groups
            var groupBy = _groupBy.Evaluate(context).ToString().Trim();

            var groups = list.GroupBy
            (
                (item)=> item.GetSingleValue(groupBy, null),
                (item)=> item,
                (groupValue, items) => { 
                                            IDictionary<string, object> item = new ExpandoObject(); 
                                                    
                                            item[groupBy] = groupValue; 
                                            item["__groupItems"] = items; 
                                                    
                                            return item as ExpandoObject; 
                                        }
            );

            var numGroups = groups.Count();

            if(numGroups == 0)
                return;

            wrap( ()=>
            { 
                // Check to see if we're outputting to an array
                if(_name != null)
                {
                    var arrayName = _name.Evaluate(context).ToString().Trim();
                
                    output.WriteContainerName(arrayName);
                    output.StartArray();
                }

                // Iterate thru the groups
                foreach(var groupScope in groups)
                {
                    var newContext = new ExpressionContext(groupScope, context, templates: this.Templates, functions: this.Functions);

                    newContext.CurrentGroup = (groupScope as dynamic).__groupItems;

                    base.Evaluate(output, newContext, (fnc)=>
                    {
                        output.StartChild();
                        output.StartObject();
                        
                        fnc();

                        output.EndObject();
                        output.EndChild();
                    });

                }

                if(_name != null)
                { 
                    output.EndArray();
                    output.WriteRaw("\r\n");
                }                    
            });
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TProperty : TToken
    {
        /****************************************************************************/
        internal TProperty(string name, object val)
        {
            this.Name  = CreateValue(name);
            this.Value = CreateValue(val);
        }

        internal IValue Name  { get; set; }
        internal IValue Value { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var name = this.Name?.Evaluate(context)?.ToString() ?? "";
            var val  = this.Value.Evaluate(context);

            wrap( ()=> output.WriteProperty(name, val));
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TVariable : TProperty
    {
        /****************************************************************************/
        internal TVariable(string name, object val) : base(name.Substring("#variable(".Length, name.Length - "#variable(".Length - 1), val)
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var name = this.Name.Evaluate(context).ToString();
            var val  = this.Value.Evaluate(context);

            context.SetVariable(name, val);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TVariableObject : TContainer
    {    
        /****************************************************************************/
        internal TVariableObject(string name) 
        {
            this.Name = name.Substring("#variable(".Length, name.Length - "#variable(".Length - 1);
        }

        internal string Name { get; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var newContext = new ExpressionContext(context.Data, context);

            if(this.Children[0] is IPropertyCondition)
            {
                if(this.Children.EvaluateConditionals(newContext, out object result))
                {
                    context.SetVariable(this.Name, result);
                }
            }
            else
            { 
                var varOutput  = new JsonStringWriter();

                varOutput.StartObject();
                base.Evaluate(varOutput, newContext, (fnc)=> fnc());
                varOutput.EndObject();

                var result = varOutput.ToString();

                context.SetVariable(this.Name, result.JsonToExpando());
            }
        }
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

    /****************************************************************************/
    /****************************************************************************/
    internal class TTemplate : TContainer
    {
        public List<string> Parameters { get; } = new List<string>();

        /****************************************************************************/
        internal TTemplate(string name) 
        {
            name = name.Substring("#template(".Length);

            var parms = name.Substring(0, name.Length - 1).Split(new char[] { ',' });

            this.Name = parms[0].ToLower().Trim();

            this.Parameters.AddRange(parms.Select( s=> s.Trim()));
            this.Parameters.RemoveAt(0);
        }

        internal string Name      { get; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var newContext = new ExpressionContext(context.Data, context);

            base.Evaluate(output, newContext, wrap);
        }    
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TFunction : TContainer
    {
        public List<string> Parameters { get; } = new List<string>();

        /****************************************************************************/
        internal TFunction(string name) 
        {
            var parms   = CompiledTransform.ParseElementParams("function", name, new List<bool> {true} );
            var context = new ExpressionContext(new {});

            this.Name = parms[0].Evaluate(context).ToString().ToLower().Trim();

            this.Parameters.AddRange(parms.Select( s=> s.Evaluate(context).ToString().Trim()));
            this.Parameters.RemoveAt(0);
        }

        internal string Name { get; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var newContext = new ExpressionContext(context.Data, context);

            base.Evaluate(output, newContext, (fnc)=> fnc());
        }    
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TCallTemplate : TContainer
    {
        private readonly string _templateName;

        /****************************************************************************/
        internal TCallTemplate(string name) 
        {
            _templateName = name.Substring("#calltemplate(".Length).ReplaceEnding(")", "");
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var template = context.GetTemplate(_templateName);

            if(template == null)
                throw new Transformer.SyntaxException($"A template with that name was not found: {_templateName}");

            var newContext = new ExpressionContext(context.Data, context);
            var paramsOutput = new JsonStringWriter();

            paramsOutput.StartObject();
            base.Evaluate(paramsOutput, newContext, (fnc)=> fnc()); 
            paramsOutput.EndObject();

            var jsonParams = paramsOutput.ToString().JTranToExpando();

            foreach(var paramName in template.Parameters)
                newContext.SetVariable(paramName, (jsonParams as IDictionary<string, object>)[paramName].ToString());

            template.Evaluate(output, newContext, wrap);
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
        private string _value;

        /****************************************************************************/
        internal SimpleValue(object value)
        {
            _value = value.ToString();
        }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            return _value;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class StringValue
    {
        private readonly string _val;

        internal StringValue(string val)
        {
            _val = val;
        }

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
            if(Math.Floor(_value) == _value)
                return Convert.ToInt64(_value);

            return _value;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class ExpressionValue : IValue 
    {
        private IExpression _expression;

        /****************************************************************************/
        internal ExpressionValue(string value)
        {
            _expression = Compiler.Compile(value);
        }

        /****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            var val = _expression?.Evaluate(context);

            if(val == null)
                return null;

            if(!(val is StringValue) && decimal.TryParse(val.ToString(), out decimal dval))
            {
                if(Math.Floor(dval) == dval)
                    return Convert.ToInt64(dval);

                return dval;
            }

            return val;
        }
    }

    #endregion
}
