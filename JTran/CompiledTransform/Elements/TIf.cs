
using System;

using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TIf : TContainer
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TIf(string name) : this(name, "#if(") 
        {
        }

        /****************************************************************************/
        internal protected TIf(string name, string elementName) 
        {
            name = name.Substring(elementName.Length);

            _expression = Compiler.Compile(name.Substring(0, name.Length - 1));
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
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
        internal TElseIf(string name) : base(name, "#elseif(")
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
        internal TElse(string name) 
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
