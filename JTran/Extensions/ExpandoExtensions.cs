/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: ExpandoExtensions.cs					    		        
 *        Class(es): ExpandoExtensions				         		            
 *          Purpose: Extension methods for ExpandoObject                 
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
 *                                                                          
 *   Copyright (c) 2020 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    public static class ExpandoExtensions
    {
        /****************************************************************************/
        internal static ExpandoObject SetParent(this ExpandoObject obj)
        {
            var dict = obj as IDictionary<string, object>;

            foreach(var val in dict)
            {
                if(val.Value != null)
                { 
                    var type = val.Value.GetType().Name;

                    if(!val.Key.StartsWith("_jtran_"))
                    {
                        SetChild(val.Value, obj, null, -1, val.Key);
                    }
                }
            }

            return obj;
        }

        /****************************************************************************/
        public static string ToJson(this ExpandoObject obj)
        {            
            var sb = new StringBuilder();

            obj.ToJson(sb, true);

            return sb.ToString();
        }

        /****************************************************************************/
        private static void ToJson(this ExpandoObject obj, StringBuilder sb, bool last)
        {
            if(obj == null)
                return;

            sb.AppendLine("{");

            var dict     = (obj as IDictionary<string, object>).Where( kv=> !kv.Key.StartsWith("_jtran_") );
            var index    = 0;
            var numItems = dict.Count();

            foreach(var kv in dict)
            {
                var comma = (index == numItems-1) ? "" : ",";

                sb.Append(kv.Key + ":");

                ToJson(kv.Value, sb, comma);

                ++index;
            }

            sb.AppendLine("}" + (last ? "" : ","));
        }

        /****************************************************************************/
        internal static void ToJson(object value, StringBuilder sb, string comma)
        {
            if(value == null)
            {
                sb.AppendLine(" null" + comma);
            }
            else if(value is IList list)
            {
                sb.AppendLine();
                sb.AppendLine("[");

                var childIndex = 0;
                var numChildItems = list.Count;

                foreach(var child in list)
                {
                    var childComma = (++childIndex == numChildItems) ? "" : ",";

                    ToJson(child, sb, childComma);
                }

                sb.AppendLine("]" + comma);
            }
            else if(value is ExpandoObject expando)
            {
                sb.AppendLine();
                expando.ToJson(sb, comma == "");                   
            }
            else
            {                       
                if(value is bool)
                    sb.AppendLine(" " + value.ToString().ToLower() + comma);
                else if(bool.TryParse(value.ToString(), out bool bval))
                    sb.AppendLine(" " + bval.ToString().ToLower() + comma);
                else if(long.TryParse(value.ToString(), out long lval))
                    sb.AppendLine(" " + lval.ToString() + comma);
                else if(decimal.TryParse(value.ToString(), out decimal dval))
                    sb.AppendLine(" " + dval.ToString().ReplaceEnding(".0", "") + comma);
                else if(value is DateTime dtVal)
                    sb.AppendLine(" \"" + dtVal.ToString("o") + "\"" + comma);
                else if(DateTime.TryParse(value.ToString(), out DateTime dtVal2))
                    sb.AppendLine(" \"" + dtVal2.ToString("o") + "\"" + comma);
                else
                    sb.AppendLine(" \"" + value.ToString() + "\"" + comma);
            }
        }

        /****************************************************************************/
        private static void SetChild(object child, object parent, object gparent, int index, string name)
        {
            if(child is ExpandoObject expando)
            {
                dynamic dyn = expando;

                dyn._jtran_parent = parent;

                if(gparent != null)
                    dyn._jtran_gparent = gparent;

                if(index != -1)
                    dyn._jtran_position = index;

                if(name != null)
                    dyn._jtran_name = name;

                expando.SetParent();
            }
            else if(child is IList list)
            {
                var childIndex = 0;

                foreach(var gchild in list)
                    SetChild(gchild, child, parent, childIndex++, (childIndex - 1).ToString());
            }

            return;
        }
    }
}
