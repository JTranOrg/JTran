using JTran.Common;
using JTran.Expressions;
using System.Collections.Generic;

namespace JTran
{
    internal class JsonObject : Dictionary<CharacterSpan, object>
    {
        internal JsonObject() 
        { 
        }

        public object this[string key] => this[CharacterSpan.FromString(key)];
    }
}
