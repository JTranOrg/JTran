using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JTran.Common;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TSimpleArrayItem : TToken
    {
        private readonly IValue _val;

        /****************************************************************************/
        internal TSimpleArrayItem(ICharacterSpan? val, long lineNumber) 
        {
            _val = CreateValue(val, false, lineNumber);
            
            this.IsOutput = true;
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            var value = _val.Evaluate(context);

            if(value is IEnumerable<object> list)
            {
                if(list.Any())
                { 
                    writer.StartChild(); 

                    wrap( ()=> writer.WriteList(list));

                    writer.EndChild();
                }
            }
            else
                wrap( ()=> writer.WriteSimpleArrayItem(value));
        }
    } 
}
