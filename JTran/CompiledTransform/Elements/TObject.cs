using JTran.Common;
using System;
using System.Linq;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TObject : TContainer
    {
        /****************************************************************************/
        internal TObject(CharacterSpan name, long lineNumber)
        {
            this.Name = CreateValue(name, true, lineNumber);
        }

        internal IValue Name  { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var name = this.Name.Evaluate(context);
            var csname = name as CharacterSpan;

           if(output.InObject && (csname?.IsNullOrWhiteSpace() ?? true))
               throw new Transformer.SyntaxException("Property name evaluates to null or empty string");

            if(this.Children.Any() && this.Children[0] is IPropertyCondition)
            {
                if(this.Children.EvaluateConditionals(context, out object result))
                {
                    wrap( ()=> output.WriteProperty(csname!, result));
                }
                else
                {
                    wrap( ()=> output.WriteProperty(csname!, null));
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
                            output.WriteContainerName(csname!);
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
