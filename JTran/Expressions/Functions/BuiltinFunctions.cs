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
 *   Copyright (c) 2020-2023 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JTran.Extensions;

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
        public object floor(object val)
        {
            if(val == null)
                return null;

            if(!decimal.TryParse(val.ToString(), out decimal dVal))
                return null;

            return Math.Floor(dVal);
        }

        /*****************************************************************************/
        public object ceiling(object val)
        {
            if(val == null)
                return null;

            if(!decimal.TryParse(val.ToString(), out decimal dVal))
                return null;

            return Math.Ceiling(dVal);
        }

        /*****************************************************************************/
        public object round(object val)
        {
            if(val == null)
                return null;

            if(!decimal.TryParse(val.ToString(), out decimal dVal))
                return null;

            return Math.Round(dVal);
        }

        /*****************************************************************************/
        public object abs(object val)
        {
            if(val == null)
                return null;

            if(!decimal.TryParse(val.ToString(), out decimal dVal))
                return null;

            return Math.Abs(dVal);
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
        public decimal pi()
        {            
            return (decimal)Math.PI;
        }

        /*****************************************************************************/
        public object pow(object val1, object val2)
        {            
            if(val1 == null || val2 == null)
                return null;

            if(decimal.TryParse(val1.ToString(), out decimal dVal1))
                if(decimal.TryParse(val2.ToString(), out decimal dVal2))
                    return (decimal)Math.Pow((double)dVal1, (double)dVal2);

            return null;
        }

        /*****************************************************************************/
        public object sqrt(object val)
        {            
            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal dVal))
                return(decimal)Math.Sqrt((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object sin(object val)
        {            
            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal dVal))
                return(decimal)Math.Sin((double)dVal);

            return null;
        }


        /*****************************************************************************/
        public object cos(object val)
        {            
            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal dVal))
                return(decimal)Math.Cos((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object sinh(object val)
        {            
            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal dVal))
                return(decimal)Math.Sinh((double)dVal);

            return null;
        }


        /*****************************************************************************/
        public object cosh(object val)
        {            
            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal dVal))
                return(decimal)Math.Cosh((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object tan(object val)
        {            
            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal dVal))
                return(decimal)Math.Tan((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object acos(object val)
        {            
            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal dVal))
                return(decimal)Math.Acos((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object asin(object val)
        {            
            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal dVal))
                return(decimal)Math.Asin((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object atan(object val)
        {            
            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal dVal))
                return(decimal)Math.Atan((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object atan2(object val1, object val2)
        {            
            if(val1 == null || val2 == null)
                return null;

            if(decimal.TryParse(val1.ToString(), out decimal dVal1))
                if(decimal.TryParse(val2.ToString(), out decimal dVal2))
                    return (decimal)Math.Atan2((double)dVal1, (double)dVal2);

            return null;
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
        public bool contains(object val, object searchFor)
        {
            if(searchFor == null)
                return false;

            if(val is string sVal)
            { 
                var substr = searchFor.ToString();

                if(string.IsNullOrEmpty(sVal) )
                    return false;

                return sVal.Contains(substr);
            }

            if(val.IsDictionary())
            { 
                var found = val.GetPropertyValue(searchFor.ToString());

                return found != null;
            }

            if(val is IEnumerable<object> list)
                return list.Any( i=> i.CompareTo(searchFor, out Type type) == 0 );

            return false;
        }

        /*****************************************************************************/
        public string normalizespace(string val)
        {
            return val?.Trim()?.Replace("  ", " ");
        }

        /*****************************************************************************/
        public string trim(string val)
        {
            return val?.Trim();
        }

        /*****************************************************************************/
        public string trimend(string val)
        {
            return val?.TrimEnd();
        }

        /*****************************************************************************/
        public string trimstart(string val)
        {
            return val?.TrimStart();
        }
        
        /*****************************************************************************/
        public string replace(string val, string r1, string r2)
        {
            return val?.Replace(r1, r2);
        }

        /*****************************************************************************/
        public string replaceending(string val, string r1, string r2)
        {
            return val?.ReplaceEnding(r1, r2);
        }

        /*****************************************************************************/
        public string remove(string val, string r1)
        {
            return val?.Replace(r1, "");
        }

        /*****************************************************************************/
        public string removeany(string val, IEnumerable<object> list)
        {
            if(val == null)
                return null;
            
            foreach(var r1 in list)
            { 
                var newVal = val?.Replace(r1.ToString(), "");

                val = newVal;
            }

            return val;
        }

        /*****************************************************************************/
        public string padleft(string val, string padchar, int totalLen)
        {
            if(val == null || padchar == null || padchar.Length == 0)
                return val;

            return val.PadLeft(totalLen, padchar.FirstOrDefault());
        }

        /*****************************************************************************/
        public string padright(string val, string padchar, int totalLen)
        {
            if(val == null || padchar == null || padchar.Length == 0)
                return val;

            return val.PadRight(totalLen, padchar.FirstOrDefault());
        }

        /*****************************************************************************/
        public string removeending(string val, string r1)
        {
            return val?.ReplaceEnding(r1, "");
        }

        /*****************************************************************************/
        public string removeanyending(string val, object list)
        {
            foreach(var r1 in list as IEnumerable<object>)
            { 
                var newVal = val?.ReplaceEnding(r1.ToString(), "");

                if(newVal != val)
                    return newVal;
            }

            return val;
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

        /*****************************************************************************/
        public IEnumerable<string> split(string val, string separator)
        {
            if(string.IsNullOrWhiteSpace(val)) 
                return Enumerable.Empty<string>();

            return val.Split(separator).Select( s=> s.Trim() );
        }

        #endregion

        #region General Functions

        /*****************************************************************************/
        public bool not(object val)
        {
            return !System.Convert.ToBoolean(val);
        }
        
        /*****************************************************************************/
        public bool empty(object val)
        {
            if(val == null)
                return true;

            if(decimal.TryParse(val.ToString(), out decimal dValue))
                return dValue == 0M;

            if(val is IEnumerable<object> list)
                return list.Count() == 0;

            if(val is string str)
                return string.IsNullOrWhiteSpace(str);

            var dict = new Dictionary<string, object>();

            foreach (PropertyInfo item2 in from p in val.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                        where p.CanRead
                                        select p)
            {
                try
                {
                    object obj2 = item2.GetGetMethod().Invoke(val, null);

                    if (obj2 != null)
{
                        dict.Add(item2.Name, obj2);
                    }
                }
                catch
                {
                }
            }

            return dict.Count == 0;
        }
        
        /*****************************************************************************/
        public object required(object val, string errorMessage)
        {
            if(val == null)
                 throw new Transformer.UserError(errorMessage);

            if(val is IEnumerable<object> list)
            {
                if(list.Count() == 0)
                    throw new Transformer.UserError(errorMessage);

                return val;
            }

            if(string.IsNullOrWhiteSpace(val.ToString()))
                throw new Transformer.UserError(errorMessage);

            return val;
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
        public string errormessage(ExpressionContext context)
        {
            try
            { 
                return context.UserError?.Message;
            }
            catch
            {
                return "";
            }
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public string errorcode(ExpressionContext context)
        {
            try
            { 
                return context.UserError?.ErrorCode;
            }
            catch
            {
                return "";
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

        /*****************************************************************************/
        public IList<object> sequence(object from, object to)
        {
            return sequence(from, to, 1m);
        }

        /*****************************************************************************/
        public IList<object> sequence(object from, object to, object increment)
        {
            if(!decimal.TryParse(from.ToString(), out decimal dFrom))
                return new List<object>();

            if(!decimal.TryParse(to.ToString(), out decimal dTo))
                return new List<object>();

            decimal dIncrement = 1m;

            if(decimal.TryParse(increment.ToString(), out decimal dIncr) && dIncr != 0m)
                dIncrement = dIncr;

            var list = new List<object>();

            if(dIncrement > 0)
            { 
                for(decimal d = dFrom; d <= dTo; d += dIncrement)
                    list.Add(d);
            }
            else
            { 
                for(decimal d = dFrom; d >= dTo; d += dIncrement)
                    list.Add(d);
            }
             
            return list;
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public string coalesce(string primary, params string[] fields)
        {
            if(!string.IsNullOrWhiteSpace(primary))
                return primary.Trim();

            foreach(string field in fields)
                if(!string.IsNullOrWhiteSpace(field))
                    return field.Trim();
               
            return "";
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public double coalescenumber(string primary, params string[] fields)
        {
            if(double.TryParse(primary, out double d))
                if(d != 0d)
                    return d;

            foreach(string field in fields)
            {
                if(double.TryParse(field, out double d2))

                if(d2 != 0d)
                    return d2;
            }

            return 0d;
        }

        /*****************************************************************************/
        public object iif(bool condition, object first, object second)
        {
            return condition ? first : second;  
        }

        #endregion
    }
}
