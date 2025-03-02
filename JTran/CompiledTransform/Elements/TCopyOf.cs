﻿using System;
using System.Threading.Tasks;

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
        internal TCopyOf(ICharacterSpan? name, ICharacterSpan val) 
        {
            _name = (name?.Equals("#noobject") ?? false) ? null : CreateValue(name, true, 0);

            var parms = CompiledTransform.ParseElementParams("#copyof", val, CompiledTransform.SingleFalse );

            if(parms.Count == 0)
                throw new Transformer.SyntaxException("Missing expression for #copyof");

            _expression = parms[0];

            this.IsOutput = true;
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            var newScope = _expression.Evaluate(context);
            var name     = _name?.Evaluate(context) as ICharacterSpan;

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
