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
using JTran.Extensions;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal class AggregateFunctions
    {
        private readonly IDictionary<string, IComparer<object>> _sortComparers = new Dictionary<string, IComparer<object>>();

        /*****************************************************************************/
        public int count(object val)
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

            return new String(val.ToString().Reverse().ToArray());
        }

        /*****************************************************************************/
        public object? last(object val)
        {
            if(val is null)
                return null;

            if(val is IEnumerable<object> list)
                return list.LastOrDefault();

            return val;
        }

        /*****************************************************************************/
        public object? first(object val)
        {
            if(val is null)
                return null;

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
                    union.Add(parm.EnsureEnumerable());
                }
            }

            return union;
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public object? sort(object expr, params string[] sortFields)
        {
            if(expr is null)
                return null;

            if(expr is IEnumerable<object> list)
            {
                if(list.IsSingle() || sortFields.Length == 0)
                    return list;

                var copy = new List<object>(list);

                copy.Sort(GetComparer(sortFields));

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

            /*****************************************************************************/
            internal SortComparer(string[] sortFields)
            {
                foreach(var sortField in sortFields)
                {
                    if(sortField == "asc")
                    {
                        _sortFields.Last().Ascending = true;
                        continue;
                    }

                    if(sortField == "desc")
                    {
                        _sortFields.Last().Ascending = false;
                        continue;
                    }

                    var field = new SortField { Name = sortField.Trim(), Ascending = true };

                    if(field.Name.EndsWith(" desc"))
                    { 
                        field.Ascending = false;
                        field.Name = field.Name.Substring(0, field.Name.Length - 4).Trim();
                    }
                    else if(field.Name.EndsWith(" asc"))
                    { 
                        field.Name = field.Name.Substring(0, field.Name.Length - 3).Trim();
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
                    var result = sortField.Ascending ? xVal.CompareTo(yVal, out Type type) : yVal.CompareTo(xVal, out Type type2);

                    if(result != 0)
                        return result;
                }

                return 0;
            }

            /*****************************************************************************/
            private class SortField
            {
                internal string Name      { get; set; } = "";
                internal bool   Ascending { get; set; } = true;
            }
        }

        /*****************************************************************************/
        private IComparer<object> GetComparer(string[] sortFields)
        {
            var key = string.Join("", sortFields);

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

            if(val is IEnumerable<object> list)
            { 
                decimal result = startVal;

                foreach(var item in list)
                    if(decimal.TryParse(item.ToString(), out decimal dval))
                        result = fn(result, dval);

                count = list.Count();

                return result;
            }

            if(decimal.TryParse(val.ToString(), out decimal dval2))
                return dval2;

            return 0m;
        }

        #endregion
    }
}
