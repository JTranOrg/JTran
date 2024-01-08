
using System;
using System.Collections.Generic;
using System.Dynamic;

using JTran.Expressions;
using JTran.Extensions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TIif : TToken
    {
        private readonly IValue _name;
        private readonly IExpression _expression;
        private readonly IExpression _if;
        private readonly IExpression _else;

        /****************************************************************************/
        internal TIif(string name, string val) 
        {
            _name = name == null ? null : CreateValue(name);

            var parms = CompiledTransform.ParseElementParams("iif", val, new List<bool> {false, true} );

            if(parms.Count < 3)
                throw new Transformer.SyntaxException("Missing expressions for #iif");

            _expression = parms[0];
            _if         = parms[1];
            _else       = parms[2];
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            wrap( ()=>
            { 
                var result = _expression.EvaluateToBool(context) ? _if.Evaluate(context) : _else.Evaluate(context);
                var name   = _name?.Evaluate(context)?.ToString();

                writer.WriteProperty(name, result);
            });
        }
    }
}
