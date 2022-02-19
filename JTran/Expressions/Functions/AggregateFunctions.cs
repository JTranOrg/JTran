/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: BuiltinFunctions.cs					    		        
 *        Class(es): BuiltinFunctions
 *          Purpose: All of the built in functions                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 18 Jun 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2022 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using JTran.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

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

            if(val is ExpandoObject)
                return 1;

            if(val is ICollection coll)
                return coll.Count;

            if(val is ICollection<object> list)
                return list.Count;

            return 1;
        }

        /*****************************************************************************/
        public bool any(object val)
        {
            return count(val) > 0;
        }

        /*****************************************************************************/
        public decimal sum(object val)
        {
            return aggregate(val, (result, dval)=> result + dval, 0M, out int count );
        }

       /*****************************************************************************/
        public decimal avg(object val)
        {
            return aggregate(val, (result, dval)=> result + dval, 0M, out int count ) / count;
        }

        /*****************************************************************************/
        public decimal max(object val)
        {
            return aggregate(val, (result, dval)=> Math.Max(result, dval), 0M, out int count );
        }

        /*****************************************************************************/
        public decimal min(object val)
        {
            var retVal = aggregate(val, (result, dval)=> Math.Min(result, dval), decimal.MaxValue, out int count );

            if(count == 0)
                return 0M;

            return retVal;
        }

        /*****************************************************************************/
        public object reverse(object val)
        {
            if(val is null)
                return null;

            if(val is IEnumerable<object> list)
                return list.Reverse().ToList();

            return new String(val.ToString().Reverse().ToArray());
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public object sort(object expr, params string[] sortFields)
        {
            if(expr is null)
                return null;

            if(expr is IEnumerable<object> list)
            {
                if(list.Count() == 1 || sortFields.Length == 0)
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
                for(var i = 0; i < sortFields.Length; i += 2)
                {
                    var field = new SortField { Name = sortFields[i] };

                    if(sortFields.Length-1 > i)
                        field.Ascending = sortFields[i+1].ToLower() == "asc";

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
                internal string Name      { get; set; }
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
                return 0M;

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

            return 0M;
        }

        #endregion
    }
}
