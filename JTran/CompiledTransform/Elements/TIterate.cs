
using System;

using JTran.Common;
using JTran.Expressions;
using JTran.Parser;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TIterate : TBaseArray
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TIterate(ICharacterSpan name) 
        {
            var parms = CompiledTransform.ParseElementParams("#iterate", name, CompiledTransform.FalseTrue );

            if(parms.Count < 1)
                throw new Transformer.SyntaxException("Missing expression for #iterate");

            _expression = parms[0];

            var arrayName = parms.Count > 1 ? (parms[1] as Value) : null;

            if(arrayName != null)
            { 
                if(arrayName?.Evaluate(null) is Token token && token.Type == Token.TokenType.ExplicitArray)
                { 
                    this.IsOutputArray = true;
                    this.Name = new SimpleValue(TToken.EmptyArray);
                }
                else
                { 
                    this.IsOutputArray = false;
                    this.Name = new SimpleValue(arrayName!.Evaluate(null));
                }
            }
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var result = _expression.Evaluate(context);

            if(result == null || !result.TryParseInt(out int numItems)) 
                throw new Transformer.SyntaxException("#iterate expression must resolve to a number");

            wrap( ()=> 
            { 
                var arrayName = WriteContainerName(output, context);

                if(this.IsOutputArray || (arrayName != null && !arrayName.Equals(EmptyObject)))
                    output.StartArray();
                
                var index = 0L;

                for(int i = 0; i < numItems; ++i)
                { 
                    try
                    { 
                        if(EvaluateChild(output, arrayName!, context.Data!, context, ref index))
                            break;
                    }
                    catch(AggregateException ex)
                    {
                        throw ex.InnerException!;
                    }
                }

                if(this.IsOutputArray || (arrayName != null && !arrayName.Equals(EmptyObject)))
                    output.EndArray();
            });
        }

        /****************************************************************************/
        private bool EvaluateChild(IJsonWriter output, ICharacterSpan arrayName, object childScope, ExpressionContext context, ref long index)
        {
            var newContext = new ExpressionContext(childScope, context, templates: this.Templates, functions: this.Functions) { Index = index };
            var bBreak = false;

           try
            { 
                var testWriter = new JsonStringWriter();

                testWriter.StartObject();

                // First test if the child will ever write anything at all
                base.Evaluate(testWriter, newContext, (fnc)=> fnc());
                    
                testWriter.EndObject();

                // Will never write anything, move on to next child
                var test = testWriter.ToString();

                if(test.Replace("\r", "").Replace("\n", "") == "{}")
                    return false;
            }
            catch(Break)
            {
                // Continue with output below
            }

            // Clear out any variables created during test run above
            newContext.ClearVariables();

            ++index;

            base.Evaluate(output, newContext, (fnc)=> 
            {
                if(arrayName != null)
                    output.StartObject();

                try
                { 
                    fnc();
                }
                catch(Break)
                {
                    bBreak = true;
                }

                if(arrayName != null)
                    output.EndObject();
            }); 

            return bBreak;
        }
    }
}
