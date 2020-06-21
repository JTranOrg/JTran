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

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    public class IgnoreParameterCount : Attribute
    {
        public IgnoreParameterCount()
        {
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    public class LiteralParameters : Attribute
    {
        public LiteralParameters()
        {
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class BuiltinFunctions
    {
        #region Math Functions

        /*****************************************************************************/
        public decimal floor(decimal val)
        {
            return decimal.Floor(val);
        }

        /*****************************************************************************/
        public decimal ceiling(decimal val)
        {
            return decimal.Ceiling(val);
        }

        /*****************************************************************************/
        public decimal round(decimal val)
        {
            return decimal.Round(val);
        }

        /*****************************************************************************/
        public decimal number(object val)
        {
            return decimal.Parse(val.ToString());
        }

        #endregion

        #region String Functions

        /*****************************************************************************/
        public object ___string(object val)
        {
            if(val == null)
                return null;

            return new StringValue(val?.ToString());
        }

        /*****************************************************************************/
        public string substring(string val, int start, int? length = null)
        {
            if(length.HasValue)
                return val.Substring(start, length.Value);

            return val.Substring(start);
        }

        /*****************************************************************************/
        public string substringafter(string val, string substr)
        {
            if(string.IsNullOrEmpty(val) || string.IsNullOrEmpty(substr))
                return val;

            var index = val.IndexOf(substr);

            if(index == -1)
                return "";
 
            return val.Substring(index + substr.Length);
        }

        /*****************************************************************************/
        public string substringbefore(string val, string substr)
        {
            if(string.IsNullOrEmpty(val) || string.IsNullOrEmpty(substr))
                return val;

            var index = val.IndexOf(substr);

            if(index == -1)
                return "";
 
            return val.Substring(0, index);
        }

        /*****************************************************************************/
        public bool startswith(string val, string substr)
        {
            if(string.IsNullOrEmpty(val) || string.IsNullOrEmpty(substr))
                return false;

            return val.StartsWith(substr);
        }

        /*****************************************************************************/
        public bool endswith(string val, string substr)
        {
            if(string.IsNullOrEmpty(val) || string.IsNullOrEmpty(substr))
                return false;

            return val.EndsWith(substr);
        }

        /*****************************************************************************/
        public bool contains(string val, string substr)
        {
            if(string.IsNullOrEmpty(val) || string.IsNullOrEmpty(substr))
                return false;

            return val.Contains(substr);
        }

        /*****************************************************************************/
        public string normalizespace(string val)
        {
            return val?.Trim()?.Replace("  ", " ");
        }

        /*****************************************************************************/
        public int stringlength(string val)
        {
            return val?.Length ?? 0;
        }

        /*****************************************************************************/
        public int indexof(string val, string substr)
        {
            return val?.IndexOf(substr) ?? -1;
        }

        #endregion

        #region Aggregate/Array Functions

        /*****************************************************************************/
        public int count(object val)
        {
            if(val is null)
                return 0;

            if(val is IList<object> list)
                return list.Count;

            return 1;
        }

        /*****************************************************************************/
        public decimal sum(object val)
        {
            if(val is null)
                return 0M;

            if(val is IList<object> list)
            { 
                decimal sum = 0M;

                foreach(var item in list)
                    if(decimal.TryParse(item.ToString(), out decimal dval))
                        sum += dval;

                return sum;
            }

            if(decimal.TryParse(val.ToString(), out decimal dval2))
                return dval2;

            return 0M;
        }

        #endregion

        #region General Functions

        /*****************************************************************************/
        public bool not(object val)
        {
            return !Convert.ToBoolean(val);
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public long position(ExpressionContext context)
        {
            try
            { 
                dynamic dyn      = context.Data;
                long    position = dyn._jtran_position;

                return position;
            }
            catch
            {
                return 0;
            }
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        [LiteralParameters]
        public object document(string repoName, string docName, ExpressionContext context)
        {
            return context.GetDocument(repoName, docName);
        }


        #endregion

    }
}
