using JTran.Common;
using JTran.Expressions;
using System;
using System.Collections.Generic;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal interface IJsonToken
    {
        object? Parent { get; }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class JsonArray : List<object>, IJsonToken
    {
        public JsonArray(object? parent) 
        { 
            this.Parent = parent;
        }

        public object? Parent { get; }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class JsonObject : Dictionary<ICharacterSpan, object>, IObject, IJsonToken
    {
        /****************************************************************************/
        internal JsonObject(object? parent, ICharacterSpan? name = null) 
        { 
            this.Parent = parent;
            this.Name = name;
        }

        public ICharacterSpan? Name   { get; }
        public object?         Parent { get; set; }

        public object this[string key] => this[CharacterSpan.FromString(key)];

        /****************************************************************************/
        public object? GetPropertyValue(ICharacterSpan name)
        {
            if(this.ContainsKey(name))
                return this[name];

            return null;
        }
        
        /****************************************************************************/
        public void ForEachProperty(Action<ICharacterSpan, object> onProperty)
        {
            foreach(var kv in this)
            {
                onProperty(kv.Key, kv.Value);
            }
        }
    }
}
