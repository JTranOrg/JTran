
using System;
using JTran.Common;
using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TIf : TContainer
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TIf(CharacterSpan name) : this(name, "#if(") 
        {
        }

        /****************************************************************************/
        internal protected TIf(CharacterSpan name, string elementName) 
        {
            _expression = Compiler.Compile(name.Substring(elementName.Length, name.Length - elementName.Length - 1));
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            context.PreviousCondition = false;

            if(_expression.EvaluateToBool(context))
            { 
                context.PreviousCondition = true;
                base.Evaluate(output, new ExpressionContext(context.Data, context, templates: this.Templates, functions: this.Functions), wrap);
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TElseIf : TIf
    {
        /****************************************************************************/
        internal TElseIf(CharacterSpan name) : base(name, "#elseif(")
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
                base.Evaluate(output, context, wrap);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TElse : TContainer
    {
        /****************************************************************************/
        internal TElse() 
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
                base.Evaluate(output, context, wrap);
        }
    }
}
