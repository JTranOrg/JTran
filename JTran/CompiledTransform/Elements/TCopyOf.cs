using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JTran.Common;
using JTran.Expressions;
using JTran.Json;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TCopyOf : TToken
    {
        private readonly IExpression _expression;
        private readonly IValue? _name;

        /****************************************************************************/
        internal TCopyOf(CharacterSpan? name, CharacterSpan val) 
        {
            _name = (name?.Equals("#noobject") ?? false) ? null : CreateValue(name, true, 0);

            var parms = CompiledTransform.ParseElementParams("#copyof", val, CompiledTransform.SingleFalse );

            if(parms.Count == 0)
                throw new Transformer.SyntaxException("Missing expression for #copyof");

            _expression = parms[0];
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            var newScope = _expression.Evaluate(context);
            var name     = _name?.Evaluate(context) as CharacterSpan;

            wrap( ()=>
            { 
                if(name != null)
                    JsonObjectExtensions.ToJson(name, newScope, writer);
                else
                    writer.WriteProperties(newScope);
            });
        }
    }
}
