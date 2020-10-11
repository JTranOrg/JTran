using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JTran
{
    internal class TemplateRepository : IDictionary<string, TTemplate>
    {
        private readonly IDictionary<string, TTemplate> _cache = new Dictionary<string, TTemplate>();

        internal TemplateRepository()
        {

        }

        #region IDictionary

        public TTemplate this[string key] 
        { 
            get 
            {
                if(_cache.ContainsKey(key))
                    return _cache[key];

                    return null;
            }
            
            set => throw new NotSupportedException(); 
        }

        public bool IsReadOnly => true;

        public bool Contains(KeyValuePair<string, TTemplate> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, TTemplate>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out TTemplate value)
        {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #region Not Supported

        public ICollection<string> Keys => throw new NotSupportedException();

        public ICollection<TTemplate> Values => throw new NotSupportedException();

        public int Count => throw new NotSupportedException();

        public void CopyTo(KeyValuePair<string, TTemplate>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public void Add(string key, TTemplate value)
        {
            throw new NotSupportedException();
        }

        public void Add(KeyValuePair<string, TTemplate> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<string, TTemplate> item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #endregion
    }
}
