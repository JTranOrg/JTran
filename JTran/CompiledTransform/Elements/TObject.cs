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
        internal TObject(ICharacterSpan? name, long lineNumber)
        {
            this.Name = ((name?.Length ?? 0) == 0) ? null : CreateValue(name, true, lineNumber);
        }

        internal IValue? Name  { get; set; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var name = this.Name?.Evaluate(context);
            var csname = name as ICharacterSpan;

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
