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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JTran.Collections;
using JTran.Common;
using JTran.Expressions;

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    internal class GroupKey : Dictionary<ICharacterSpan, object>
    {
        public override string ToString()
        {
            return string.Join(";;", this.Select(v=> v.Key.ToString() + ",," + v.Value?.ToString() ?? ""));
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal static class ObjectExtensions
    {
        /****************************************************************************/
        internal static IEnumerable EnsureEnumerable(this object obj)
        {
            // These things will cast as IEnumerable but are actually single objects
            if(obj is ICharacterSpan || obj is string || obj is IObject)
                return new [] { obj };

            if(obj is IEnumerable list)
                return list;

            return new [] { obj };
        }

        /****************************************************************************/
        internal static IEnumerable<object> EnsureObjectEnumerable(this object obj)
        {
            // These things will cast as IEnumerable but are actually single objects
            if(obj is ICharacterSpan || obj is string || obj is IObject)
                return [obj];

            if(obj is IEnumerable<object> list)
                return list;

            if(obj is IEnumerable enm)
                return new EnumerableWrapper(enm);

            return [obj];
        }

        /****************************************************************************/
        internal static GroupKey GetGroupByKey(this object obj, IExpression groupExpr, ExpressionContext context, IList<ICharacterSpan> fields)
        {
            context.Data = obj;

            var groupValues = groupExpr.Evaluate(context);

            var result = new GroupKey();

            if(groupValues is IEnumerable<object> list)
            {
                var numFields = fields.Count;
                var i = 0;

                foreach(var item in list)
                { 
                    result.TryAdd(fields[i++], item); 
                }
            }
            else
                result.TryAdd(fields[0], groupValues);

            return result;
        }
        
        /****************************************************************************/
        internal static bool IsDictionary(this object obj)
        {
            return obj is IEnumerable && obj.GetType().Name.Contains("Dictionary");
        }

        /****************************************************************************/
        // ??? what about DateTimeOffset
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
        internal static int Compare(object? leftVal, object? rightVal, out object? lVal, out object? rVal)
        {
            lVal = leftVal;
            rVal = rightVal;

            if(leftVal == null && rightVal == null)
                return 0;

            if(rightVal == null)
                return 1;

            if(leftVal == null)
                return -1;

            if(leftVal is bool b1 && rightVal is bool b2)
                return b1.CompareTo(b2);

            if(leftVal.TryParseDecimal(out decimal d1) && rightVal.TryParseDecimal(out decimal d2))
            { 
                lVal = d1;
                rVal = d2;
                return d1.CompareTo(d2);
            }

            var leftValStr  = leftVal.ToString(); 
            var rightValStr = rightVal.ToString(); 

            if(leftVal.TryParseDateTime(out DateTime? dtLeft) && rightVal.TryParseDateTime(out DateTime? dtRight))
            { 
                lVal = dtLeft!.Value.ToString("s");
                rVal = dtRight!.Value.ToString("s");
                return DateTime.Compare(dtLeft!.Value, dtRight!.Value);
            }
                    
            return leftValStr.CompareTo(rightValStr);
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

            if(leftVal is bool b1 && rightVal is bool b2)
            { 
                type = typeof(bool);
                return b1.CompareTo(b2);
            }

            if(leftVal.TryParseDecimal(out decimal d1) && rightVal.TryParseDecimal(out decimal d2))
            { 
                type = typeof(bool);
                return d1.CompareTo(d2);
            }

            var leftValStr  = leftVal.ToString(); 
            var rightValStr = rightVal.ToString(); 

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
