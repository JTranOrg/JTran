/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: BuiltinFunctions.cs					    		        
 *        Class(es): BuiltinFunctions
 *          Purpose: All of the built in functions                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 18 Jun 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using JTran.Collections;
using JTran.Common;
using JTran.Extensions;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal class AggregateFunctions
    {
        private readonly Dictionary<ICharacterSpan, IComparer<object>> _sortComparers = new();

        /*****************************************************************************/
        public int count(object val)
        {
            return count_internal(val);
        }

        /*****************************************************************************/
        internal static int count_internal(object val)
        {
            if(val is null)
                return 0;

            if(val is JsonObject)
                return 1;

            if(val is ICollection coll)
                return coll.Count;

            if(val is ICollection<object> list)
                return list.Count;

            if(val is IEnumerable<object> enm)
                return enm.Count();

            if(val is IEnumerable<string> enm2)
                return enm2.Count();

            if(val is IEnumerable list2)
            {
                var count = 0;

                foreach(var item in list2)
                    ++count;

                return count;
            }

            return 1;
        }

        /*****************************************************************************/
        public bool any(object val)
        {
            if(val is null)
                return false;

            if(val is JsonObject)
                return true;

            if(val is IEnumerable<object> enm)
                return enm.Any();

            return true;
        }

        /*****************************************************************************/
        public decimal sum(object val)
        {
            return aggregate(val, (result, dval)=> result + dval, 0m, out int count );
        }

       /*****************************************************************************/
        public decimal avg(object val)
        {
            return aggregate(val, (result, dval)=> result + dval, 0m, out int count ) / count;
        }

        /*****************************************************************************/
        public decimal max(object val)
        {
            return aggregate(val, (result, dval)=> Math.Max(result, dval), 0m, out int count );
        }

        /*****************************************************************************/
        public decimal min(object val)
        {
            var retVal = aggregate(val, (result, dval)=> Math.Min(result, dval), decimal.MaxValue, out int count );

            if(count == 0)
                return 0m;

            return retVal;
        }

        /*****************************************************************************/
        public object? reverse(object val)
        {
            if(val is null)
                return null;

            if(val is IEnumerable<object> list)
                return list.Reverse();

            if(val is ICharacterSpan cspan)
                return new CharacterSpan(cspan.Reverse().ToArray(), 0, -1, cspan.HasEscapeCharacters);

            if(val is IEnumerable<char> enm)
                return new CharacterSpan(enm.Reverse().ToArray(), 0);

            return new String(val.ToString().Reverse().ToArray());
        }

        /*****************************************************************************/
        public object? last(object val)
        {
            if(val is null)
                return null;

            if(val is ICharacterSpan cspan)
                return cspan.Length == 0 ? '\0' : cspan[cspan.Length - 1];

            if(val is IEnumerable<object> list)
                return list.LastOrDefault();

            return val;
        }

        /*****************************************************************************/
        public object? first(object val)
        {
            if(val is null)
                return null;

            if(val is ICharacterSpan cspan)
                return cspan.Length == 0 ? '\0' : cspan[0];

            if(val is IEnumerable<object> list)
                return list.FirstOrDefault();

            return val;
        }

        /*****************************************************************************/
        public string? join(object val, string separator)
        {
            if(val is null)
                return null;

            if(val is IEnumerable<object> list)
                return string.Join(separator, list);

            return val.ToString();
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public object? union(params object[] lists)
        {
            var union = new Union<object>();

            foreach(var parm in lists) 
            {
                if(parm != null)
                { 
                    union.Add(parm.EnsureObjectEnumerable());
                }
            }

            return union;
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public object? sort(object expr, params object?[] sortFields) 
        {
            if(expr is null)
                return null;

            if(expr is IEnumerable<object> list)
            {
                if(list.IsSingle() || sortFields.Length == 0)
                    return list;

                var chSortFields = sortFields.Where(f=> f != null).Select(f=> f!.AsCharacterSpan(true));
                var copy = new List<object>(list);

                copy.Sort(GetComparer(chSortFields));

                return copy;
            }

            return expr;
        }

        #region Private Methods

        /*****************************************************************************/
        /*****************************************************************************/
        private class SortComparer : IComparer<object>
        {
            private readonly IList<SortField> _sortFields = new List<SortField>();

            private static ICharacterSpan _asc        = CharacterSpan.FromString("asc");
            private static ICharacterSpan _desc       = CharacterSpan.FromString("desc");
            private static ICharacterSpan _ascSpaced  = CharacterSpan.FromString(" asc");
            private static ICharacterSpan _descSpaced = CharacterSpan.FromString(" desc");

            /*****************************************************************************/
            internal SortComparer(IEnumerable<ICharacterSpan> sortFields)
            {
                foreach(var sortField in sortFields)
                {
                    if(sortField.Equals(_asc))
                    {
                        _sortFields.Last().Ascending = true;
                        continue;
                    }

                    if(sortField.Equals(_desc))
                    {
                        _sortFields.Last().Ascending = false;
                        continue;
                    }

                    var field = new SortField { Name = sortField, Ascending = true };

                    if(field.Name.EndsWith(_descSpaced))
                    { 
                        field.Ascending = false;
                        field.Name = field.Name.Substring(0, field.Name.Length - 4, trim: true);
                    }
                    else if(field.Name.EndsWith(_ascSpaced))
                    { 
                        field.Name = field.Name.Substring(0, field.Name.Length - 3, trim: true);
                    }

                    _sortFields.Add(field);
                }
            }

            /*****************************************************************************/
            public int Compare(object x, object y)
            {
                foreach(var sortField in _sortFields)
                {
                    var xVal   = x.GetPropertyValue(sortField.Name);
                    var yVal   = y.GetPropertyValue(sortField.Name);
                    var result = sortField.Ascending ? xVal.CompareTo(yVal) : yVal.CompareTo(xVal);

                    if(result != 0)
                        return result;
                }

                return 0;
            }

            /*****************************************************************************/
            private class SortField
            {
                internal ICharacterSpan Name      { get; set; } = CharacterSpan.Empty;
                internal bool           Ascending { get; set; } = true;
            }
        }

        /*****************************************************************************/
        private IComparer<object> GetComparer(IEnumerable<ICharacterSpan> sortFields)
        {
            var key = CharacterSpan.Join(sortFields, '_');

            if(_sortComparers.ContainsKey(key))
                return _sortComparers[key];

            var comparer = new SortComparer(sortFields);

            _sortComparers.Add(key, comparer);

            return comparer;
        }

        /*****************************************************************************/
        private static decimal aggregate(object val, Func<decimal, decimal, decimal> fn, decimal startVal, out int count)
        {
            count = 1;

            if(val is null)
                return 0m;

            if(!(val is ICharacterSpan) && val is not string)
            {
                if(val is IEnumerable<object> list)
                { 
                    decimal result = startVal;

                    foreach(var item in list)
                    { 
                        if(item.TryParseDecimal(out decimal dval))
                            result = fn(result, dval);
                    }

                    count = list.Count();

                    return result;
                }
            }

            if(val.TryParseDecimal(out decimal dval2))
                return dval2;

            return 0m;
        }

        #endregion
    }
}
