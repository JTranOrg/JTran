using System;
using System.Collections.Generic;
using System.Dynamic;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TExplicitArray : TContainer
    {
        /****************************************************************************/
        internal TExplicitArray(string? name)
        {
            this.Name = name != null ? CreateValue(name) : null;
        }

        /****************************************************************************/
        internal override TToken CreateObject(string name)
        {
            var obj = new TObject(null);

            this.Children.Add(obj);

            return obj;
        }

        /****************************************************************************/
        internal override TToken CreateArray(string name, object? val)
        {
            var obj = new TExplicitArray(null);

            this.Children.Add(obj);

            return obj;
        }

        /****************************************************************************/
        internal override TToken CreateProperty(string name, object val)
        {
            var obj = new TSimpleArrayItem(val);

            this.Children.Add(obj);

            return obj;
        }

        internal IValue Name  { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            base.Evaluate(output, context, (fnc)=>
            {
                wrap( ()=>
                { 
                    if(output.InObject)
                    { 
                        var name = this.Name.Evaluate(context)?.ToString();

                        if(string.IsNullOrWhiteSpace(name))
                            throw new Transformer.SyntaxException("Array name evaluates to null or empty string");
            
                        output.WriteContainerName(name);
                    }

                    output.StartArray();
                    fnc();
                    output.EndArray();
                });
            });
        }
    }
}
