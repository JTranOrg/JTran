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
 *   Copyright (c) 2020-2023 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

using JTran.Json;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    public static class StringExtensions
    {
        /****************************************************************************/
        public static string FormatForJsonOutput(this string str)
        {            
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\f", "\\f").Replace("\b", "\\b");
        }

        /****************************************************************************/
        public static T ExpandoToObject<T>(this object obj)
        {            
            return JsonConvert.DeserializeObject<T>((obj as ExpandoObject).ToJson());
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
