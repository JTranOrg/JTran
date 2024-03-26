﻿using System;

using JTran.Common;
using JTran.Json;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal interface IVariable
    {
        object? GetActualValue(ExpressionContext context);
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TVariable : TProperty, IVariable
    {
        private ExpressionContext? _context;

        /****************************************************************************/
        internal TVariable(ICharacterSpan name, object val, long lineNumber) 
                    : base(name.Substring("#variable(".Length, name.Length - "#variable(".Length - 1), val, lineNumber)
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var name = this.Name.Evaluate(context) as ICharacterSpan;

            context.SetVariable(name!, this);

            _context = context;
        }

        /****************************************************************************/
        public object? GetActualValue(ExpressionContext context)
        {
            // Use the context when first evaluated. The param here is the context where the var is being used which we don't want to use.
            var result = this.Value.Evaluate(_context!);

            if(result != null && result is ICharacterSpan cspan)
                cspan.ExpressionResult = false; // Suppress inline string transforms

            var name = this.Name.Evaluate(context) as ICharacterSpan;

            if(name.StartsWith("Tempor"))
                return result;

            return result;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal abstract class TVariableContainer : TContainer, IVariable
    {    
        /****************************************************************************/
        internal TVariableContainer(ICharacterSpan name) 
        {
            this.Name = name.Substring("#variable(".Length, name.Length - "#variable(".Length - 1);
        }

        internal ICharacterSpan Name { get; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            context.SetVariable(this.Name, this);
        }
        
        /****************************************************************************/
        public object? GetActualValue(ExpressionContext context)
        {
            var result = GetActualValueInternal(context);

            if(result != null && result is ICharacterSpan cspan)
                cspan.ExpressionResult = false; // Suppress inline string transforms

            return result;
        }

        /****************************************************************************/
        private object? GetActualValueInternal(ExpressionContext context)
        {
            var newContext = new ExpressionContext(context.Data, context);

            if(this.Children[0] is IPropertyCondition)
            {
                if(this.Children.EvaluateConditionals(newContext, out object result))
                    return result;
            }
            else
               return EvaluateValue(newContext);

            return null;
        }

        /****************************************************************************/
        internal protected void BaseEvaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            base.Evaluate(output, context, (fnc)=> fnc());
        }

        internal abstract object EvaluateValue(ExpressionContext context);
    }    

    /****************************************************************************/
    /****************************************************************************/
    internal class TVariableObject : TVariableContainer
    {    
        /****************************************************************************/
        internal TVariableObject(ICharacterSpan name) : base(name)
        {
        }

        /****************************************************************************/
        internal override object EvaluateValue(ExpressionContext context)
        {
            var output = new JsonStringWriter();

            output.StartObject();
            BaseEvaluate(output, context, (fnc)=> fnc());
            output.EndObject();

            return output.ToString().ToJsonObject();
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TVariableArray : TVariableContainer
    {    
        /****************************************************************************/
        internal TVariableArray(ICharacterSpan name)  : base(name)
        {
        }

        /****************************************************************************/
        internal override object EvaluateValue(ExpressionContext context)
        {
            var output  = new JsonStringWriter();

            output.StartArray();
            BaseEvaluate(output, context, (fnc)=> fnc());
            output.EndArray();
                        
            return output.ToString().ToJsonObject();
        }

        /****************************************************************************/
        protected override TToken CreatePropertyToken(ICharacterSpan name, object val, object? previous, long lineNumber)
        {
            return new TSimpleArrayItem(val as ICharacterSpan, lineNumber);
        }
    }
}
