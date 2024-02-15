﻿/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
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
            var expObj = parser.Parse(str) as JsonObject;

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
        public static string SubstringAfter(this string str, string part)
        {            
            var index = str.IndexOf(part);

            if(index == -1)
                return str;

            return str.Substring(index + part.Length);
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

        /****************************************************************************/
        internal static bool IsSingleQuoted(this string s)
        {
            return(s.StartsWith("'") && s.EndsWith("'"));
        }

        /****************************************************************************/
        internal static bool IsDoubleQuoted(this string s)
        {
            return(s.StartsWith("\"") && s.EndsWith("\""));
        }

        /****************************************************************************/
        internal static bool IsQuoted(this string s)
        {
            return(s.IsSingleQuoted() || s.IsDoubleQuoted());
        }

        /****************************************************************************/
        public static string ReplaceEnding(this string s, string ending, string replace)
        {
            if(!s.EndsWith(ending))
                return s;
            
            return s.Substring(0, s.Length - ending.Length) + replace;
        }

        /****************************************************************************/
        public static string EnsureDoesNotStartWith(this string s, string start)
        {
            if(!s.StartsWith(start))
                return s;
            
            return s.Substring(start.Length);
        }
    }
}
