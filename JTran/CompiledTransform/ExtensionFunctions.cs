/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: ExtensionFunctions.cs					    		        
 *        Class(es): ExtensionFunctions			         		            
 *          Purpose: Extension functions                   
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

using JTran.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JTran
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal class ExtensionFunctions
    {
        private readonly IDictionary<string, Function> _functions = new Dictionary<string, Function>();

        /*****************************************************************************/
        internal ExtensionFunctions(IList<object> containers)
        {
            foreach(var container in containers)
            {
                var list = Function.Extract(container);

                foreach(var func in list)
                {
                    var key = CreateKey(func.Name, func.IgnoreParams ? -1 : func.NumParams);

                    if(!_functions.ContainsKey(key))
                        _functions.Add(key, func);
                }
            }
        }

        /*****************************************************************************/
        internal Function GetFunction(string name, int numParams)
        {
            var key = CreateKey(name, numParams);

            if(_functions.ContainsKey(key))
                return _functions[key];

            key = CreateKey(name, -1);

            if(_functions.ContainsKey(key))
            {
                var fn = _functions[key];

                if(fn.IgnoreParams) 
                    return fn;
            }

            return null;
        }

        /*****************************************************************************/
        private string CreateKey(string name, int numParams)
        {
            if(numParams == -1) 
                return name;

            if(name == "document" || name == "sort" || name == "coalesce")
                return name;

            return name + "_" + numParams;
        }
    }
}
