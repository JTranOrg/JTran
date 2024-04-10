using System.Collections.Generic;

namespace JTran.Project
{
    /****************************************************************************/
    /****************************************************************************/
    internal static class DictionaryExtensions
    {
        /****************************************************************************/
        internal static IDictionary<K, V> Merge<K, V>(this IDictionary<K, V>? dict1, IDictionary<K, V>? dict2)
        {
            if(dict2 == null || dict2.Count == 0)
                return dict1;
       
            if(dict1 == null)
                return dict2;
       
            foreach(var kv in dict2)
                dict1[kv.Key] = kv.Value;

            return dict1;
        }
    }
}
