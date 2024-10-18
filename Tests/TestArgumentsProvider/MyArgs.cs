using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace TestArgumentsProvider
{
    public class MyArgs : Dictionary<string, object>
    {
        public MyArgs() 
        {
            this.Add("Phrase", "bobs your uncle");
        }
    }    
    
    public class MyArgs2 : IReadOnlyDictionary<string, object>
    {
        public MyArgs2() 
        {
        }

        public object this[string key]
        {
            get
            {
                switch(key) 
                {
                   case "Phrase": return  "bobs your uncle";
                   default: throw new KeyNotFoundException();  
                }
            }
        }

        public IEnumerable<string> Keys => new List<string> { "Phrase" };

        public IEnumerable<object> Values =>  new List<string> { "bobs your uncle" };

        public int Count => 1;

        public bool ContainsKey(string key)
        {
            return key == "Phrase";
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch(key) 
            {
                case "Phrase": value = "bobs your uncle"; return true;
                default: value = null; return false;  
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    
    public class MyArgs3 : IDictionary<string, object>
    {
        public MyArgs3() 
        {
        }

        public object this[string key]
        {
            get
            {
                switch(key) 
                {
                   case "Phrase": return  "bobs your uncle";
                   default: throw new KeyNotFoundException();  
                }
            }
        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                switch(key) 
                {
                   case "Phrase": return  "bobs your uncle";
                   default: throw new KeyNotFoundException();  
                }
            }

            set => throw new NotSupportedException();
        }

        public IEnumerable<string> Keys => new List<string> { "Phrase" };

        public IEnumerable<object> Values =>  new List<string> { "bobs your uncle" };

        public int Count => 1;

        public bool IsReadOnly => false;

        ICollection<string> IDictionary<string, object>.Keys => new List<string> { "Phrase" };

        ICollection<object> IDictionary<string, object>.Values => new List<object> { "bobs your uncle" };

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return key == "Phrase";
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch(key) 
            {
                case "Phrase": value = "bobs your uncle"; return true;
                default: value = null; return false;  
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
