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
 *   Copyright (c) 2020 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System.Runtime.CompilerServices;
using System.Dynamic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    internal static class StringExtensions
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
        internal static object JsonToExpando(this string s)
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
    }
}
