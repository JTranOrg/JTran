/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: EnumerableExtensions.cs					    		        
 *        Class(es): EnumerableExtensions				         		            
 *          Purpose: Extension methods for collections                 
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

using System.Collections;
using System.Collections.Generic;

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    internal static class EnumerableExtensions
    {
        /****************************************************************************/
        internal static bool Empty(this ICollection list)
        {
            return (list?.Count ?? 0) == 0;
        }

        /****************************************************************************/
        internal static bool Empty<T>(this ICollection<T> list)
        {
            return (list?.Count ?? 0) == 0;
        }
    }
}
