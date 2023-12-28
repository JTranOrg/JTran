using System;
using System.Collections.Generic;
using System.Dynamic;

using JTran.Expressions;

namespace JTran
{
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
}
