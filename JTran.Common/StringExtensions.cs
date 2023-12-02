/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: StringExtensions.cs					    		        
 *        Class(es): StringExtensions				         		            
 *          Purpose: Extension methods for strings                 
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 2 Dec Apr 2023                                             
 *                                                                          
 *   Copyright (c) 2023 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

namespace JTran.Common
{
    public static class StringExtensions
    {
        public static string SubstringBefore(this string val, string before)
        {
            var index = val.IndexOf(before);

            if(index == -1)
                return val;

            return val.Substring(0, index);
        }       
    }
}
