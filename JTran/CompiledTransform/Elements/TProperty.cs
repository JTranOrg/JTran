using System;

using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TProperty : TToken
    {
        /****************************************************************************/
        internal TProperty(string name, object val)
        {
            this.Name  = CreateValue(name);
            this.Value = CreateValue(val);
        }

        internal IValue Name  { get; set; }
        internal IValue Value { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var name = this.Name?.Evaluate(context)?.ToString() ?? "";
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
        internal TPropertyIf(string name, object val) : this(name, val, "#if(") 
        {
        }

        public object EvaluatedValue { get; set; }
        public bool   If             { get; set; } = false;

        /****************************************************************************/
        internal protected TPropertyIf(string name, object val, string elementName) : base(name, val) 
        {
            name = name.Substring(elementName.Length);

            _expression = Compiler.Compile(name.Substring(0, name.Length - 1));
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(_expression.EvaluateToBool(context))
            { 
                this.If = true;
                this.EvaluatedValue = this.Value.Evaluate(context);
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TPropertyElseIf : TPropertyIf
    {
        /****************************************************************************/
        internal TPropertyElseIf(string name, object val) : base(name, val, "#elseif(")
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
        internal TPropertyElse(string name, object val)  : base(name, val)
        {
        }

        public object EvaluatedValue { get; set; }
        public bool   If             { get; set; } = false;

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!context.PreviousCondition)
            { 
                this.If = true;
                this.EvaluatedValue = this.Value.Evaluate(context);
            }
        }
    }


}
