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
 *   Copyright (c) 2020-2022 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    public static class StringExtensions
    {
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
        public static object JsonToExpando(this string s)
        {
            var convertor = new ExpandoObjectConverter();
            
            return JsonConvert.DeserializeObject<ExpandoObject>(s, convertor).SetParent();
        }

        /****************************************************************************/
        internal static string ReplaceEnding(this string s, string ending, string replace)
        {
            if(!s.EndsWith(ending))
                return s;
            
            return s.Substring(0, s.Length - ending.Length) + replace;
        }

        /****************************************************************************/
        internal static string EnsureDoesNotStartWith(this string s, string start)
        {
            if(!s.StartsWith(start))
                return s;
            
            return s.Substring(start.Length);
        }

        /****************************************************************************/
        internal static T ExpandoToObject<T>(this object obj)
        {            
            return JsonConvert.DeserializeObject<T>((obj as ExpandoObject).ToJson());
        }

        /****************************************************************************/
        internal static bool TryParseDateTime(this string sdate, out DateTime dtValue)
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
