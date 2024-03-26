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
            return numberoperation(val, (dVal)=> Math.Floor(dVal));
        }

        /*****************************************************************************/
        public object? ceiling(object val)
        {
            return numberoperation(val, (dVal)=> Math.Ceiling(dVal));
        }

        /*****************************************************************************/
        public object? round(object val)
        {
            return numberoperation(val, (dVal)=> Math.Round(dVal));
        }

        /*****************************************************************************/
        public object? precision(object val, int numPlaces)
        {
            var multiplier = (decimal)Math.Pow(10, numPlaces);

            return numberoperation(val, (dVal)=> Math.Floor(dVal * multiplier) / multiplier);
        }

        /*****************************************************************************/
        public object? number(object? val)
        {
            return numberoperation(val, (n)=> n) ?? 0m;
        }

        /*****************************************************************************/
        public object? sqrt(object val)
        {            
            return numberoperation(val, (dVal)=> (decimal)Math.Sqrt((double)dVal));
        }

        /*****************************************************************************/
        public object? sin(object val)
        {            
            return numberoperation(val, (dVal)=> (decimal)Math.Sin((double)dVal));
        }

        /*****************************************************************************/
        public object? cos(object val)
        {            
            return numberoperation(val, (dVal)=> (decimal)Math.Cos((double)dVal));
        }

        /*****************************************************************************/
        public object? sinh(object val)
        {            
            return numberoperation(val, (dVal)=> (decimal)Math.Sinh((double)dVal));
        }

        /*****************************************************************************/
        public object? cosh(object val)
        {            
            return numberoperation(val, (dVal)=> (decimal)Math.Cosh((double)dVal));
        }

        /*****************************************************************************/
        public object? tan(object val)
        {            
            return numberoperation(val, (dVal)=> (decimal)Math.Tan((double)dVal));
        }

        /*****************************************************************************/
        public object? acos(object val)
        {            
            return numberoperation(val, (dVal)=> (decimal)Math.Acos((double)dVal));
        }

        /*****************************************************************************/
        public object? asin(object val)
        {            
            return numberoperation(val, (dVal)=> (decimal)Math.Asin((double)dVal));
        }

        /*****************************************************************************/
        public object? atan(object val)
        {            
            return numberoperation(val, (dVal)=> (decimal)Math.Atan((double)dVal));
        }

        /*****************************************************************************/
        public object? atan2(object? val1, object? val2)
        {            
            if(val1 == null || val2 == null)
                return null;

            if(val1.TryParseDecimal(out decimal dVal1) && val2.TryParseDecimal(out decimal dVal2))
                return (decimal)Math.Atan2((double)dVal1, (double)dVal2);

            return null;
        }
        
        /*****************************************************************************/
        public bool isnumber(object val)
        {
            if(val == null)
                return false;

            return decimal.TryParse(val.ToString(), out decimal _);
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

            return long.TryParse(val.ToString(), out long _);
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
        public object? pow(object? val1, object? val2)
        {            
            if(val1 == null || val2 == null)
                return null;

            if(val1.TryParseDecimal(out decimal dVal1) && val2.TryParseDecimal(out decimal dVal2))
                return (decimal)Math.Pow((double)dVal1, (double)dVal2);

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
        public object? uppercase(object? val)
        {
            return Transform(val, 
                            (ch)=> (true, char.ToUpperInvariant(ch)), 
                            (s)=> s.ToUpperInvariant());
        }

        /*****************************************************************************/
        public object? lowercase(object? val)
        {
            return Transform(val, 
                            (ch)=> (true, char.ToLowerInvariant(ch)), 
                            (s)=> s.ToLowerInvariant());
        }

        #region substring

        /*****************************************************************************/
        [IgnoreParameterCount]
        public object? substring(object? val, int start, int length = -1000)
        {
            if(val == null)
                return val;

            if(val is ICharacterSpan cspan)
                return cspan!.Substring(start, length);

            var str = val.ToString();

            if(length == -1000)
                return str.Substring(start);

            if(length >= 0)
                length = Math.Min(length, str.Length - start);  
            
            if(start >= str.Length || length < 1)
                return string.Empty;

            return str.Substring(start, length);
        }

        /*****************************************************************************/
        public object? substringafter(object? val, object? substr)
        {
            return Match<object?>(val, substr, val, 
                                 (val, search)=> val.Find(search, out int index) ? val.Substring(index + search.Length) : CharacterSpan.Empty, 
                                 (val, search)=> val.Find(search, out int index) ? val.Substring(index + search.Length) : CharacterSpan.Empty);
        }

        /*****************************************************************************/
        public object? substringbefore(object? val, object? substr)
        {
            return Match<object?>(val, substr, val, 
                                 (val, search)=> val.Find(search, out int index) ? val.Substring(0, index) : val, 
                                 (val, search)=> val.Find(search, out int index) ? val.Substring(0, index) : val);
        }

        #endregion

        /*****************************************************************************/
        public bool startswith(object? val, object? substr)
        {
            return Match(val, substr, false, 
                         (val, search)=> val.IndexOf(search) == 0, 
                         (val, search)=> val.IndexOf(search) == 0);
        }

        /*****************************************************************************/
        public bool endswith(object? val, object? substr)
        {
            return Match(val, substr, false, 
                        (val, search)=> val.LastIndexOf(search) == (val.Length - search.Length), 
                        (val, search)=> val.LastIndexOf(search) == (val.Length - search.Length));
        }

        /*****************************************************************************/
        public bool contains(object? val, object? searchFor)
        {
            if(val == null || searchFor == null)
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
                return list.Any( i=> i.CompareTo(searchFor) == 0 );

            return false;
        }

        #region trim

        /*****************************************************************************/
        public object? normalizespace(object? val)
        {
            if(val == null)
                return null;

            var cspan = val.AsCharacterSpan();
            var previous = '\0';

            return cspan.Trim().Transform( ch=> 
            {
                var result = (ch != ' ' || previous != ' ', ch);

                previous = ch;

                return result;
            });
        }

        /*****************************************************************************/
        public object? trim(object? val)
        {
            if(val == null)
                return val;

            if(val is ICharacterSpan cspan)
                return cspan!.Trim();

            return val.ToString().Trim();
        }

        /*****************************************************************************/
        public object? trimend(object? val)
        {
            if(val == null)
                return val;

            if(val is ICharacterSpan cspan)
                return cspan!.Trim(false, true);

            return val.ToString().TrimEnd();
        }

        /*****************************************************************************/
        public object? trimstart(object? val)
        {
            if(val == null)
                return val;

            if(val is ICharacterSpan cspan)
                return cspan!.Trim(true, false);

            return val.ToString().TrimStart();
        }

        #endregion

        #region replace

        /*****************************************************************************/
        public object? replace(object? val, object? r1, object? r2)
        {
            if(val == null || r1 == null || r2 == null)
                return val;

            if(val is ICharacterSpan cspan)
                return cspan.Replace(r1.AsCharacterSpan(), r2.AsCharacterSpan());

            return val!.ToString()!.Replace(r1.ToString()!, r2.ToString());
        }

        /*****************************************************************************/
        public object? replaceending(object? val, object? r1, object? r2)
        {
            if(val == null || r1 == null || r2 == null)
                return val;

            if(val is ICharacterSpan cspan)
                return cspan.ReplaceEnding(r1.AsCharacterSpan(), r2.AsCharacterSpan());

            return val.ToString()!.ReplaceEnding(r1.ToString()!, r2.ToString());
        }

        #endregion

        #region remove

        /*****************************************************************************/
        public object? remove(object? val, object? r1)
        {
            if(val == null || r1 == null)
                return val;

            if(val is ICharacterSpan cspan)
                return cspan.Remove(r1.AsCharacterSpan());

            var r1Str = r1.ToString();
            var valStr = val.ToString();

            if(r1Str!.Length == 0 || valStr!.Length == 0)
                return val;

            return valStr.Replace(r1Str, "");
        }

        /*****************************************************************************/
        public object? removeany(object? val, object? list)
        {
            if(val != null && list != null && list is IEnumerable<object> listOfThingsToRemove)
            { 
                foreach(var r1 in listOfThingsToRemove)
                    val = remove(val, r1);
            }

            return val;
        }

        /*****************************************************************************/
        public object? removeending(object? val, object? r1)
        {
            if(val == null || r1 == null)
                return val;

            if(val is ICharacterSpan cspan)
            { 
                var ending = r1.AsCharacterSpan();

                if(cspan.EndsWith(ending))
                { 
                    var index = cspan.Length - ending.Length;

                    if(index == 0)
                        return CharacterSpan.Empty;

                    return cspan.Substring(0, index);
                }

                return val;
            }            

            return val!.ToString().ReplaceEnding(r1!.ToString(), "");
        }

        /*****************************************************************************/
        public object? removeanyending(object? val, object? list)
        {
            if(val == null || list == null)
                return val;

            if(list is IEnumerable<object> enm)
            { 
                foreach (var r1 in enm)
                { 
                    var newVal = removeending(val, r1);

                    if(newVal != val)
                        return newVal;
                }
            }
            else
                return removeending(val, list);

            return val;
        }

        #endregion

        #region pad

        /*****************************************************************************/
        public object? padright(object? val, object? pad, int totalLen)
        {
            return Pad(val, pad, totalLen, false);
        }

        /*****************************************************************************/
        public object? padleft(object? val, object? pad, int totalLen)
        {
            return Pad(val, pad, totalLen, true);
        }

        #endregion

        /*****************************************************************************/
        public int stringlength(object? val)
        {
            if(val is null)
                return 0;

            if(val is ICharacterSpan cspan)
                return cspan.Length;

            return val!.ToString().Length;
        }

        /*****************************************************************************/
        public int indexof(object? val, object? search)
        {
            return Match(val, search, -1, 
                        (val, search)=> val.IndexOf(search), 
                        (val, search)=> val.IndexOf(search));
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

        #region Private
        
        /*****************************************************************************/
        private object? Transform(object? val, Func<char, (bool Use, char NewVal)> cspanTransform, Func<string, string> strTransform)
        {
            if(val == null)
                return null;

            if(val is ICharacterSpan cspan)
                return cspan.Transform(cspanTransform);
            
            return strTransform(val.ToString()!);
        }
        
        /*****************************************************************************/
        private object? Pad(object? val, object? pad, int totalLen, bool left)
        {
            if(pad == null || totalLen == 0)
                return val;

            var padstr = pad.ToString();

            if(padstr!.Length == 0)
                return val;

            if(val == null)
                val = CharacterSpan.Empty;

            if(val is ICharacterSpan cspan)
                return cspan.Pad(padstr[0], totalLen, left);

            var valStr = val.ToString();

            return left ? valStr!.PadLeft(totalLen, padstr[0]) : valStr!.PadRight(totalLen, padstr[0]);
        }

        /*****************************************************************************/
        private T Match<T>(object? val, object? substr, T tDefault, Func<ICharacterSpan, ICharacterSpan, T> cspanMatches, Func<string, string, T> strMatches)
        {
            if(val == null || substr == null)
                return tDefault;

            if(val is ICharacterSpan cspan)
            {
                var search = substr.AsCharacterSpan(true);

                if(search.Length == 0) 
                    return tDefault;

                return cspanMatches(cspan, search);
            }

            var searchStr = substr.ToString();

            if(searchStr.Length == 0) 
                return tDefault;

            return strMatches(val.ToString(), searchStr);
        }

        /*****************************************************************************/
        private object? numberoperation(object? val, Func<decimal, decimal> op)
        {
            if(val == null)
                return null;

            if(!decimal.TryParse(val.ToString(), out decimal dVal))
                return null;

            return op(dVal);
        }

        #endregion
    }
}
