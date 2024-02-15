
using System;

using JTran.Expressions;
using JTran.Collections;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TInnerOuterJoinProperty : TToken, IValue
    {
        private readonly IExpression _left;
        private readonly IExpression _right;
        private readonly IExpression _expression;
        private readonly bool _inner;

        /****************************************************************************/
        internal TInnerOuterJoinProperty(string val, bool inner, long lineNumber) 
        {
            var name = inner ? "innerjoin" : "outerjoin";
            var parms = CompiledTransform.ParseElementParams(name, val, CompiledTransform.SingleFalse);

            if(parms.Count < 3)
                throw new Transformer.SyntaxException($"Missing expressions for #{name}") {LineNumber = lineNumber};

            _left       = parms[0];
            _right      = parms[1];
            _expression = parms[2];
            _inner      = inner;
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

            return new InnerOuterJoin(left, right, _expression, context, _inner);
        }

        #endregion

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            throw new NotSupportedException();
        }
    }
}
