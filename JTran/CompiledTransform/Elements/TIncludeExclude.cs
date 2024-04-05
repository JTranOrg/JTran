
using JTran.Common;
using System;
using System.Threading.Tasks;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TIncludeExclude : TToken
    {
        private readonly IValue? _name;
        private readonly TToken _property;

        /****************************************************************************/
        internal TIncludeExclude(ICharacterSpan? name, ICharacterSpan val, bool include, long lineNumber) 
        {
            _name = (name?.Equals("#noobject") ?? false) ? null : CreateValue(name, true, 0);

            _property = new TIncludeExcludeProperty(val, include, lineNumber);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            ICharacterSpan? name = null;
            bool written = false;

            wrap( ()=>
            { 
                _property.Evaluate(writer, context, fn=>
                { 
                    name = _name?.Evaluate(context) as ICharacterSpan;
                    written = true;

                    if(name != null)
                        writer.WriteContainerName(name);

                    writer.StartObject();
                    fn();
                    writer.EndObject();
                });

                if(!written && name != null)
                    writer.WriteProperty(name, null);
            });
        }
    }
}
