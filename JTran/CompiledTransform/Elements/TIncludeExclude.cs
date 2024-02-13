
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TIncludeExclude : TToken
    {
        private readonly IValue? _name;
        private readonly TToken _property;

        /****************************************************************************/
        internal TIncludeExclude(string? name, string val, bool include, long lineNumber) 
        {
            _name = name == null || name == "#noobject" ? null : CreateValue(name, true, 0);
            _property = new TIncludeExcludeProperty(val, include, lineNumber);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter writer, ExpressionContext context, Action<Action> wrap)
        {
            string? name = null;
            bool written = false;

            wrap( ()=>
            { 
                _property.Evaluate(writer, context, fn=>
                { 
                    name = _name?.Evaluate(context)?.ToString();
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
