using System;
using System.Dynamic;

using JTran.Json;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal abstract class TToken
    {
        public abstract void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap);

        /****************************************************************************/
        internal protected IValue CreateValue(object value)
        {
            return CreateValue(value?.ToString() ?? "");
        }  
        
        /****************************************************************************/
        private IValue CreateValue(string sval)
        {
            if(!sval.StartsWith("#("))
            { 
                if(decimal.TryParse(sval, out decimal val))
                    return new NumberValue(val);

                return new SimpleValue(sval);
            }

            if(!sval.EndsWith(")"))
                throw new Transformer.SyntaxException("Missing closing parenthesis");

            var expr = sval.Substring(2, sval.Length - 3);

            return new ExpressionValue(expr);
        }   
    }
}
