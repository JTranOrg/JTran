
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using JTran.Expressions;
using JTran.Collections;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TInnerJoinProperty : TToken, IValue
    {
        private readonly IExpression _left;
        private readonly IExpression _right;
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TInnerJoinProperty(string val) 
        {
            var parms = CompiledTransform.ParseElementParams("innerjoin", val, CompiledTransform.SingleFalse);

            if(parms.Count < 3)
                throw new Transformer.SyntaxException($"Missing expressions for #innerjoin");

            _left       = parms[0];
            _right      = parms[1];
            _expression = parms[2];
        }

        /****************************************************************************/
        public bool EvaluateToBool(ExpressionContext context)
        {
            return false;
        }

        #region IValue

        /****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            var left  = _left.Evaluate(context);
            var right = _right.Evaluate(context);

            return new InnerJoin(left, right, _expression, context);
        }

        #endregion

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            throw new NotSupportedException();
        }
    }
}
