/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
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

namespace JTran
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
        
        /****************************************************************************/
        /// <summary>
        /// Determines if list has a single item
        /// </summary>
        internal static bool IsSingle<T>(this IEnumerable<T>? list)
        {
            var items = 0;

            if(list == null) 
                return false;

            foreach(var item in list) 
            { 
                if(++items > 1)
                    return false;
            }

            return items == 1;
        }   
    }
}
