using System;

using JTran.Common;
using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TProperty : TToken
    {
        /****************************************************************************/
        internal TProperty(ICharacterSpan name, object val, long lineNumber)
        {
            this.Name  = CreateValue(name, true, lineNumber);
            this.Value = CreateValue(val, false, lineNumber);
        }

        internal IValue Name  { get; set; }
        internal IValue Value { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var name = this.Name?.Evaluate(context) as ICharacterSpan;
            var val  = this.Value.Evaluate(context);

            wrap( ()=> output.WriteProperty(name, val));
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TPropertyIf : TProperty, IPropertyCondition
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TPropertyIf(ICharacterSpan name, object val, long lineNumber) : this(name, val, "#if(", lineNumber) 
        {
        }

        public object? EvaluatedValue { get; set; }
        public bool    If             { get; set; } = false;

        /****************************************************************************/
        internal protected TPropertyIf(ICharacterSpan name, object val, string elementName, long lineNumber) : base(name, val, lineNumber) 
        {
            _expression = Compiler.Compile(name.Substring(elementName.Length, -1));
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            context.PreviousCondition = false;

            if(_expression.EvaluateToBool(context))
            { 
                this.If = true;
                this.EvaluatedValue = this.Value.Evaluate(context);
                context.PreviousCondition = true;
            }
            else
                this.If = false;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TPropertyElseIf : TPropertyIf
    {
        /****************************************************************************/
        internal TPropertyElseIf(ICharacterSpan name, object val, long lineNumber) : base(name, val, "#elseif(", lineNumber)
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
            { 
                base.Evaluate(output, context, wrap);
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TPropertyElse : TProperty, IPropertyCondition
    {
        /****************************************************************************/
        internal TPropertyElse(ICharacterSpan name, object val, long lineNumber)  : base(name, val, lineNumber)
        {
        }

        public object? EvaluatedValue { get; set; }
        public bool    If             { get; set; } = false;

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
            { 
                this.If = true;
                this.EvaluatedValue = this.Value.Evaluate(context);
            }
            else
                this.If = false;
        }
    }
}
