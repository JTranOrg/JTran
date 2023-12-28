using System;
using System.Collections.Generic;
using System.Linq;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TSimpleArrayItem : TToken
    {
        private readonly IValue _val;

        /****************************************************************************/
        internal TSimpleArrayItem(object val) 
        {
            _val = CreateValue(val);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var value = _val.Evaluate(context);

            if(value is IEnumerable<object> list)
            {
                var numItems = list.Count();

                if(numItems > 0)
                { 
                    output.StartChild(); 

                    wrap( ()=> output.WriteList(list));
                    output.EndChild();
                }
            }
            else
                wrap( ()=> output.WriteSimpleArrayItem(value));
        }
    } 
}
