using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace JTran.Project
{
    /****************************************************************************/
    /****************************************************************************/
    public class FileIncludeRepository : IDictionary<string, string>
    {
        private readonly string _path;

        /****************************************************************************/
        public FileIncludeRepository(string path)
        {
            _path = path;
        }

        public string this[string key] 
        { 
            get { return File.ReadAllText(Path.Combine(_path, key)); }
            set { throw new NotSupportedException(); } 
            }

        public bool IsReadOnly => true;

        public bool ContainsKey(string key)
        {
            var path = Path.Combine(_path, key);

            return File.Exists(path);
        }

        #region NotSupported

        public ICollection<string> Keys => throw new NotSupportedException();

        public ICollection<string> Values => throw new NotSupportedException();

        public int Count => throw new NotSupportedException();


        public void Add(string key, string value)
        {
            throw new NotSupportedException();
        }

        public void Add(KeyValuePair<string, string> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
