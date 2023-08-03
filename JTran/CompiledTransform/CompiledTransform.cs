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
 *   Copyright (c) 2020-2022 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                                                                                                 
 *    http://www.opensource.org/licenses/mit-license.php                    
 * 
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using Newtonsoft.Json.Linq;

using JTran.Extensions;
using JTran.Expressions;
using JTran.Parser;

using JTranParser = JTran.Parser.Parser;

using System.Diagnostics;

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
        }

        /****************************************************************************/
        internal void Transform(Stream input, Stream output, TransformerContext context, ExtensionFunctions extensionFunctions)
        {
            var data = input.ReadString();
            
            using(var writer = new JsonStreamWriter(output))
            { 
                Transform(data, writer, context, extensionFunctions);
            }

            return;
        }            
        
        /****************************************************************************/
        internal string Transform(string data, TransformerContext context, ExtensionFunctions extensionFunctions)
        {
            var output = new JsonStringWriter();

            Transform(data, output, context, extensionFunctions);
    
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

        /****************************************************************************/
        private void Transform(string data, IJsonWriter output, TransformerContext context, ExtensionFunctions extensionFunctions)
        {
            var expando = data.JsonToExpando();

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
        internal static CompiledTransform Compile(string source, IDictionary<string, string> includeSource = null)
        {
            var transform = new CompiledTransform(includeSource);

            transform.Compile(transform, JObject.Parse(source));

            return transform;
        }

        /*****************************************************************************/
        internal void LoadInclude(string fileName)
        {
            if(_includeSource != null)
            { 
                fileName = fileName.ToLower();

                if(!_loadedIncludes.ContainsKey(fileName))
                {
                    if(!_includeSource.ContainsKey(fileName))   
                        throw new Transformer.SyntaxException($"include file not found: {fileName}");

                    var include = _includeSource[fileName];

                    Compile(this, JObject.Parse(include));

                    _loadedIncludes.Add(fileName, true);
                }
            }
        }

        /****************************************************************************/
        internal static IList<IExpression> ParseElementParams(string elementName, string source, IList<bool> isExplicitParam) 
        {
            var result = new List<IExpression>();
            var parser = new JTranParser();
            var tokens = parser.Parse(source);
            var tokens2 = Precompiler.Precompile(tokens);

            if(tokens2[0].Value != ("#" + elementName))
                throw new Transformer.SyntaxException("Error in parsing element parameters");

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

            return result;
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
    }

    /****************************************************************************/
    /****************************************************************************/
    internal abstract class TToken
    {
        public abstract void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap);

        /****************************************************************************/
        internal protected IValue CreateValue(JToken value)
        {
            return CreateValue(value.ToString());
        }   

        /****************************************************************************/
        private IValue CreateValue(string sval)
        {
            if(!sval.StartsWith("#("))
            { 
                if(decimal.TryParse(sval, out decimal val))
                    return new NumberValue(val);

                return new SimpleValue(sval);
            }

            if(!sval.EndsWith(")"))
                throw new Transformer.SyntaxException("Missing closing parenthesis");

            var expr = sval.Substring(2, sval.Length - 3);

            return new ExpressionValue(expr);
        }   
     }

    /****************************************************************************/
    /****************************************************************************/
    internal class TContainer : TToken
    {
        internal List<TToken>                   Children    { get; }
        internal IDictionary<string, TTemplate> Templates   { get; }
        internal IDictionary<string, TFunction> Functions   { get; }

        private TToken _previous;

        /****************************************************************************/
        internal TContainer()
        {
            this.Children  = new List<TToken>();
            this.Templates = new Dictionary<string, TTemplate>();
            this.Functions = new Dictionary<string, TFunction>();
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var numChildren = this.Children.Count;

            wrap( ()=>
            {
                foreach(var child in this.Children)
                { 
                    child.Evaluate(output, context, (cb)=>
                    {                  
                        cb();
                    });
                }
            });
        }

        /****************************************************************************/
        internal protected void Compile(CompiledTransform transform, JObject source, TContainer parent = null)
        {   
            foreach(var child in source)
            {
                var newToken = CreateToken(transform, child.Key, child.Value, parent);

                if(newToken != null)
                    this.Children.Add(newToken);

                _previous = newToken;
            }
        }

        /****************************************************************************/
        private TToken CreateToken(CompiledTransform transform, string name, JToken child, TContainer parent)
        {   
            var t = child.GetType().ToString();

            if(child is JValue val)
            { 
                if(name.StartsWith("#include"))
                { 
                    transform.LoadInclude(val.Value.ToString());
                    return null;
                }

                if(name.StartsWith("#break"))
                    return new TBreak();

                if(name.StartsWith("#variable"))
                    return new TVariable(name, val);

                if(name.StartsWith("#message"))
                    return new TMessage(val);

                if(name.StartsWith("#throw"))
                    return new TThrow(name, val);

                var sval = val.ToString();

                if(name.StartsWith("#mapitem"))
                {                
                    if(parent is TMap && (_previous is TMapItem || _previous == null))
                        return new TMapItem(name, val);

                    throw new Transformer.SyntaxException("#mapitem must be a child of #map");
                }

                if(name.StartsWith("#arrayitem"))
                    return new TSimpleArrayItem(val);

                if(sval.StartsWith("#copyof"))
                { 
                    if(name.StartsWith("#noobject"))
                        return new TCopyOf(null, sval);

                    return new TCopyOf(name, sval);
                }

                if(name.StartsWith("#if"))
                    return new TPropertyIf(name, val);

                if(name.StartsWith("#elseif"))
                { 
                    if(_previous is TPropertyIf || _previous is TElseIf)
                        return new TPropertyElseIf(name, val);

                    throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");
                }

                if(name.StartsWith("#else"))
                { 
                    if(_previous is TPropertyIf || _previous is TPropertyElseIf)
                        return new TPropertyElse(name, val);

                    throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");
                }                
                
                return new TProperty(name, val);
            }

            if(child is JObject obj)
            { 
                if(name.StartsWith("#template"))
                { 
                    var template = new TTemplate(name, obj);

                    this.Templates.Add(template.Name, template);
                    return null;
                }

                if(name.StartsWith("#function"))
                { 
                    var function = new TFunction(name, obj);

                    this.Functions.Add(function.Name, function);
                    return null;
                }

                if(name.StartsWith("#variable"))
                    return new TVariableObject(name, obj);

                if(name.StartsWith("#map("))
                    return new TMap(name, obj);

                 if(name.StartsWith("#calltemplate"))
                    return new TCallTemplate(name, obj);

                if(name.StartsWith("#bind"))
                    return new TBind(name, obj);

                if(name.StartsWith("#foreachgroup"))
                    return new TForEachGroup(name, obj);

                if(name.StartsWith("#foreach"))
                    return new TForEach(name, obj);

                if(name.StartsWith("#arrayitem"))
                    return new TArrayItem(name, obj);

                if(name.StartsWith("#array"))
                    return new TArray(name, obj);

                if(name.StartsWith("#try"))
                    return new TTry(obj);

                if(name.StartsWith("#catch"))
                { 
                    if(_previous is TTry || _previous is TCatch)
                        return new TCatch(name, obj);

                    throw new Transformer.SyntaxException("#catch must follow a #try or another #catch");
                }

                if(name.StartsWith("#if"))
                    return new TIf(name, obj);

                if(name.StartsWith("#elseif"))
                { 
                    if(_previous is TIf || _previous is TElseIf)
                        return new TElseIf(name, obj);

                    throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");
                }

                if(name.StartsWith("#else"))
                { 
                    if(_previous is TIf || _previous is TElseIf)
                        return new TElse(name, obj);

                    throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");
                }

                return new TObject(name, obj);
            }
            
            if(child is JArray array)
                return new TExplicitArray(name, array);

            return null;
        }    
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TArrayItem : TObject
    {
        /****************************************************************************/
        internal TArrayItem(string name, JObject val) : base(name, val)
        {
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TObject : TContainer
    {
        /****************************************************************************/
        internal TObject(string name, JObject val)
        {
            this.Name = CreateValue(name);

            Compile(null, val);
        }

        internal IValue Name  { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var name = this.Name.Evaluate(context)?.ToString();

           if(output.InObject && string.IsNullOrWhiteSpace(name))
               throw new Transformer.SyntaxException("Property name evaluates to null or empty string");

            if(this.Children.Count != 0 && this.Children[0] is IPropertyCondition)
            {
                if(this.Children.EvaluateConditionals(context, out object result))
                {
                    wrap( ()=> output.WriteProperty(name, result));
                }
                else
                {
                    wrap( ()=> output.WriteProperty(name, null));
                }
            }
            else
            { 
                wrap( ()=>
                { 
                    base.Evaluate(output, context, f=> 
                    {
                        if(output.InObject)
                        { 
                            output.WriteContainerName(name);
                        }

                        output.StartObject();
                        f();
                        output.EndObject();
                    });
                });
            }
        }
    }
    
    /****************************************************************************/
    /****************************************************************************/
    internal class TMessage : TToken
    {
        private IValue _message;

        /****************************************************************************/
        internal TMessage(JValue val)
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

    #region Control

    /****************************************************************************/
    /****************************************************************************/
    internal class TThrow: TToken
    {
        private IExpression _code;
        private IValue _message;

        /****************************************************************************/
        internal TThrow(string name, JValue val)
        {
            var parms = CompiledTransform.ParseElementParams("throw", name, new List<bool> {false, true} );

            _code = parms.Count > 0 ? parms[0] : null;

            _message = CreateValue(val);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var msg = _message.Evaluate(context);

            if(_code != null)
                throw new Transformer.UserError(_code.Evaluate(context).ToString(), msg.ToString());

            throw new Transformer.UserError(msg.ToString());
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TBreak : TToken
    {
        /****************************************************************************/
        internal TBreak()
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            throw new Break();
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class Break : Exception
    { 
        internal Break() { }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TTry : TContainer
    {
        /****************************************************************************/
        internal TTry(JObject val) 
        {
            // Compile children
            Compile(null, val);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            try
            { 
                var newOutput = new JsonStringWriter();

                base.Evaluate(newOutput, context, (fnc)=> fnc());

                wrap( ()=> output.WriteRaw(newOutput.ToString()));

                context.PreviousCondition = true;
            }
            catch(Transformer.UserError ex)
            {
                context.UserError = ex;

                // Do nothing
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TCatch : TContainer
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TCatch(string name, JObject val) 
        {
            var parms = CompiledTransform.ParseElementParams("catch", name, new List<bool> {false, true} );

            _expression = parms.Count > 0 ? parms[0] : null;

            // Compile children
            Compile(null, val);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
            { 
                if(_expression == null || _expression.EvaluateToBool(context))
                { 
                    base.Evaluate(output, context, wrap);
                    context.PreviousCondition = true;
                }
            }
        }
    }

    #endregion

    #region Arrays

    /****************************************************************************/
    /****************************************************************************/
    internal class TExplicitArray : TContainer
    {
        /****************************************************************************/
        internal TExplicitArray(string name, JArray array)
        {
            this.Name = CreateValue(name);

            foreach(var item in array.Children())
            {
                if(item is JValue val)
                   this.Children.Add(new TSimpleArrayItem(val));
                else if(item is JObject obj)
                   this.Children.Add(new TObject(null, obj)); 
            }
        }

        internal IValue Name  { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            base.Evaluate(output, context, (fnc)=>
            {
                wrap( ()=>
                { 
                    if(output.InObject)
                    { 
                        var name = this.Name.Evaluate(context)?.ToString();

                        if(string.IsNullOrWhiteSpace(name))
                            throw new Transformer.SyntaxException("Array name evaluates to null or empty string");
            
                        output.WriteContainerName(name);
                    }

                    output.StartArray();
                    fnc();
                    output.EndArray();
                });
            });
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TSimpleArrayItem : TToken
    {
        private readonly IValue _val;

        /****************************************************************************/
        internal TSimpleArrayItem(JValue val) 
        {
            _val = CreateValue(val);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var value = _val.Evaluate(context);

            if(value is IEnumerable<object> list)
            {
                var numItems = list.Count();

                if(numItems > 0)
                { 
                    output.StartChild(); 

                    wrap( ()=> output.WriteList(list));
                    output.EndChild();
                }
            }
            else
                wrap( ()=> output.WriteSimpleArrayItem(value));
        }
    }    

    /****************************************************************************/
    /****************************************************************************/
    internal class TArray : TContainer
    {
        internal TArray(string name, JObject val)
        { 
           name = name.Substring("#array(".Length, name.Length - "#array(".Length - 1);

           this.Name = CreateValue(name);

            Compile(null, val);
        }

        internal IValue Name  { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            base.Evaluate(output, context, (fnc)=>
            {
                wrap( ()=>
                { 
                    output.WriteContainerName(this.Name.Evaluate(context).ToString());
                    output.StartArray();

                    fnc();

                    output.EndArray();
                });
            });
        }
    }

    #endregion

    /****************************************************************************/
    /****************************************************************************/
    internal class TCopyOf : TToken
    {
        private readonly IExpression _expression;
        private readonly IValue _name;

        /****************************************************************************/
        internal TCopyOf(string name, string val) 
        {
            _name = name == null ? null : CreateValue(name);

            var parms = CompiledTransform.ParseElementParams("copyof", val, new List<bool> {false} );

            if(parms.Count == 0)
                throw new Transformer.SyntaxException("Missing expression for #copyof");

            _expression = parms[0];
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            var newScope = _expression.Evaluate(context);
            var name     = _name?.Evaluate(context)?.ToString();

            wrap( ()=>
            { 
                if(name != null)
                { 
                    writer.WriteContainerName(name);

                    if(newScope is ExpandoObject expObject)
                    { 
                        writer.WriteItem(expObject);
                    }
                    else if(newScope is IEnumerable<object> list)
                    { 
                        writer.WriteList(list);
                    }
                }
                else
                {
                    writer.WriteProperties(newScope);
                }
            });
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TBind : TContainer
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TBind(string name, JObject val) 
        {
            var parms = CompiledTransform.ParseElementParams("bind", name, new List<bool> {false} );

            _expression = parms[0];

            // Compile children
            Compile(null, val);
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
        internal TIf(string name, JObject val) : this(name, val, "#if(") 
        {
        }

        /****************************************************************************/
        internal protected TIf(string name, JObject val, string elementName) 
        {
            name = name.Substring(elementName.Length);

            _expression = Compiler.Compile(name.Substring(0, name.Length - 1));

            // Compile children
            Compile(null, val);
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
        internal TElseIf(string name, JObject val) : base(name, val, "#elseif(")
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
        internal TElse(string name, JObject val) 
        {
            // Compile children
            Compile(null, val);
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
        internal TMap(string name, JObject val) : this(name, val, "#map(") 
        {
            // Compile children
            Compile(null, val, this);
        }

        public object EvaluatedValue { get; set; }
        public bool   If             => true;

        /****************************************************************************/
        internal protected TMap(string name, JObject val, string elementName) 
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
        internal TMapItem(string name, JValue val) : this(name, val, "#mapitem") 
        {
        }

        /****************************************************************************/
        internal protected TMapItem(string name, JValue val, string elementName) : base(name, val)  
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
        internal TPropertyIf(string name, JValue val) : this(name, val, "#if(") 
        {
        }

        public object EvaluatedValue { get; set; }
        public bool   If             { get; set; } = false;

        /****************************************************************************/
        internal protected TPropertyIf(string name, JValue val, string elementName) : base(name, val) 
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
        internal TPropertyElseIf(string name, JValue val) : base(name, val, "#elseif(")
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
        internal TPropertyElse(string name, JValue val)  : base(name, val)
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
        internal TForEach(string name, JObject val) 
        {
            var parms = CompiledTransform.ParseElementParams("foreach", name, new List<bool> {false, true} );

            if(parms.Count < 1)
                throw new Transformer.SyntaxException("Missing expression for #foreach");

            _expression = parms[0];
            _name       = parms.Count > 1 ? parms[1] : null;

            // Compile children
            Compile(null, val);
        }

        /****************************************************************************/
        internal protected TForEach(string expression) 
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
        internal TForEachGroup(string name, JObject val) 
        {
            name = name.Substring("#foreachgroup(".Length);

            var parms = name.Substring(0, name.Length - 1).Split(new char[] { ',' }); // ??? Really need to parse this

            _expression = Compiler.Compile(parms[0]);
            _groupBy    = parms.Length > 1 ? new Value(parms[1]) : null;
            _name       = parms.Length > 2 ? new Value(parms[2]) : null;

            // Compile children
            Compile(null, val);
        }

        /****************************************************************************/
        internal protected TForEachGroup(string expression) 
        {
            _expression = Compiler.Compile(expression);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var result = _expression.Evaluate(context);

            // If the result of the expression is an array
            if(result is IEnumerable<object> list)
            { 
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
            else 
            {
                // Not an array. Just treat it as a bind
                base.Evaluate(output, new ExpressionContext(result, context, templates: this.Templates, functions: this.Functions), wrap);
            }
        }
   }

    /****************************************************************************/
    /****************************************************************************/
    internal class TProperty : TToken
    {
        /****************************************************************************/
        internal TProperty(string name, JValue val)
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
        internal TVariable(string name, JValue val) : base(name.Substring("#variable(".Length, name.Length - "#variable(".Length - 1), val)
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
        internal TVariableObject(string name, JObject val) 
        {
            this.Name = name.Substring("#variable(".Length, name.Length - "#variable(".Length - 1);

            // Compile children
            Compile(null, val);
        }

        internal string Name      { get; }

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
        internal TTemplate(string name, JObject val) 
        {
            name = name.Substring("#template(".Length);

            var parms = name.Substring(0, name.Length - 1).Split(new char[] { ',' });

            this.Name = parms[0].ToLower().Trim();

            this.Parameters.AddRange(parms.Select( s=> s.Trim()));
            this.Parameters.RemoveAt(0);

            // Compile children
            Compile(null, val);
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
        internal TFunction(string name, JObject val) 
        {
            var parms   = CompiledTransform.ParseElementParams("function", name, new List<bool> {true} );
            var context = new ExpressionContext(new {});

            this.Name = parms[0].Evaluate(context).ToString().ToLower().Trim();

            this.Parameters.AddRange(parms.Select( s=> s.Evaluate(context).ToString().Trim()));
            this.Parameters.RemoveAt(0);

            // Compile children
            Compile(null, val);
        }

        internal string Name      { get; }

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
        internal TCallTemplate(string name, JObject val) 
        {
            _templateName = name.Substring("#calltemplate(".Length).ReplaceEnding(")", "");

            // Compile children
            Compile(null, val);
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

            var jsonParams = JObject.Parse(paramsOutput.ToString());

            foreach(var paramName in template.Parameters)
                newContext.SetVariable(paramName, jsonParams.GetValue(paramName).ToString());

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
