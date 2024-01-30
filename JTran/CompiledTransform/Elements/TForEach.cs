
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using JTran.Extensions;
using JTran.Expressions;
using JTran.Parser;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TForEach : TBaseArray
    {
        private readonly IExpression _expression;
        
        /****************************************************************************/
        internal TForEach(string name) 
        {
            var parms = CompiledTransform.ParseElementParams("foreach", name, CompiledTransform.FalseTrue );

            if(parms.Count < 1)
                throw new Transformer.SyntaxException("Missing expression for #foreach");

            _expression = parms[0];

            var arrayName = parms.Count > 1 ? (parms[1] as Value) : null;

            if(arrayName != null)
            { 
                if(arrayName?.Evaluate(null) is Token token && token.Type == Token.TokenType.ExplicitArray)
                { 
                    this.IsOutputArray = true;
                    this.Name = new SimpleValue("[]");
                }
                else
                { 
                    this.IsOutputArray = false;
                    this.Name = new SimpleValue(arrayName!.Evaluate(null));
                }
            }
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
           
            if(!(result is IEnumerable<object>))
                result = new [] { result };

            // If the result of the expression is an array
            if(result is IEnumerable<object> list && list.Any())
            {       
                wrap( ()=> 
                { 
                    var arrayName = WriteContainerName(output, context);

                    if(this.IsOutputArray || (arrayName != null && arrayName != "{}"))
                        output.StartArray();
                
                    EvaluateChildren(output, arrayName, list, context);

                    if(this.IsOutputArray || (arrayName != null && arrayName != "{}"))
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
    internal class TForEachGroup : TBaseArray
    {
        private readonly IExpression  _expression;
        private readonly IEnumerable<string>? _groupBy;

        /****************************************************************************/
        internal TForEachGroup(string name) 
        {
            var parms = CompiledTransform.ParseElementParams("foreachgroup", name, CompiledTransform.FalseTrue )!;

            _expression = parms![0];
            this.Name = parms.Count > 2 ? new SimpleValue(parms.Last()!) : null;

            if(parms.Count > 1)
            { 
                var groupBy = (parms[1] as Value)!.Evaluate(null) as Token;

                if(groupBy!.Type == Token.TokenType.ExplicitArray)
                    _groupBy = groupBy.Select(x => x.Value );
                else 
                    _groupBy = new [] { groupBy.Value };
            }
        }

        /****************************************************************************/
        internal protected TForEachGroup(string expression, bool expr) 
        {
            _expression = Compiler.Compile(expression);
        }

        /****************************************************************************/
        private class GroupByComparer : IEqualityComparer<GroupKey>
        {
            private readonly IEnumerable<string> _fields;

            public GroupByComparer(IEnumerable<string> fields)
            {
                _fields = fields;
            }

            public bool Equals(GroupKey x, GroupKey y)
            {
                foreach(var field in _fields)
                {
                    var compare = x[field].CompareTo(y[field], out Type t);

                    if(compare != 0)
                        return false;
                }

                return true;
            }

            public int GetHashCode(GroupKey obj)
            {
                return obj.ToString().GetHashCode();
            }
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
            IEnumerable<ExpandoObject>? groups;

            if(_groupBy.IsSingle())
            {
                var groupBy = _groupBy.First();

                groups = list.GroupBy
                (
                    (item)=> item.GetSingleValue(groupBy, null),
                    (item)=> item,
                    (groupValue, items) => {    
                                                var newObj = new ExpandoObject();

                                                newObj.TryAdd(groupBy, groupValue);
                                                newObj.TryAdd("__groupItems", items); 
                                                    
                                                return newObj; 
                                            }
                );
            }
            else
            { 
                groups = list.GroupBy
                (
                    (item)=> item.GetGroupByKey(_groupBy!),
                    (item)=> item,
                    (groupValue, items) => {    
                                                var newObj = new ExpandoObject();

                                                foreach(var item in groupValue)
                                                    newObj.TryAdd(item.Key, item.Value);

                                                newObj.TryAdd("__groupItems", items); 
                                                    
                                                return newObj;
                    },
                    new GroupByComparer(_groupBy)
                );
            }

            var numGroups = groups.Count();

            if(numGroups == 0)
                return;

            wrap( ()=>
            { 
                // Check to see if we're outputting to an array
                var arrayName = this.WriteContainerName(output, context);
                
                if(arrayName != null)
                    output.StartArray();

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

                if(arrayName != null)
                { 
                    output.EndArray();
                    output.WriteRaw("\r\n");
                }                    
            });
        }
    }


}
