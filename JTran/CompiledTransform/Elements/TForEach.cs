
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JTran.Extensions;
using JTran.Expressions;
using JTran.Parser;
using JTran.Common;
using JTran.Collections;

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

            var list = result.EnsureEnumerable();

            _hasConditionals = this.Children.Any() && this.Children[0] is IPropertyCondition;
      
            wrap( ()=> 
            { 
                var arrayName   = WriteContainerName(output, context);
                var outputArray = this.IsOutputArray || (arrayName != null && !arrayName.Equals(EmptyObject));

                if(outputArray)
                    output.StartArray();
                
                EvaluateChildren(output, arrayName, list, context);

                if(outputArray)
                    output.EndArray();
            });
        }

        /****************************************************************************/
        private void EvaluateChildren(IJsonWriter output, ICharacterSpan arrayName, IEnumerable list, ExpressionContext context)
        {
            try
            { 
                var index = 0L;

                if(list is IEnumerable<object> enm && enm.IsPocoList(out Type? type))
                    list = new PocoEnumerableWrapper(type!, enm);

                foreach(var childScope in list) 
                { 
                    if(EvaluateChild(output, arrayName, childScope, context, ref index))
                        break;

                    if(context.MaxIterations == 1 && output.HasWritten) 
                        break;
                }
            }
            catch(AggregateException ex)
            {
                throw ex.InnerException!;
            }
        }

        /****************************************************************************/
        private bool EvaluateChild(IJsonWriter output, ICharacterSpan arrayName, object childScope, ExpressionContext context, ref long index)
        {
            var newContext = new ExpressionContext(childScope, context, templates: this.Templates, functions: this.Functions) { Index = index };
            var maxIterations = context.MaxIterations;

            newContext.MaxIterations = 1;

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

            newContext.MaxIterations = maxIterations;
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
        private readonly IExpression? _groupBy;

        /****************************************************************************/
        internal TForEachGroup(ICharacterSpan name) 
        {
            var parms = CompiledTransform.ParseElementParams("#foreachgroup", name, CompiledTransform.FalseFalseTrue )!;

            _expression = parms![0];
            
            if(parms.Count > 2)
                SetName(parms.Last());

            if(parms.Count > 1)
                _groupBy = parms[1];
        }

        /****************************************************************************/
        internal protected TForEachGroup(ICharacterSpan expression, bool expr) 
        {
            _expression = Compiler.Compile(expression);
        }

        /****************************************************************************/
        private class GroupByComparer : IEqualityComparer<GroupKey>
        {
            private readonly IEnumerable<ICharacterSpan> _fields; 

            public GroupByComparer(IEnumerable<ICharacterSpan> fields) 
            {
                _fields = fields;
            }

            public bool Equals(GroupKey x, GroupKey y)
            {
                foreach(var field in _fields)
                {
                    var compare = x[field].CompareTo(y[field]); 

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
            if(!(result is IEnumerable<object> enm))
            { 
                enm = new List<object> { result };
            }

            // Get the groups
            IEnumerable<JsonObject>? groups;
            var newContext = new ExpressionContext(null, context);
            var groupNames = ((_groupBy is ArrayExpression array) ? array.SubExpressions.Select( s=> s.AsCharacterSpan(true)) : new[] {_groupBy!.AsCharacterSpan(true) }).ToList();

            var list = enm.ToList(); // No way to avoid loading the entire thing into memory

            groups = list.GroupBy
            (
                (item)=> item.GetGroupByKey(_groupBy!, newContext, groupNames),
                (item)=> item,
                (groupValue, items) => {    
                                            var newObj = new JsonObject(context.Data is IJsonToken token ? token.Parent : null);

                                            foreach(var item in groupValue)
                                                newObj.TryAdd(item.Key.LastItemIn('.'), item.Value);

                                            newObj.TryAdd(_groupItems, items); 
                                                    
                                            return newObj;
                                       },
                new GroupByComparer(groupNames)
            );

            if(!groups.Any())
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
                }                    
            });
        }
    }


}
