/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran.Extensions						            
 *             File: DateTimeExtensions.cs					    		        
 *        Class(es): DateTimeExtensions				         		            
 *          Purpose: Extension methods for DateTime                 
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 26 Jul 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2022 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Data;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    internal static class DateTimeExtensions
    {
        /****************************************************************************/
        internal static int DayOfWeekOccurrence(this DateTime dt)
        {
            var nweeks = (int)Math.Floor(dt.Day / 7d);
            var nday   = dt.Day - (nweeks * 7);
            var flr    = (int)Math.Ceiling(nday / 7d);

            return flr + nweeks;
        }
    }
}
