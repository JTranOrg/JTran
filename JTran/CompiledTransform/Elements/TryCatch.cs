using System;
using System.Threading.Tasks;

using JTran.Common;
using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TThrow: TToken
    {
        private IExpression? _code;
        private IValue _message;

        /****************************************************************************/
        internal TThrow(ICharacterSpan name, object val, long lineNumber)
        {
            var parms = CompiledTransform.ParseElementParams("#throw", name, CompiledTransform.FalseTrue );

            _code = parms.Any() ? parms[0] : null;

            _message = CreateValue(val as ICharacterSpan, true, lineNumber); // It's not a name per se but we want it to evaluate to a simple string nevertheless
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var msg = _message.Evaluate(context)?.ToString() ?? "";

            if(_code != null)
                throw new Transformer.UserError(_code!.Evaluate(context)!.ToString()!, msg);

            throw new Transformer.UserError(msg);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TBreak : TToken
    {
        /****************************************************************************/
        internal TBreak()
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            throw new Break();
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class Break : Exception
    { 
        internal Break() { }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TTry : TContainer
    {
        /****************************************************************************/
        internal TTry() 
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            context.PreviousCondition = false;

            try
            { 
                wrap( ()=> base.Evaluate(output, context, (fnc)=> fnc()) );

                context.PreviousCondition = true;
            }
            catch(Transformer.UserError ex)
            {
                context.UserError = ex;

                // Do nothing
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TCatch : TContainer
    {
        private readonly IExpression? _expression;

        /****************************************************************************/
        internal TCatch(ICharacterSpan name) 
        {
            var parms = CompiledTransform.ParseElementParams("#catch", name, CompiledTransform.FalseTrue );

            _expression = parms.Any() ? parms[0] : null;
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
            { 
                if(_expression == null || _expression.EvaluateToBool(context))
                { 
                    base.Evaluate(output, context, wrap);
                    context.PreviousCondition = true;
                }
            }
        }
    }
}
