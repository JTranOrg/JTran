/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran.Extensions						            
 *             File: CharExtensions.cs					    		        
 *        Class(es): CharExtensions				         		            
 *          Purpose: Extension methods for char                 
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

namespace JTran.Extensions
{
    internal static class CharExtensions
    {
        /****************************************************************************/
        /****************************************************************************/
        internal static bool IsNumberChar(this char ch)
        {
            return(char.IsDigit(ch) || ch == '.' || ch == '-');
        }
    }
}
