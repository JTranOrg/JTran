/***************************************************************************
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
 *   Copyright (c) 2020-2023 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System.Runtime.CompilerServices;

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
