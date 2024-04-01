using System;

using JTran.Common;
using JTran.Expressions;
using JTran.Extensions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TMap : TContainer, IPropertyCondition
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TMap(ICharacterSpan name) : this(name, "#map(") 
        {
        }

        /****************************************************************************/
        public object EvaluatedValue { get; set; }
        public bool   If             => true;

        /****************************************************************************/
        internal protected TMap(ICharacterSpan name, string elementName) 
        {
            _expression = Compiler.Compile(name.Substring(elementName.Length, -1)); // -1 means lop off last character
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var val = _expression.Evaluate(context);
            var newContext = new ExpressionContext(val, context);

            foreach(IPropertyCondition mapItem in this.Children)
            {
                mapItem.Evaluate(null, newContext, null);

                if(mapItem.If)
                {
                    this.EvaluatedValue = mapItem.EvaluatedValue;
                    break;
                }
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TMapItem : TProperty, IPropertyCondition
    {
        private readonly IExpression? _expression;

        /****************************************************************************/
        internal TMapItem(ICharacterSpan name, object val, long lineNumber) : this(name, val, "#mapitem", lineNumber) 
        {
        }

        /****************************************************************************/
        internal protected TMapItem(ICharacterSpan name, object val, string elementName, long lineNumber) : base(name, val, lineNumber)  
        {
            name = name.Substring(elementName.Length);

            if(!name.IsNullOrWhiteSpace())
                _expression = Compiler.Compile(name.Substring(1, name.Length - 2));
        }

        public object EvaluatedValue { get; set; }
        public bool   If             { get; set; } = false;

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            this.If = false;

            if(_expression != null)
            {
                var val = _expression.Evaluate(context);

                if(val is bool bVal)
                {   
                    if(!bVal)
                        return;
                }
                else 
                {   
                    // If the expression evaluates to bool then we use item if it true as above
                    //   but if we want to compare against the map value that is a bool then we
                    //   use a 'true' or 'false' string for the mapitem expression
                    var bVal1 = val?.ToString()?.ToLower() ?? "";

                    if(bVal1 == "true" || bVal1 == "false")
                    {
                        var bVal2 = context.Data?.ToString()?.ToLower() ?? "";

                        if(bVal1 != bVal2)
                            return;

                        goto Next;
                    }

                    if(context.Data.CompareTo(val) != 0)
                        return;
                }
            }

          Next:

            this.EvaluatedValue = this.Value.Evaluate(context);
            this.If = true;
        }
    }
}
