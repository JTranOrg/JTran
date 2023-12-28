using System;
using System.Dynamic;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TArray : TContainer
    {
        internal TArray(string name)
        { 
           name = name.Substring("#array(".Length, name.Length - "#array(".Length - 1);

           this.Name = CreateValue(name);
        }

        internal IValue Name  { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            base.Evaluate(output, context, (fnc)=>
            {
                wrap( ()=>
                { 
                    output.WriteContainerName(this.Name.Evaluate(context).ToString());
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
