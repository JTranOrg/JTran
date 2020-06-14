/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: CompiledTransform.cs					    		        
 *        Class(es): CompiledTransform			         		            
 *          Purpose: Compiles and evaluates tranformations                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
 *                                                                          
 *   Copyright (c) 2020 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                                                                                                 
 *    http://www.opensource.org/licenses/mit-license.php                    
 * 
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

using JTran.Extensions;
using JTran.Expressions;
using System.Collections;
using System.Dynamic;
using System.Data;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("JTranUnitTests")]

namespace JTran
{
    /*****************************************************************************/
    /*****************************************************************************/
    public class ExpressionContext
    {
        private readonly object                                   _data;
        private readonly IDictionary<string, object>              _variables;
        private readonly IDictionary<string, IDocumentRepository> _docRepositories;
        private readonly ExpressionContext                        _parent;

        /*****************************************************************************/
        internal ExpressionContext(object data, TransformerContext transformerContext = null)
        {
            _data            = data;
            _variables       = transformerContext?.Arguments ?? new Dictionary<string, object>();
            _docRepositories = transformerContext?.DocumentRepositories;
            _parent          = null;
        }

        /*****************************************************************************/
        internal ExpressionContext(object data, ExpressionContext parentContext)
        {
            _data            = data;
            _variables       = new Dictionary<string, object>();
            _docRepositories = parentContext?._docRepositories;
            _parent          = parentContext;
        }

        internal object Data      => _data;
        internal bool   PreviousCondition { get; set; }

        /*****************************************************************************/
        internal object GetDocument(string repoName, string docName)
        {
            if(_docRepositories?.ContainsKey(repoName) ?? false)
            { 
                var doc = _docRepositories[repoName].GetDocument(docName);
                    
                return doc.JsonToExpando();
            }

            return null;
        }

        /*****************************************************************************/
        internal object GetVariable(string name)
        {
            if(_variables.ContainsKey(name))
                return _variables[name];

            if(_parent == null)
                throw new SyntaxErrorException($"A variable with that name does not exist: {name}");

            return _parent.GetVariable(name);
        }

        /*****************************************************************************/
        internal void SetVariable(string name, object val)
        {
            _variables.Add(name, val);
        }

        /*****************************************************************************/
        internal object GetDataValue(string name)
        { 
            if(_data is IList<object> list)
            {
                var result = new List<object>();

                foreach(var child in list)
                { 
                    var childResult = GetDataValue(child, name);

                    if(childResult is IList<object> childList)
                        result.AddRange(childList);
                    else
                        result.Add(childResult);
                }

                if(result.Count == 0)
                    return null;

                if(result.Count == 1)
                    return result[0];

                return result;
            }

            return GetDataValue(_data, name);
        }

        /*****************************************************************************/
        private static object GetDataValue(object data, string name)
        { 
            var val = data.GetValue(name, null);

            if(val == null)
                return null;

            if(!val.GetType().IsClass)
            { 
                // If it's any kind of number return it as a decimal
                if(decimal.TryParse(val.ToString(), out decimal dVal))
                    return dVal;

                return val;
            }

            return data.GetValue(name, null);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class CompiledTransform : TContainer
    {
        /****************************************************************************/
        internal protected CompiledTransform()
        {
        }

        /****************************************************************************/
        internal string Transform(string data, TransformerContext context)
        {
            var output  = JObject.Parse("{}");
            var expando = data.JsonToExpando();

            this.Evaluate(output, new ExpressionContext(expando, context));
              
            return output.ToString();
        }

        /****************************************************************************/
        internal static CompiledTransform Compile(string source)
        {
            var transform = new CompiledTransform();

            transform.Compile(JObject.Parse(source));

            return transform;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal abstract class TToken
    {
        internal abstract void Evaluate(JContainer output, ExpressionContext context);

        /****************************************************************************/
        internal protected IValue CreateValue(JToken value)
        {
            var sval = value.ToString();

            if(!sval.StartsWith("#("))
            { 
                if(decimal.TryParse(value.ToString(), out decimal val))
                    return new NumberValue(val);

                return new SimpleValue(value);
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
        internal List<TToken> Children { get; set; } = new List<TToken>();
        private TToken _previous;

        /****************************************************************************/
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            foreach(var child in this.Children)
                child.Evaluate(output, context);
        }

        /****************************************************************************/
        internal protected void Compile(JObject source)
        {   
            foreach(var child in source)
            {
                var newToken = CreateToken(child.Key, child.Value);

                this.Children.Add(newToken);

                _previous = newToken;
            }
        }

        /****************************************************************************/
        private TToken CreateToken(string name, JToken child)
        {   
            var t = child.GetType().ToString();

            if(child is JValue val)
            { 
                if(name.StartsWith("#variable"))
                    return new TVariable(name, val);

                var sval = val.ToString();

                if(sval.StartsWith("#copyof"))
                    return new TCopyOf(name, sval);

               return new TProperty(name, val);
            }

            if(child is JObject obj)
            { 
                if(name.StartsWith("#bind"))
                    return new TBind(name, obj);

                if(name.StartsWith("#foreach"))
                    return new TForEach(name, obj);

                if(name.StartsWith("#if"))
                    return new TIf(name, obj);

                if(name.StartsWith("#elseif"))
                { 
                    if(_previous is TIf || _previous is TElseIf)
                        return new TElseIf(name, obj);

                    throw new SyntaxErrorException("#elseif must follow an #if or another #elseif");
                }

                if(name.StartsWith("#else"))
                { 
                    if(_previous is TIf || _previous is TElseIf)
                        return new TElse(name, obj);

                    throw new SyntaxErrorException("#elseif must follow an #if or an #elseif");
                }

                return new TObject(name, obj);
            }

            return null;
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

            Compile(val);
        }

        internal IValue Name  { get; set; }

        /****************************************************************************/
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            var child = JObject.Parse("{}");

            base.Evaluate(child, context);

            if(output is JObject joutput)
                joutput.Add(this.Name.Evaluate(context).ToString(), child);
            else if(output is JArray jarray)
                jarray.Add(child);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TCopyOf : TToken
    {
        private readonly string _expression;
        private readonly IValue _name;

        /****************************************************************************/
        internal TCopyOf(string name, string val) 
        {
            _name = CreateValue(name);

            val = val.Substring("#copyof(".Length);

            _expression = val.Substring(0, val.Length - 1);
        }

        /****************************************************************************/
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            var newScope = context.Data.GetValue(_expression, context);

            output.Add(new JProperty(_name.Evaluate(context).ToString(), JObject.Parse((newScope as ExpandoObject).ToJson()))); 
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
            name = name.Substring("#bind(".Length);

            var parms = name.Substring(0, name.Length - 1).Split(new char[] { ',' });

            _expression = Compiler.Compile(parms[0]);

            // Compile children
            Compile(val);
        }

        /****************************************************************************/
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            var newScope = _expression.Evaluate(context);

            base.Evaluate(output, new ExpressionContext(newScope, context));
        }
    }

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
            Compile(val);
        }

        /****************************************************************************/
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            if(_expression.EvaluateToBool(context))
            { 
                context.PreviousCondition = true;
                base.Evaluate(output, new ExpressionContext(context.Data, context));
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
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            if(!context.PreviousCondition)
                base.Evaluate(output, context);
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
            Compile(val);
        }

        /****************************************************************************/
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            if(!context.PreviousCondition)
                base.Evaluate(output, context);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TForEach : TContainer
    {
        private readonly IExpression _expression;
        private readonly IExpression _name;

        /****************************************************************************/
        internal TForEach(string name, JObject val) 
        {
            name = name.Substring("#foreach(".Length);

            var parms = name.Substring(0, name.Length - 1).Split(new char[] { ',' });

            _expression = Compiler.Compile(parms[0]);
            _name       = parms.Length > 1 ? new Value(parms[1]) : null;

            // Compile children
            Compile(val);
        }

        /****************************************************************************/
        internal protected TForEach(string expression) 
        {
            _expression = Compiler.Compile(expression);
        }

        /****************************************************************************/
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            var result = _expression.Evaluate(context);

            // If the result of the expression is an array
            if(result is IList list)
            { 
                JContainer arrayOutput;
                
                if(_name != null)
                {
                    arrayOutput = JArray.Parse("[]");

                    var arrayName = _name.Evaluate(context);
                
                    output.Add(new JProperty(arrayName.ToString().Trim(), arrayOutput));
                }
                else
                    arrayOutput = output;

                foreach(var childScope in list)
                { 
                    var childOutput = JObject.Parse("{}");

                    base.Evaluate(childOutput, new ExpressionContext(childScope, context));

                    if(childOutput.Count > 0)
                    { 
                        var schild = childOutput.ToString();

                        if(_name == null)
                        {
                            foreach(var grandchild in childOutput)
                                if(grandchild is KeyValuePair<string, JToken> pair)
                                    arrayOutput.Add(new JProperty(pair.Key, pair.Value));
                        }
                        else 
                            arrayOutput.Add(childOutput);
                    }
                }
            }
            else 
            {
                // Not an array. Just treat it as a bind
                base.Evaluate(output, new ExpressionContext(result, context));
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
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            output.Add(new JProperty(this.Name.Evaluate(context).ToString(), this.Value.Evaluate(context)));
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
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            var name = this.Name.Evaluate(context).ToString();
            var val  = this.Value.Evaluate(context);

            context.SetVariable(name, val);
        }
    }

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

        /****************************************************************************/
        object IValue.Evaluate(ExpressionContext context)
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
            var val = _expression.Evaluate(context);

            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal dval))
            {
                if(Math.Floor(dval) == dval)
                    return Convert.ToInt64(dval);

                return dval;
            }

            return val;
        }
    }
}
