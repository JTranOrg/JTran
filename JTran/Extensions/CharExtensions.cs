/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran.Extensions						            
 *             File: CharExtensions.cs					    		        
 *        Class(es): CharExtensions				         		            
 *          Purpose: Extension methods for char                 
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
