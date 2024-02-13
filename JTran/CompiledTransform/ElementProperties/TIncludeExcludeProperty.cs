
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TIncludeExcludeProperty : TToken, IValue
    {
        private readonly IExpression _expression;
        private readonly IDictionary<string, string> _properties;
        private readonly bool _include;

        /****************************************************************************/
        internal TIncludeExcludeProperty(string val, bool include, long lineNumber) 
        {
            var name  = include ? "include" : "exclude";
            var parms = CompiledTransform.ParseElementParams(name, val, CompiledTransform.FalseTrue);

            _include = include;

            if(parms.Count == 0)
                throw new Transformer.SyntaxException($"Missing expression for #{name}") { LineNumber = lineNumber};

            _expression = parms[0];
            _properties = parms.Skip(1).Select( s=> s.ToString()).ToDictionary( k=> k, v=> v);
        }

        /****************************************************************************/
        public bool EvaluateToBool(ExpressionContext context)
        {
            return false;
        }

        #region IValue

        public object Evaluate(ExpressionContext context)
        {
            var newScope = _expression.Evaluate(context);

            if(newScope is ExpandoObject expObject)
            { 
                var properties = expObject.Where( kv=> _properties.ContainsKey(kv.Key) == _include);

                if(properties.Any())
                {
                    var result = new ExpandoObject();

                    foreach(var kv in properties)
                        result.TryAdd(kv.Key, kv.Value);

                    return result;
                }
            }

            return null;
        }

        #endregion

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            var newScope = _expression.Evaluate(context);

            if(newScope is ExpandoObject expObject)
            { 
                var properties = expObject.Where( kv=> _properties.ContainsKey(kv.Key) == _include);

                if(properties.Any())
                { 
                    wrap( ()=>
                    {
                        foreach(var kv in properties)
                            writer.WriteProperty(kv.Key, kv.Value);
                    });
                }
            }

            return;
        }
    }
}
