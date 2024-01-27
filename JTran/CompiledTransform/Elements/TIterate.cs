
using System;
using System.Collections.Generic;

using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TIterate : TContainer
    {
        private readonly IExpression _expression;
        private readonly IExpression? _name;

        /****************************************************************************/
        internal TIterate(string name) 
        {
            var parms = CompiledTransform.ParseElementParams("iterate", name, CompiledTransform.FalseTrue );

            if(parms.Count < 1)
                throw new Transformer.SyntaxException("Missing expression for #iterate");

            _expression = parms[0];
            _name       = parms.Count > 1 ? parms[1] : null;
        }

        /****************************************************************************/
        internal protected TIterate(string expression, bool expr) 
        {
            _expression = Compiler.Compile(expression);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var result = _expression.Evaluate(context);

            if(result == null)
                return;

            if(!double.TryParse(result.ToString(), out double d))
                throw new Transformer.SyntaxException("#iterate expression must resolve to number");

            var numItems = (long)Math.Floor(d);
            var arrayName = _name == null ? null : _name.Evaluate(context)?.ToString()?.Trim();
            
            arrayName = string.IsNullOrWhiteSpace(arrayName) ? null : arrayName;

            wrap( ()=> 
            { 
                if(arrayName != null && arrayName != "{}")
                    output.WriteContainerName(arrayName);

                if(arrayName != null && arrayName != "{}")
                    output.StartArray();
                
                for(int i = 0; i < numItems; ++i)
                { 
                    try
                    { 
                        dynamic dyn = context.Data;

                        dyn._jtran_position = i;

                        if(EvaluateChild(output, arrayName, context.Data, context))
                            break;
                    }
                    catch(AggregateException ex)
                    {
                        throw ex.InnerException;
                    }
                }

                if(arrayName != null && arrayName != "{}")
                    output.EndArray();
            });
        }

        /****************************************************************************/
        private bool EvaluateChild(IJsonWriter output, string arrayName, object childScope, ExpressionContext context)
        {
            var newContext = new ExpressionContext(childScope, context, templates: this.Templates, functions: this.Functions);
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
