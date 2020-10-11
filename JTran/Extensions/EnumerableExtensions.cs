/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: ObjectExtensions.cs					    		        
 *        Class(es): ObjectExtensions				         		            
 *          Purpose: Extension methods for object                 
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

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    internal static class EnumerableExtensions
    {
        /****************************************************************************/
        internal static JArray ToJArray(this IEnumerable list)
        {
            var array = JArray.Parse("[]");

            foreach(var item in list)
            { 
                if(item is ExpandoObject expObject)
                { 
                    var json = expObject.ToJson();
                    var data = JObject.Parse(json);

                    array.Add(data);
                }
                else if(item is string)
                { 
                    array.Add(item);
                }
                else if(item is IEnumerable childList)
                { 
                    array.Add(childList.ToJArray());
                }
                else
                {
                    array.Add(item);
                }
            }

            return array;
        }
    }
}
