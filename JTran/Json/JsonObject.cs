using JTran.Common;
using JTran.Expressions;
using System;
using System.Collections.Generic;

namespace JTran
{
    internal class JsonObject : Dictionary<CharacterSpan, object>, IObject
    {
        internal JsonObject() 
        { 
        }

        public object this[string key] => this[CharacterSpan.FromString(key)];

        public object? GetPropertyValue(CharacterSpan name)
        {
            if(this.ContainsKey(name))
                return this[name];

            return null;
        }
    }
}
