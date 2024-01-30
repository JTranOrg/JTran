using System;
using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TBaseArray : TContainer
    {
        /****************************************************************************/
        protected TBaseArray()
        {
        }

        internal bool              IsOutputArray { get; set; } = false;
        internal protected IValue? Name          { get; set; }
      
        /****************************************************************************/
        internal string? EvaluateName(ExpressionContext context)
        {
            var arrayName = this.IsOutputArray ? "[]" : (this.Name == null ? null : this.Name.Evaluate(context)?.ToString()?.Trim());

            return string.IsNullOrWhiteSpace(arrayName) ? null : arrayName;
        }
      
        /****************************************************************************/
        internal protected string? WriteContainerName(IJsonWriter output, ExpressionContext context)
        {
            if(this.IsOutputArray)
                return "[]";

            var arrayName = this.EvaluateName(context);

            if(arrayName != null && arrayName != "{}")
            { 
                output.WriteContainerName(arrayName);
            }

            return arrayName;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TArray : TBaseArray
    {
        /****************************************************************************/
        internal TArray(string name)
        { 
           name = name.Substring("#array(".Length, name.Length - "#array(".Length - 1);

           this.Name = CreateValue(name);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            base.Evaluate(output, context, (fnc)=>
            {
                wrap( ()=>
                { 
                    WriteContainerName(output, context);
                    output.StartArray();

                    fnc();

                    output.EndArray();
                });
            });
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TArrayItem : TObject
    {
        /****************************************************************************/
        internal TArrayItem(string name) : base(name)
        {
        }
    }
}
