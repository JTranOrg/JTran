﻿using System;
using System.Collections.Generic;
using System.Dynamic;

using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TThrow: TToken
    {
        private IExpression _code;
        private IValue _message;

        /****************************************************************************/
        internal TThrow(string name, object val)
        {
            var parms = CompiledTransform.ParseElementParams("throw", name, new List<bool> {false, true} );

            _code = parms.Count > 0 ? parms[0] : null;

            _message = CreateValue(val);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var msg = _message.Evaluate(context);

            if(_code != null)
                throw new Transformer.UserError(_code.Evaluate(context).ToString(), msg.ToString());

            throw new Transformer.UserError(msg.ToString());
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
            try
            { 
                var newOutput = new JsonStringWriter();

                base.Evaluate(newOutput, context, (fnc)=> fnc());

                wrap( ()=> output.WriteRaw(newOutput.ToString()));

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
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TCatch(string name) 
        {
            var parms = CompiledTransform.ParseElementParams("catch", name, new List<bool> {false, true} );

            _expression = parms.Count > 0 ? parms[0] : null;
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