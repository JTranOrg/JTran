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
 *   Copyright (c) 2020 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal class AggregateFunctions
    {
        /*****************************************************************************/
        public int count(object val)
        {
            if(val is null)
                return 0;

            if(val is ExpandoObject)
                return 1;

            if(val is IList<object> list)
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

            if(val is IList<object> list)
                return list.Reverse().ToList();

            return new String(val.ToString().Reverse().ToArray());
        }

        #region Private Methods

        /*****************************************************************************/
        private static decimal aggregate(object val, Func<decimal, decimal, decimal> fn, decimal startVal, out int count)
        {
            count = 1;

            if(val is null)
                return 0M;

            if(val is IList<object> list)
            { 
                decimal result = startVal;

                foreach(var item in list)
                    if(decimal.TryParse(item.ToString(), out decimal dval))
                        result = fn(result, dval);

                count = list.Count;

                return result;
            }

            if(decimal.TryParse(val.ToString(), out decimal dval2))
                return dval2;

            return 0M;
        }

        #endregion
    }
}
