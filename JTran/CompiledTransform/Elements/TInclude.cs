
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using JTran.Expressions;

namespace JTran
{
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
}
