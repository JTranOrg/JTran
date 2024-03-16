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

using JTran.Common;
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
        public object? floor(object val)
        {
            if(val == null)
                return null;

            if(!decimal.TryParse(val.ToString(), out decimal dVal))
                return null;

            return Math.Floor(dVal);
        }

        /*****************************************************************************/
        public object? ceiling(object val)
        {
            if(val == null)
                return null;

            if(!decimal.TryParse(val.ToString(), out decimal dVal))
                return null;

            return Math.Ceiling(dVal);
        }

        /*****************************************************************************/
        public object? round(object val)
        {
            if(val == null)
                return null;

            if(!decimal.TryParse(val.ToString(), out decimal dVal))
                return null;

            return Math.Round(dVal);
        }

        /*****************************************************************************/
        public object? precision(object val, int numPlaces)
        {
            if(val == null || numPlaces < 1)
                return null;

            if(!decimal.TryParse(val.ToString(), out decimal dVal))
                return null;

            var multiplier = (decimal)Math.Pow(10, numPlaces);

            return Math.Floor(dVal * multiplier) / multiplier;
        }

        /*****************************************************************************/
        public object? abs(object val)
        {
            if(val == null)
                return null;

            if(!decimal.TryParse(val.ToString(), out decimal dVal))
                return null;

            return Math.Abs(dVal);
        }

        /*****************************************************************************/
        public object? number(object val)
        {
            if(val == null)
                return null;

            if(decimal.TryParse(val.ToString(), out decimal result))
                return result;

            return 0d;
        }

        /*****************************************************************************/
        public bool isnumber(object val)
        {
            if(val == null)
                return false;

            return decimal.TryParse(val.ToString(), out decimal result);
        }

        /*****************************************************************************/
        public bool? isinteger(object? val)
        {
            if(val == null)
                return false;

            if(val is long)
                return true;

            if(val is int)
                return true;

            return long.TryParse(val.ToString(), out long result);
        }

        /*****************************************************************************/
        public object? max(object val1, object val2)
        {
            return JTran.Extensions.ObjectExtensions.Compare(val1, val2, out object? t1, out object? t2) == 1 ? t1 : t2;
        }

        /*****************************************************************************/
        public object? min(object val1, object val2)
        {
            return JTran.Extensions.ObjectExtensions.Compare(val1, val2, out object? t1, out object? t2) == -1 ? t1 : t2;
        }
        
        /*****************************************************************************/
        public decimal pi()
        {            
            return (decimal)Math.PI;
        }

        /*****************************************************************************/
        public object? pow(object val1, object val2)
        {            
            if(val1 == null || val2 == null)
                return null;

            if(val1.TryParseDecimal(out decimal dVal1) && val2.TryParseDecimal(out decimal dVal2))
                return (decimal)Math.Pow((double)dVal1, (double)dVal2);

            return null;
        }

        /*****************************************************************************/
        public object? sqrt(object val)
        {            
            if(val == null)
                return null;

            if(val.TryParseDecimal(out decimal dVal))
                return (decimal)Math.Sqrt((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object? sin(object val)
        {            
            if(val == null)
                return null;

            if(val.TryParseDecimal(out decimal dVal))
                return Math.Sin((double)dVal);

            return null;
        }


        /*****************************************************************************/
        public object? cos(object val)
        {            
            if(val == null)
                return null;

            if(val.TryParseDecimal(out decimal dVal))
                return Math.Cos((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object? sinh(object val)
        {            
            if(val == null)
                return null;

            if(val.TryParseDecimal(out decimal dVal))
                return Math.Sinh((double)dVal);

            return null;
        }


        /*****************************************************************************/
        public object? cosh(object val)
        {            
            if(val == null)
                return null;

            if(val.TryParseDecimal(out decimal dVal))
                return Math.Cosh((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object? tan(object val)
        {            
            if(val == null)
                return null;

            if(val.TryParseDecimal(out decimal dVal))
                return Math.Tan((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object? acos(object val)
        {            
            if(val == null)
                return null;

            if(val.TryParseDecimal(out decimal dVal))
                return Math.Acos((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object? asin(object val)
        {            
            if(val == null)
                return null;

            if(val.TryParseDecimal(out decimal dVal))
                return Math.Asin((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object? atan(object val)
        {            
            if(val == null)
                return null;

            if(val.TryParseDecimal(out decimal dVal))
                return Math.Atan((double)dVal);

            return null;
        }

        /*****************************************************************************/
        public object? atan2(object val1, object val2)
        {            
            if(val1 == null || val2 == null)
                return null;

            if(val1.TryParseDecimal(out decimal dVal1) && val2.TryParseDecimal(out decimal dVal2))
                return (decimal)Math.Atan2((double)dVal1, (double)dVal2);

            return null;
        }

        #endregion

        #region String Functions

        /*****************************************************************************/
        public object ___string(object val)
        {
            if(val == null)
                return null;

            if(val is ICharacterSpan cspan)
                return cspan;

            return new StringValue(val!.ToString());
        }

        /*****************************************************************************/
        public string guid()
        {
            return Guid.NewGuid().ToString().ToLower();
        }

        /*****************************************************************************/
        public string? lowercase(object val)
        {
            if(val == null)
                return null;

            return val?.ToString()?.ToLower();
        }

        /*****************************************************************************/
        public string? uppercase(object val)
        {
            if(val == null)
                return null;

            return val?.ToString()?.ToUpper();
        }

        /*****************************************************************************/
        public string? substring(string val, int start, int length)
        {
            return val?.Substring(start, length);
        }

        /*****************************************************************************/
        public string? substring(string val, int start)
        {
            return val?.Substring(start);
        }

        /*****************************************************************************/
        public string? substringafter(string val, string substr)
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
                return val;
 
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

            if(val is ICharacterSpan cspan)
            { 
                if(cspan.IsNullOrWhiteSpace())
                    return false;

                var substr = searchFor.AsCharacterSpan();

                return cspan!.Contains(substr);
            }

            if(val is string sVal)
            { 
                if(string.IsNullOrEmpty(sVal))
                    return false;

                var substr = searchFor.ToString();

                return sVal.Contains(substr);
            }

            if(val.IsDictionary())
            { 
                var found = val.GetPropertyValue(searchFor.AsCharacterSpan());

                return found != null;
            }

            if(val is IEnumerable<object> list)
                return list.Any( i=> JTran.Extensions.ObjectExtensions.compareto(i, searchFor, out Type type) == 0 );

            return false;
        }

        /*****************************************************************************/
        public string? normalizespace(string val)
        // ??? Change first param to object
        {
            return val?.Trim()?.Replace("  ", " ");
        }

        /*****************************************************************************/
        // ??? Change first param to object
        public string? trim(string val)
        {
            return val?.Trim();
        }

        /*****************************************************************************/
        // ??? Change first param to object
        public string? trimend(string val)
        {
            return val?.TrimEnd();
        }

        /*****************************************************************************/
        // ??? Change first param to object
        public string? trimstart(string val)
        {
            return val?.TrimStart();
        }
        
        /*****************************************************************************/
        // ??? Change first param to object
        public string? replace(string val, string r1, string r2)
        {
            return val?.Replace(r1, r2);
        }

        /*****************************************************************************/
        // ??? Change first param to object
        public string? replaceending(string val, string r1, string r2)
        {
            return val?.ReplaceEnding(r1, r2);
        }

        /*****************************************************************************/
        // ??? Change first param to object
        public string? remove(string val, string r1)
        {
            return val?.Replace(r1, "");
        }

        /*****************************************************************************/
        public object? removeany(object? val, object list)
        {
            if(val == null)
                return null;
            
            if(list is IEnumerable<object> listOfThingsToRemove)
            { 
                var sval = val!.ToString(); // ???

                foreach(var r1 in listOfThingsToRemove)
                { 
                    var newVal = sval.Replace(r1.ToString(), "");

                    sval = newVal;
                }

                return CharacterSpan.FromString(sval);
            }

            return val;
        }

        /*****************************************************************************/
        // ??? Change first param to object
        public string? padleft(string? val, string? padchar, int totalLen)
        {
            if(val == null || padchar == null || padchar.Length == 0)
                return val;

            return val.PadLeft(totalLen, padchar.FirstOrDefault());
        }

        /*****************************************************************************/
        // ??? Change first param to object
        public string? padright(string val, string padchar, int totalLen)
        {
            if(val == null || padchar == null || padchar.Length == 0)
                return val;

            return val.PadRight(totalLen, padchar.FirstOrDefault());
        }

        /*****************************************************************************/
        // ??? Change first param to object
        public string? removeending(string val, string r1)
        {
            return val?.ReplaceEnding(r1, "");
        }

        /*****************************************************************************/
        // ??? Change first param to object
        public string? removeanyending(string? val, object list)
        {
            if(val == null || list == null)
            return null;
            {
                
            }
            foreach (var r1 in list as IEnumerable<object>)
            { 
                var newVal = val?.ReplaceEnding(r1.ToString(), "");

                if(newVal != val)
                    return newVal;
            }

            return val;
        }

        /*****************************************************************************/
        public int stringlength(object? val)
        {
            if(val is null)
                return 0;

            if(val is string str)
                return str.Length;

            return val.AsCharacterSpan().Length;
        }

        /*****************************************************************************/
        // ??? Change first param to object
        public int indexof(string val, string substr)
        {
            return val?.IndexOf(substr) ?? -1;
        }

        /*****************************************************************************/
        public IEnumerable<object> split(object? val, object separator)
        {
            if(val == null)
                return Enumerable.Empty<string>();

            ICharacterSpan cspan = val.AsCharacterSpan();

            if(cspan.IsNullOrWhiteSpace()) 
                return Enumerable.Empty<string>();

            ICharacterSpan sep = separator.AsCharacterSpan();

            return cspan.Split(sep, true);
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

            if(val is ICharacterSpan cspan)
                return cspan.IsNullOrWhiteSpace();

            if(val is string str)
                return string.IsNullOrWhiteSpace(str);

            if(val is JsonObject jobj)
                return !jobj.Any();

            if(val is IDictionary<string, object> exp)
                return !exp.Any();

            if(val is bool)
                return false;

            if(val is IEnumerable<object> list)
            { 
                return !list.Any();
            }

            if(val is IEnumerable list2)
            { 
                foreach(var _ in list2)
                    return false;

                return true;
            }

            if(val.TryParseDecimal(out decimal dval))
                return dval == 0m;

            return Poco.FromObject(val).IsEmpty(val);
        }
        
        /*****************************************************************************/
        public object required(object val, string errorMessage)
        {
            if(empty(val))
                 throw new Transformer.UserError(errorMessage);

            return val;
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public long position(ExpressionContext context)
        {
            return context.Index;
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public string errormessage(ExpressionContext context)
        {
            return context?.UserError?.Message ?? "";
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public string errorcode(ExpressionContext context)
        {
            return context?.UserError?.ErrorCode ?? "";
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
        public ICharacterSpan name(ExpressionContext context)
        {
            if(context.Data is JsonObject jobj)
                return jobj.Name ?? CharacterSpan.Empty;

            return CharacterSpan.Empty;
        }

        /*****************************************************************************/
        public IEnumerable<object> sequence(object from, object to)
        {
            return sequence(from, to, 1d);
        }

        /*****************************************************************************/
        public IEnumerable<object> sequence(object from, object to, object increment)
        {
            if(!decimal.TryParse(from.ToString(), out decimal dFrom))
                return Array.Empty<object>();

            if(!decimal.TryParse(to.ToString(), out decimal dTo))
                return Array.Empty<object>();

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
        public ICharacterSpan? coalesce(object? primary, params object?[] fields)
        {
            if(primary != null)
            {
                var primarySpan = primary.AsCharacterSpan();

                if(!primarySpan.IsNullOrWhiteSpace())
                    return primarySpan;
            }

            foreach(var field in fields)
            { 
                if(field == null) 
                    continue; 

                if(field is ICharacterSpan cspan)
                    if(!cspan.IsNullOrWhiteSpace())
                        return cspan; 

                var fieldStr = field.ToString();

                if(!string.IsNullOrWhiteSpace(fieldStr))
                    return CharacterSpan.FromString(fieldStr.Trim()); 
            } 

            return CharacterSpan.Empty;
        }

        /*****************************************************************************/
        [IgnoreParameterCount]
        public decimal coalescenumber(string primary, params object?[] fields)
        {
            if(decimal.TryParse(primary, out decimal d) && d != 0m)
                return d;

            foreach(var field in fields)
            {
                if(decimal.TryParse(field?.ToString() ?? "", out decimal d2) && d2 != 0m)
                    return d2;
            }

            return 0m;
        }

        /*****************************************************************************/
        public object iif(bool condition, object first, object second)
        {
            return condition ? first : second;  
        }

        #endregion
    }
}
