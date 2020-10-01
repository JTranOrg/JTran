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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;

using Newtonsoft.Json.Linq;

using JTran.Extensions;
using JTran.Expressions;
using System.Reflection;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class CompiledTransform : TContainer
    {
        /****************************************************************************/
        internal protected CompiledTransform()
        {
        }

        /****************************************************************************/
        internal string Transform(string data, TransformerContext context, IDictionary<string, Function> extensionFunctions)
        {
            var output  = JObject.Parse("{}");
            var expando = data.JsonToExpando();

            this.Evaluate(output, new ExpressionContext(expando, "__root", context, extensionFunctions, templates: this.Templates));
              
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
        internal List<TToken>                   Children    { get; }
        internal IDictionary<string, TTemplate> Templates   { get; }

        private TToken _previous;

        /****************************************************************************/
        internal TContainer()
        {
            this.Children  = new List<TToken>();
            this.Templates = new Dictionary<string, TTemplate>();
        }

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

                if(newToken != null)
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
                if(name.StartsWith("#template"))
                { 
                    var template = new TTemplate(name, obj);

                    this.Templates.Add(template.Name, template);
                    return null;
                }

                if(name.StartsWith("#bind"))
                    return new TBind(name, obj);

                if(name.StartsWith("#foreachgroup"))
                    return new TForEachGroup(name, obj);

                if(name.StartsWith("#foreach"))
                    return new TForEach(name, obj);

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
            { 
                var name = this.Name.Evaluate(context)?.ToString();

                if(string.IsNullOrWhiteSpace(name))
                    throw new Transformer.SyntaxException("Property name evaluates to null or empty string");

                joutput.Add(name, child);
            }
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

            base.Evaluate(output, new ExpressionContext(data: newScope, parentContext: context, templates: this.Templates));
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
                base.Evaluate(output, new ExpressionContext(context.Data, context, templates: this.Templates));
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

                    base.Evaluate(childOutput, new ExpressionContext(childScope, context, templates: this.Templates));

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
                base.Evaluate(output, new ExpressionContext(result, context, templates: this.Templates));
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
            Compile(val);
        }

        /****************************************************************************/
        internal protected TForEachGroup(string expression) 
        {
            _expression = Compiler.Compile(expression);
        }

        /****************************************************************************/
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            var result = _expression.Evaluate(context);

            // If the result of the expression is an array
            if(result is IList<object> list)
            { 
                JContainer arrayOutput;

                // Checkto see if we're outputting to an array
                if(_name != null)
                {
                    arrayOutput = JArray.Parse("[]");

                    var arrayName = _name.Evaluate(context);
                
                    output.Add(new JProperty(arrayName.ToString().Trim(), arrayOutput));
                }
                else
                    arrayOutput = output;

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

                // Iterate thru the groups
                foreach(var groupScope in groups)
                {
                    var groupOutput = JObject.Parse("{}");
                    var newContext  = new ExpressionContext(groupScope, context, templates: this.Templates);

                    newContext.CurrentGroup = (groupScope as dynamic).__groupItems;

                    base.Evaluate(groupOutput, newContext);

                    if(groupOutput.Count > 0)
                    { 
                        var schild = groupOutput.ToString();

                        if(_name == null)
                        {
                            foreach(var grandchild in groupOutput)
                                if(grandchild is KeyValuePair<string, JToken> pair)
                                    arrayOutput.Add(new JProperty(pair.Key, pair.Value));
                        }
                        else 
                            arrayOutput.Add(groupOutput);
                    }
                }
            }
            else 
            {
                // Not an array. Just treat it as a bind
                base.Evaluate(output, new ExpressionContext(result, context, templates: this.Templates));
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
            output.Add(new JProperty(this.Name?.Evaluate(context)?.ToString() ?? "", this.Value.Evaluate(context)));
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
    internal class TTemplate : TContainer
    {
        public List<string> Parameters { get; } = new List<string>();

        /****************************************************************************/
        internal TTemplate(string name, JObject val) 
        {
            name = name.Substring("#template(".Length);

            var parms = name.Substring(0, name.Length - 1).Split(new char[] { ',' });

            this.Name = parms[0].ToLower();

            this.Parameters.AddRange(parms);
            this.Parameters.RemoveAt(0);

            // Compile children
            Compile(val);
        }

        internal string Name      { get; }

        /****************************************************************************/
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            var newContext = new ExpressionContext(context.Data, context);

            base.Evaluate(output, newContext);
        }    
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TCallTemplate : TContainer
    {
        /****************************************************************************/
        internal TCallTemplate(string name, JObject val) 
        {
            // Compile children
            Compile(val);
        }

        internal string Name      { get; }

        /****************************************************************************/
        internal override void Evaluate(JContainer output, ExpressionContext context)
        {
            var newContext = new ExpressionContext(context.Data, context);

            base.Evaluate(output, newContext);
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
            var val = _expression?.Evaluate(context);

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
