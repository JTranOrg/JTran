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

using JTran.Extensions;
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
        public decimal? number(object val)
        {
            if(val != null)
                if(decimal.TryParse(val.ToString(), out decimal result))
                    return result;

            return null;
        }

        /*****************************************************************************/
        public bool isnumber(object val)
        {
            return decimal.TryParse(val.ToString(), out decimal result);
        }

        /*****************************************************************************/
        public bool isinteger(object val)
        {
            return long.TryParse(val.ToString(), out long result);
        }

        /*****************************************************************************/
        public object max(object val1, object val2)
        {
            var result = val1.CompareTo(val2, out Type type) == 1 ? val1 : val2;
            
            return Convert(result, type);
        }

        /*****************************************************************************/
        public object min(object val1, object val2)
        {
            var result = val1.CompareTo(val2, out Type type) == -1 ? val1 : val2;
            
            return Convert(result, type);
        }
        
        /*****************************************************************************/
        private object Convert(object result, Type type)
        {
            switch(type.Name)
            {
                case "Long":      return long.Parse(result.ToString());
                case "Decimal":   return decimal.Parse(result.ToString());
                case "Boolean":   return bool.Parse(result.ToString());
                case "String":    return result.ToString();

                case "DateTime":  
                { 
                    result.TryParseDateTime(out DateTime? dtValue);
                    if(dtValue.HasValue)
                        return dtValue.Value.ToString("o");

                    return null;
                }

                default:          
                return result;
            }
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
        public string lowercase(object val)
        {
            if(val == null)
                return null;

            return val?.ToString()?.ToLower();
        }

        /*****************************************************************************/
        public string uppercase(object val)
        {
            if(val == null)
                return null;

            return val?.ToString()?.ToUpper();
        }

        /*****************************************************************************/
        public string substring(string val, int start, int length)
        {
            return val.Substring(start, length);
        }

        /*****************************************************************************/
        public string substring(string val, int start)
        {
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

        #region General Functions

        /*****************************************************************************/
        public bool not(object val)
        {
            return !System.Convert.ToBoolean(val);
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

        /*****************************************************************************/
        [IgnoreParameterCount]
        public IList<object> currentgroup(ExpressionContext context)
        {
            return context.CurrentGroup;
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public string name(ExpressionContext context)
        {
            try
            { 
                dynamic dyn      = context.Data;
                string  name = dyn._jtran_name;

                return name;
            }
            catch
            {
                return "";
            }
        }

        #endregion
    }
}
