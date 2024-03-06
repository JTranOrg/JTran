
using System;
using System.Collections.Generic;
using System.Linq;

using JTran.Extensions;
using JTran.Expressions;
using JTran.Parser;
using JTran.Common;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TForEach : TBaseArray
    {
        private readonly IExpression _expression;
        private bool _hasConditionals = false;        

        /****************************************************************************/
        internal TForEach(ICharacterSpan name, long lineNumber) 
        {
            var parms = CompiledTransform.ParseElementParams("#foreach", name, CompiledTransform.FalseTrue );

            if(parms.Count < 1)
                throw new Transformer.SyntaxException("Missing expression for #foreach") { LineNumber = lineNumber };

            _expression = parms[0];

            if(parms.Count > 1)
                SetName(parms[1]);
        }

        /****************************************************************************/
        internal protected TForEach(ICharacterSpan expression, bool expr) 
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

            _hasConditionals = this.Children.Any() && this.Children[0] is IPropertyCondition;

            // If the result of the expression is an array.
            if(result is IEnumerable<object> list) // ??? check for poco array
            {       
                wrap( ()=> 
                { 
                    var arrayName = WriteContainerName(output, context);

                    if(this.IsOutputArray || (arrayName != null && !arrayName.Equals(EmptyObject)))
                        output.StartArray();
                
                    EvaluateChildren(output, arrayName, list, context);

                    if(this.IsOutputArray || (arrayName != null && !arrayName.Equals(EmptyObject)))
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
        private void EvaluateChildren(IJsonWriter output, ICharacterSpan arrayName, IEnumerable<object> list, ExpressionContext context)
        {
            try
            { 
                var index = 0L;

                foreach(var childScope in list)
                { 
                    if(EvaluateChild(output, arrayName, childScope, context, ref index))
                        break;
                }
            }
            catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        /****************************************************************************/
        private bool EvaluateChild(IJsonWriter output, ICharacterSpan arrayName, object childScope, ExpressionContext context, ref long index)
        {
            var newContext = new ExpressionContext(childScope, context, templates: this.Templates, functions: this.Functions) { Index = index };

            if(_hasConditionals)
            {
                try
                {
                    if(this.Children.EvaluateConditionals(newContext, out object result))
                    { 
                        output.WriteItem(result);
                    }
                
                    return false;
                }
                catch(Break)
                {
                    return true;
                }
            }

            try
            { 
                // First test if the child will ever write anything at all
                var testWriter = new JsonTestWriter();

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

            var bBreak = false;

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

    /****************************************************************************/
    /****************************************************************************/
    internal class TForEachGroup : TBaseArray
    {
        private readonly IExpression  _expression;
        private readonly IEnumerable<string>? _groupBy;

        /****************************************************************************/
        internal TForEachGroup(ICharacterSpan name) 
        {
            var parms = CompiledTransform.ParseElementParams("#foreachgroup", name, CompiledTransform.FalseTrue )!;

            _expression = parms![0];
            
            if(parms.Count > 2)
                SetName(parms.Last());

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
        internal protected TForEachGroup(ICharacterSpan expression, bool expr) 
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

        private static readonly ICharacterSpan _groupItems = CharacterSpan.FromString("__groupItems");

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
            IEnumerable<JsonObject>? groups;
            var csGroup = new CharacterSpanGroup();

            if(_groupBy.IsSingle())
            {
                var groupBy = _groupBy.First();
                var dict = new Dictionary<string, List<object>>();

                foreach(var item in list)
                {
                    var key = item.GetSingleValue(groupBy, null)?.ToString();

                    if(key != null)
                    {
                        if(!dict.ContainsKey(key))
                            dict.Add(key, new List<object> { item});
                        else
                            dict[key].Add(item);                            
                    }
                }

                groups = dict.Select( kv=>  {    
                                                var newObj = new JsonObject(null);// ??? parent

                                                newObj.TryAdd(csGroup.Get(groupBy), kv.Key);
                                                newObj.TryAdd(_groupItems, kv.Value); 
                                                    
                                                return newObj;
                                            } );
            }
            else
            { 
                groups = list.GroupBy
                (
                    (item)=> item.GetGroupByKey(_groupBy!),
                    (item)=> item,
                    (groupValue, items) => {    
                                                var newObj = new JsonObject(null); // ??? parent

                                                foreach(var item in groupValue)
                                                    newObj.TryAdd(csGroup.Get(item.Key), item.Value);

                                                newObj.TryAdd(_groupItems, items); 
                                                    
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

                    newContext.CurrentGroup = groupScope[_groupItems] as IList<object>;

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
                    output.WriteRaw(CharacterSpan.Empty);
                }                    
            });
        }
    }


}
