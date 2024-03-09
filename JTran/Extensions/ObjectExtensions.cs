/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: ObjectExtensions.cs					    		        
 *        Class(es): ObjectExtensions				         		            
 *          Purpose: Extension methods for object                 
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using JTran.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    internal class GroupKey : Dictionary<string, object>
    {
        public override string ToString()
        {
            return string.Join(";;", this.Select(v=> v.Key.ToString() + ",," + v.Value.ToString()));
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal static class ObjectExtensions
    {
        /****************************************************************************/
        internal static IEnumerable<T> EnsureEnumerable<T>(this T obj)
        {
            if(obj is IEnumerable<T> list)
                return list;

            return new [] {obj};
        }

        /****************************************************************************/
        internal static object GetSingleValue(this object obj, ICharacterSpan expression, ExpressionContext context)
        {
            var results = obj.GetValue(expression, context);

            if(results is IEnumerable<object> list)
                return list.First();

            return results;
        }

        /****************************************************************************/
        [Obsolete("Use ICharacterSpan")]
        internal static object GetSingleValue(this object obj, string expression, ExpressionContext context)
        {
            var results = obj.GetValue(expression.AsCharacterSpan(), context);

            if(results is IEnumerable<object> list)
                return list.First();

            return results;
        }

        /****************************************************************************/
        internal static GroupKey GetGroupByKey(this object obj, IEnumerable<string> fields)
        {
            var result = new GroupKey();

            foreach(var field in fields)
            { 
                var val = obj.GetSingleValue(field, null);

                result.TryAdd(field.ToString(), val); 
            }

            return result;
        }
        
        /****************************************************************************/
        [Obsolete("This should only be used for unit tests")]
        internal static object GetValue(this object obj, string expression, ExpressionContext context)
        {
            return obj.GetValue(expression.AsCharacterSpan(), context);
        }

        /****************************************************************************/
        internal static object GetValue(this object obj, ICharacterSpan expression, ExpressionContext context)
        {
            if(expression[0] == '@')
            {
                obj = context.Data;
                 
                if(expression[1] == '.')
                { 
                    expression = expression.Substring(2);
                }
                else
                    expression = CharacterSpan.Empty;

                if(expression.Length == 0)
                    return obj;
           }
            else
            {
                // Resolve ancestors
                obj = obj.EvaluateAncestors(ref expression);
            }

            if(expression[0] == '$')
            {
                var varName = expression.Substring(1);
                var index   = expression.IndexOf('.');

                if(index != -1)
                { 
                    // ??? is there a better way to do this?
                    varName = varName.Substring(0, index-1);
                    expression = expression.Substring(1 + index);
                }
                else
                    expression = CharacterSpan.Empty;

                obj = context.GetVariable(varName, context);

                if(expression.Length == 0)
                    return obj;
            }

            var results = new List<object>(); // ??? inefficient
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
        internal static int compareto(object? leftVal, object? rightVal, out Type type)
        {
            type = typeof(object);

            if(leftVal == null && rightVal == null)
                return 0;

            if(rightVal == null)
                return 1;

            if(leftVal == null)
                return -1;

            var leftValStr  = leftVal.ToString(); 
            var rightValStr = rightVal.ToString(); 

            if(long.TryParse(leftValStr, out long leftLong))
            { 
                if(long.TryParse(rightValStr, out long rightLong))
                { 
                    type = typeof(long);
                    return leftLong.CompareTo(rightLong);
                }
            }

            if(decimal.TryParse(leftValStr, out decimal leftdecimal))
            { 
                if(decimal.TryParse(rightValStr, out decimal rightdecimal))
                { 
                    type = typeof(decimal);
                    return leftdecimal.CompareTo(rightdecimal);
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
        internal static int CompareTo(this object leftVal, object rightVal, out Type type)
        {
            return compareto(leftVal, rightVal, out type);
        }

        #region Private

        /****************************************************************************/
        private static void GetValue(this object obj, List<object> results, ICharacterSpan expression, ExpressionContext context, ref bool isList)
        {
            var parts  = expression.ToString().Split(new char[] {'.'} ); 
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
        [Obsolete]
        private static void GetValue(this object obj, List<object> results, string expression, ExpressionContext context, ref bool isList)
        {
            var parts  = expression.ToString().Split(new char[] {'.'} ); 
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
        public static bool GetParent(this object obj, ref object? parent)
        {        
            if(obj is IJsonToken jobj)
            { 
                parent = jobj.Parent;
                return true;
            }

            return false;
        }

        /****************************************************************************/
        private static bool GetAncestor(this object obj, ICharacterSpan key, ref object? parent)
        {        
           if(obj is IJsonToken jobj)
            { 
                parent = jobj.Parent;
                return true;
            }

            return false; 
        }

        /****************************************************************************/
        private static object EvaluateAncestors(this object obj, ref ICharacterSpan expression)
        {
            var result = obj;
            var index = 0;

            // Resolve ancestors
            while(expression[index] == '/')
            {
                if(result?.GetParent(ref result) ?? false)
                    ++index;
                else
                    return null;
            }

            if(index != 0)
                expression = new CharacterSpan(expression, index, expression.Length - index);

            return result;
        }

        /****************************************************************************/
        internal static bool IsPocoList(this IEnumerable<object> list, out Type? type)
        {
            var first = list.FirstOrDefault();

            if(first is null || first is IObject || first is IEnumerable)
            { 
                type = null;
                return false;
            }

            type = first.GetType();

            if(type == null)
                return false;

            if(!type.IsClass || type.Name == "String" || type.Name == "Object")
            {
                type = null;
                return false;
            }

            return true;
        }

        /****************************************************************************/
        internal static object GetPropertyValue(this object obj, ICharacterSpan nameSpan)       
        {
            if(obj == null)
                return null;

            // Resolve ancestors
            obj = obj.EvaluateAncestors(ref nameSpan);

            if(obj == null)
                return null;

            if(obj is IObject iobj)
                return iobj.GetPropertyValue(nameSpan);

            if(obj is ICollection<KeyValuePair<string, object>> dict1)
            { 
                var name = nameSpan.ToString();

                foreach(var kv in dict1)
                {
                    if(kv.Key == name)
                        return kv.Value;
                }

                return null;
            }

            if(obj is IDictionary dict)
            { 
                var name = nameSpan.ToString();

                foreach(var key in dict.Keys)
                {
                    if(key.ToString() == name)
                        return dict[name];
                }

                return null;
            }

            return Poco.FromObject(obj).GetValue(obj, nameSpan);
        }

        /****************************************************************************/
        [Obsolete("Replaced by ICharacterSpan version")]
        internal static object GetPropertyValue(this object obj, string name)       
        {
            return obj.GetPropertyValue(CharacterSpan.FromString(name));
        }

        #endregion
    }
}
