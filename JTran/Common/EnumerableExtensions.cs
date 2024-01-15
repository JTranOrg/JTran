/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran.Common							            
 *             File: EnumerableExtensions.cs					    		        
 *        Class(es): EnumerableExtensions				         		            
 *          Purpose: Extension methods for IEnumerable                 
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 11 Jan 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;

namespace JTran.Common.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    internal static class EnumerableExtensions
    {
        /****************************************************************************/
        /// <summary>
        /// Finds a matching item and returns the index
        /// </summary>
        internal static int IndexOfAny<T>(this IEnumerable<T> list, Func<T, bool> match)
        {
            var index = 0;

            foreach(var item in list) 
            { 
                if(match(item))
                    return index;

                ++index;
            }

            return -1;
        }   
    }
}
