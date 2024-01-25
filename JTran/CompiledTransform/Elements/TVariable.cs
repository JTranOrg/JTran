﻿using System;

using JTran.Json;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal interface IVariable
    {
        object GetActualValue(ExpressionContext context);
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TVariable : TProperty, IVariable
    {
        /****************************************************************************/
        internal TVariable(string name, object val) : base(name.Substring("#variable(".Length, name.Length - "#variable(".Length - 1), val)
        {
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var name = this.Name.Evaluate(context).ToString();

            context.SetVariable(name, this);
        }

        /****************************************************************************/
        public object GetActualValue(ExpressionContext context)
        {
            return this.Value.Evaluate(context);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal abstract class TVariableContainer : TContainer, IVariable
    {    
        /****************************************************************************/
        internal TVariableContainer(string name) 
        {
            this.Name = name.Substring("#variable(".Length, name.Length - "#variable(".Length - 1);
        }

        internal string Name { get; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            context.SetVariable(this.Name, this);
        }
        
        /****************************************************************************/
        public object GetActualValue(ExpressionContext context)
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
        internal TVariableObject(string name) : base(name)
        {
        }

        /****************************************************************************/
        internal override object EvaluateValue(ExpressionContext context)
        {
            var output = new JsonStringWriter();

            output.StartObject();
            BaseEvaluate(output, context, (fnc)=> fnc());
            output.EndObject();

            return output.ToString().JsonToExpando();
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TVariableArray : TVariableContainer
    {    
        /****************************************************************************/
        internal TVariableArray(string name)  : base(name)
        {
        }

        /****************************************************************************/
        internal override object EvaluateValue(ExpressionContext context)
        {
            var output  = new JsonStringWriter();

            output.StartArray();
            BaseEvaluate(output, context, (fnc)=> fnc());
            output.EndArray();
                        
            var result = "{ 'result': " + output.ToString() + "}"; // ??? temporary
            dynamic dyn = result.JsonToExpando();

            return dyn.result;
        }

        /****************************************************************************/
        protected override TToken CreatePropertyToken(string name, object val, object? previous)
        {
            return new TSimpleArrayItem(val);
        }
    }
}