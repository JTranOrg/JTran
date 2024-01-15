
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using JTran.Extensions;
using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TForEach : TContainer
    {
        private readonly IExpression _expression;
        private readonly IExpression? _name;

        /****************************************************************************/
        internal TForEach(string name) 
        {
            var parms = CompiledTransform.ParseElementParams("foreach", name, new List<bool> {false, true} );

            if(parms.Count < 1)
                throw new Transformer.SyntaxException("Missing expression for #foreach");

            _expression = parms[0];
            _name       = parms.Count > 1 ? parms[1] : null;
        }

        /****************************************************************************/
        internal protected TForEach(string expression, bool expr) 
        {
            _expression = Compiler.Compile(expression);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var result = _expression.Evaluate(context);

            if(result == null)
                return;

            if(result is IEnumerable<object> tryList && !tryList.Any())
                return;

            var arrayName = _name == null ? null : _name.Evaluate(context)?.ToString()?.Trim();
            
            arrayName = string.IsNullOrWhiteSpace(arrayName) ? null : arrayName;

            if(arrayName != null && !(result is IEnumerable<object>))
                result = new List<object> { result };

            // If the result of the expression is an array
            if(result is IEnumerable<object> list && list.Any())
            {       
                wrap( ()=> 
                { 
                    if(arrayName != null && arrayName != "{}")
                        output.WriteContainerName(arrayName);

                    if(arrayName != null && arrayName != "{}")
                        output.StartArray();
                
                    EvaluateChildren(output, arrayName, list, context);

                    if(arrayName != null && arrayName != "{}")
                        output.EndArray();
                });
            }
            else 
            {
                // Not an array. Just treat it as a bind
                base.Evaluate(output, new ExpressionContext(result, context, templates: this.Templates, functions: this.Functions), wrap);
            }
        }

        /****************************************************************************/
        private void EvaluateChildren(IJsonWriter output, string arrayName, IEnumerable<object> list, ExpressionContext context)
        {
            try
            { 
                foreach(var childScope in list)
                { 
                    if(EvaluateChild(output, arrayName, childScope, context))
                        break;
                }
            }
            catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        /****************************************************************************/
        private bool EvaluateChild(IJsonWriter output, string arrayName, object childScope, ExpressionContext context)
        {
            var newContext = new ExpressionContext(childScope, context, templates: this.Templates, functions: this.Functions);
            var bBreak = false;

            try
            { 
                var testWriter = new JsonTestWriter();

                // First test if the child will ever write anything at all
                base.Evaluate(testWriter, newContext, (fnc)=> fnc());
                    
                if(testWriter.NumWrites == 0) 
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

    /****************************************************************************/
    /****************************************************************************/
    internal class TForEachGroup : TContainer
    {
        private readonly IExpression _expression;
        private readonly IExpression _name;
        private readonly IExpression _groupBy;

        /****************************************************************************/
        internal TForEachGroup(string name) 
        {
            var parms = CompiledTransform.ParseElementParams("foreachgroup", name, new List<bool> {false, true} )!;

            _expression = parms[0];
            _groupBy    = parms.Count > 1 ? new Value(parms[1]) : null;
            _name       = parms.Count > 2 ? new Value(parms.Last()) : null;
        }

        /****************************************************************************/
        internal protected TForEachGroup(string expression, bool expr) 
        {
            _expression = Compiler.Compile(expression);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var result = _expression.Evaluate(context);

            // If the result of the expression is an array
            if(!(result is IEnumerable<object> list))
            { 
                list = new List<object> { result };
            }

            // Get the groups
            var groupBy = _groupBy.Evaluate(context).ToString().Trim();

            var groups = list.GroupBy
            (
                (item)=> item.GetSingleValue(groupBy, null),
                (item)=> item,
                (groupValue, items) => { 
                                            IDictionary<string, object> item = new ExpandoObject(); 
                                                    
                                            item[groupBy] = groupValue; 
                                            item["__groupItems"] = items; 
                                                    
                                            return item as ExpandoObject; 
                                        }
            );

            var numGroups = groups.Count();

            if(numGroups == 0)
                return;

            wrap( ()=>
            { 
                // Check to see if we're outputting to an array
                if(_name != null)
                {
                    var arrayName = _name.Evaluate(context).ToString().Trim();
                
                    output.WriteContainerName(arrayName);
                    output.StartArray();
                }

                // Iterate thru the groups
                foreach(var groupScope in groups)
                {
                    var newContext = new ExpressionContext(groupScope, context, templates: this.Templates, functions: this.Functions);

                    newContext.CurrentGroup = (groupScope as dynamic).__groupItems;

                    base.Evaluate(output, newContext, (fnc)=>
                    {
                        output.StartChild();
                        output.StartObject();
                        
                        fnc();

                        output.EndObject();
                        output.EndChild();
                    });

                }

                if(_name != null)
                { 
                    output.EndArray();
                    output.WriteRaw("\r\n");
                }                    
            });
        }
    }


}
