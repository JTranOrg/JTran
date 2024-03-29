﻿/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: ObjectExtensions.cs					    		        
 *        Class(es): ObjectExtensions				         		            
 *          Purpose: Extension methods for object                 
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2022 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    internal static class ObjectExtensions
    {
        /****************************************************************************/
        internal static object GetSingleValue(this object obj, string expression, ExpressionContext context)
        {
            var results = obj.GetValue(expression, context);

            if(results is IList list)
                return list[0];

            return results;
        }

        /****************************************************************************/
        internal static object GetValue(this object obj, string expression, ExpressionContext context)
        {
            if(expression.StartsWith("@"))
            {
                obj = context.Data;
                 
                if(expression.StartsWith("@."))
                { 
                    expression = expression.Substring(2);
                }
                else
                    expression = "";

                if(expression == "")
                    return obj;
           }
            else
            {
                // Resolve ancestors
                obj = obj.EvaluateAncestors(ref expression);
            }

            if(expression.StartsWith("$"))
            {
                var varName = expression.Substring(1);
                var index   = expression.IndexOf(".");

                if(index != -1)
                { 
                    varName = varName.Substring(0, index-1);
                    expression = expression.Substring(1 + index);
                }
                else
                    expression = "";

                obj = context.GetVariable(varName);

                if(expression == "")
                    return obj;
            }

            var results = new List<object>();
            var isList  = false;

            obj.GetValue(results, expression, context, ref isList);

            if(isList)
                return results;

            if(results.Count == 0)
                return null;
                
            return results[0];
        }

        /****************************************************************************/
        internal static bool IsDictionary(this object obj)
        {
            return obj is IEnumerable && obj.GetType().Name.Contains("Dictionary");
        }

        /****************************************************************************/
        internal static bool TryParseDateTime(this object data, out DateTime? dtValue)
        {
            dtValue = null;

            if(data == null)
                return false;

            if(data is DateTime dtValue2)
            {
                dtValue = dtValue2;
                return true;
            }

            if(data is DateTimeOffset dtValue3)
            {
                dtValue = dtValue3.DateTime;
                return true;
            }

            var sdate = data.ToString();

            if(sdate.EndsWith("Z"))
            {
                if(!DateTimeOffset.TryParse(sdate, out DateTimeOffset dtoValue)) 
                    return false;

                dtValue = dtoValue.UtcDateTime;
                return true;
            }

            if(DateTime.TryParse(sdate, out DateTime dtValue4))
            {
                dtValue = dtValue4;
                return true;
            }

            return false;
        }

        /*****************************************************************************/
        internal static int CompareTo(this object leftVal, object rightVal, out Type type)
        {
            type = typeof(object);

            if(leftVal == null && rightVal == null)
                return 0;

            if(rightVal == null)
                return 1;

            if(leftVal == null)
                return -1;

            var leftValStr  = leftVal?.ToString(); 
            var rightValStr = rightVal?.ToString(); 

            if(long.TryParse(leftValStr, out long leftLong))
            { 
                if(long.TryParse(rightValStr, out long rightLong))
                { 
                    type = typeof(long);
                    return leftLong.CompareTo(rightLong);
                }
            }

            if(decimal.TryParse(leftValStr, out decimal leftDecimal))
            { 
                if(decimal.TryParse(rightValStr, out decimal rightDecimal))
                { 
                    type = typeof(decimal);
                    return leftDecimal.CompareTo(rightDecimal);
                }
            }

            if(bool.TryParse(leftValStr, out bool leftBool))
            { 
                if(bool.TryParse(rightValStr, out bool rightBool))
                { 
                    type = typeof(bool);
                    return leftBool.CompareTo(rightBool);
                }
            }

            if(leftVal.TryParseDateTime(out DateTime? dtLeft))
            { 
                if(rightVal.TryParseDateTime(out DateTime? dtRight))
                { 
                    type = typeof(DateTime);
                    return DateTime.Compare(dtLeft.Value, dtRight.Value);
                }
            }
                    
            type = typeof(string);
            return leftValStr.CompareTo(rightValStr);
        }

        /*****************************************************************************/
        internal static string FormatForOutput(this object value, bool forceString = false, bool finalOutput = false)
        {
            if(value == null)
                return "null";

            if(!forceString)
            { 
                if(value is bool)
                    return value.ToString().ToLower();

                if(!(value is StringValue))
                { 
                    if(bool.TryParse(value.ToString(), out bool bval))
                        return bval.ToString().ToLower();

                    if(long.TryParse(value.ToString(), out long lval))
                        return lval.ToString();

                    if(decimal.TryParse(value.ToString(), out decimal dval))
                        return dval.ToString().ReplaceEnding(".0", "");
                }

                if(value is DateTime dtVal)
                    return "\"" + dtVal.ToString("o") + "\"";
            }

            return "\"" + (finalOutput ? value.ToString().FormatForJsonOutput() : value.ToString()) + "\"";
        }

        #region Private

        /****************************************************************************/
        private static void GetValue(this object obj, List<object> results, string expression, ExpressionContext context, ref bool isList)
        {
            var parts  = expression.Split(new char[] {'.'} ); 
            var nParts = parts.Length;

            for(var i = 0; i < (nParts - 1); ++i)
            { 
                var part = parts[i].Trim();
                var partObj = obj.GetPropertyValue(part);

                // Array object without brackets
                if(partObj is IList array)
                {
                    isList = true;

                    foreach(var child in array)
                        child.GetValue(results, string.Join(".", parts, i+1, parts.Length - i - 1), context, ref isList);

                    return;
                }
                    
                obj = partObj;

                if(obj == null)
                    return;
            }

            var partName = parts[nParts-1];
            var val      = obj.GetPropertyValue(partName);

            if(val == null)
                return;

            if(!val.IsDictionary() && val is IList list)
            { 
                isList = true;

                foreach(var child in list)
                    results.Add(child);
            }
            else
                results.Add(val);
        }

        /****************************************************************************/
        private static object EvaluateAncestors(this object obj, ref string expression)
        {
            var result = obj;

            // Resolve ancestors
            while(expression.StartsWith("/"))
            {
                if(expression.StartsWith("//"))
                {
                    try
                    { 
                        result = (result as dynamic)._jtran_gparent;
                        expression = expression.Substring(2);
                        continue;
                    }
                    catch
                    {
                        // Fall thru
                    }
                }

                try
                { 
                    result = (result as dynamic)._jtran_parent;
                    expression = expression.Substring(1);
                }
                catch
                {
                    return null;
                }
            }

            return result;
        }

        /****************************************************************************/
        internal static object GetPropertyValue(this object obj, string name)       
        {
            if(obj == null)
                return null;

            if(obj is ExpandoObject)
            {
                var props = obj as IDictionary<string, object>;

                if(props.ContainsKey(name))
                    return props[name];

                return null;
            }

            var otype = obj.GetType();

            if(obj is ICollection<KeyValuePair<string, object>> dict1)
            { 
                foreach(var kv in dict1)
                {
                    if(kv.Key == name)
                        return kv.Value;
                }

                return null;
            }

            if(obj is IDictionary dict)
            { 
                foreach(var key in dict.Keys)
                {
                    if(key.ToString() == name)
                        return dict[name];
                }

                return null;
            }

            var prop  = otype.GetProperty(name);
            
            return prop.GetValue(obj);
       }

        /****************************************************************************/
        private static object GetArrayPart(IEnumerable array, string[] parts, int index, ExpressionContext context)
        {        
            var nextParts = string.Join(".", parts, index+1, parts.Length - index - 1);

            foreach(var child in array)
            {
                if(child != null)
                { 
                    var val = child.GetValue(nextParts, context);

                    if(val != null)
                        return val;
                }
            }

            return null;       
        }

        #endregion
    }
}
