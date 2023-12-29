/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran.Extensions						            
 *             File: StringExtensions.cs					    		        
 *        Class(es): StringExtensions				         		            
 *          Purpose: Extension methods for string                 
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
using System.Dynamic;
using System.Runtime.CompilerServices;

using JTran.Json;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    public static class StringExtensions
    {
        /****************************************************************************/
        public static T ToObject<T>(this string str) where T : new()
        {          
            var parser = new Json.Parser(new JsonModelBuilder());
            var expObj = parser.Parse(str) as ExpandoObject;

            return expObj.ToObject<T>();
        }

        /****************************************************************************/
        public static string FormatForJsonOutput(this string str)
        {            
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\f", "\\f").Replace("\b", "\\b");
        }

        /****************************************************************************/
        public static string SubstringBefore(this string str, string part)
        {            
            var index = str.IndexOf(part);

            if(index == -1)
                return str;

            return str.Substring(0, index);
        }

        /****************************************************************************/
        public static bool TryParseDateTime(this string sdate, out DateTime dtValue)
        {
            dtValue = DateTime.MinValue;

            if(sdate == null)
                return false;

            if(sdate.EndsWith("Z"))
            {
                if(!DateTimeOffset.TryParse(sdate, out DateTimeOffset dtoValue)) 
                    return false;

                dtValue = dtoValue.UtcDateTime;
                return true;
            }

            return DateTime.TryParse(sdate, out dtValue);
        }
    }
}
