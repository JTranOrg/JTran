/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: CompiledTransform.cs					    		        
 *        Class(es): CompiledTransform			         		            
 *          Purpose: Compiles and evaluates tranformations                   
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using JTran.Extensions;
using JTran.Expressions;
using JTran.Json;
using JTran.Parser;

using JTranParser = JTran.Parser.Parser;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class JTranModelBuilder : IJsonModelBuilder
    {
        private IDictionary<string, string>? _includeSource;

        /****************************************************************************/
        public JTranModelBuilder(IDictionary<string, string>? includeSource = null) 
        {
            _includeSource = includeSource;
        }
       
        #region Properties

        /****************************************************************************/
        public object AddObject(string name, object? parent)
        {
            if(parent == null)
                return new CompiledTransform(_includeSource);

            if(parent is TContainer container)
                return container.CreateObject(name);

            throw new Transformer.SyntaxException("What the heck is this?!");
        }

        /****************************************************************************/
        public object AddArray(string name, object parent)
        {
            if(parent == null)
                return new CompiledTransform(_includeSource);

            if(parent is TContainer container)
                return container.CreateArray(name, null);

            throw new Transformer.SyntaxException("What the heck is this?!");
        }

        /****************************************************************************/
        public object AddText(string name, string val, object parent)      
        { 
            if(parent is TContainer container)
                return container.CreateProperty(name, val);

            throw new Transformer.SyntaxException("What the heck is this?!");
        }

        /****************************************************************************/
        public object AddBoolean(string name, bool val, object parent)     
        { 
            if(parent is TContainer container)
                return container.CreateProperty(name, val);

            throw new Transformer.SyntaxException("What the heck is this?!");
        }
        
        /****************************************************************************/
        public object AddNumber(string name, double val, object parent)    
        { 
            if(parent is TContainer container)
                return container.CreateProperty(name, val);

            throw new Transformer.SyntaxException("What the heck is this?!");
        }

        /****************************************************************************/
        public object AddNull(string name, object parent)                
        {
            if(parent is TContainer container)
                return container.CreateProperty(name, null);

            throw new Transformer.SyntaxException("What the heck is this?!");
        }

        #endregion

        #region Array Items

        /****************************************************************************/
        public object AddObject(object? parent)
        {
            var newObj = new ExpandoObject();

            if(parent is IList<object> list)
                list.Add(newObj);

            return newObj;
        }

        /****************************************************************************/
        public object AddArray(object? parent)
        {
            var newArr = new List<object>();

            if(parent is IList<object> list)
                list.Add(newArr);

            return newArr;
        }

        /****************************************************************************/
        public object AddText(string val, object parent)      
        { 
            if(parent is IList<object> list)
                list.Add(val);

            return val; 
        }

        /****************************************************************************/
        public object AddBoolean(bool val, object parent)     
        { 
            if(parent is IList<object> list)
                list.Add(val);

            return val; 
        }
        
        /****************************************************************************/
        public object AddNumber(double val, object parent)    
        { 
            if(parent is IList<object> list)
                list.Add(val);

            return val; 
        }

        /****************************************************************************/
        public object AddNull(object parent)                
        { 
            if(parent is IList<object> list)
                list.Add(null);

            return (object)null; 
        }

        #endregion
    }
}
