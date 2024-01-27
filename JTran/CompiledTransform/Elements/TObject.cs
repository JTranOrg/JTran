using System;
using System.Dynamic;
using System.Linq;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TObject : TContainer
    {
        /****************************************************************************/
        internal TObject(string name)
        {
            this.Name = CreateValue(name);
        }

        internal IValue Name  { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var name = this.Name.Evaluate(context)?.ToString();

           if(output.InObject && string.IsNullOrWhiteSpace(name))
               throw new Transformer.SyntaxException("Property name evaluates to null or empty string");

            if(this.Children.Any() && this.Children[0] is IPropertyCondition)
            {
                if(this.Children.EvaluateConditionals(context, out object result))
                {
                    wrap( ()=> output.WriteProperty(name, result));
                }
                else
                {
                    wrap( ()=> output.WriteProperty(name, null));
                }
            }
            else
            { 
                wrap( ()=>
                { 
                    base.Evaluate(output, context, f=> 
                    {
                        if(output.InObject)
                        { 
                            output.WriteContainerName(name);
                        }

                        output.StartObject();
                        f();
                        output.EndObject();
                    });
                });
            }
        }
    }   
}
