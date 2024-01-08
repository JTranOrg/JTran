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

using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using JTran.Json;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class JTranModelBuilder : IJsonModelBuilder
    {
        private readonly IDictionary<string, string>? _includeSource;
        private readonly bool _include;
        private readonly TContainer? _parent;

        /****************************************************************************/
        public JTranModelBuilder(IDictionary<string, string>? includeSource = null, bool include = false, TContainer? parent = null) 
        {
            _includeSource = includeSource;
            _include = include;
            _parent = parent;
        }
       
        #region Properties

        /****************************************************************************/
        public object AddObject(string name, object? parent, object? previous)
        {
            if(parent == null)
            { 
                var root = _include ? new IncludedTransform(_includeSource) : new CompiledTransform(_includeSource);

                root.Parent = _parent;

                return root;
            }

            if(parent is TContainer container)
                return container.CreateObject(name, previous);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddArray(string name, object parent)
        {
            if(parent == null)
            { 
                var root = _include ? new IncludedTransform(_includeSource) : new CompiledTransform(_includeSource);

                root.Parent = _parent;

                return root;
            }

            if(parent is TContainer container)
                return container.CreateArray(name);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddText(string name, string val, object parent, object? previous)      
        { 
            if(parent is TContainer container)
                return container.CreateProperty(name, val, previous);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddBoolean(string name, bool val, object parent, object? previous)     
        { 
            if(parent is TContainer container)
                return container.CreateProperty(name, val, previous);

            throw new Transformer.SyntaxException("Invalid container");
        }
        
        /****************************************************************************/
        public object AddNumber(string name, double val, object parent, object? previous)    
        { 
            if(parent is TContainer container)
                return container.CreateProperty(name, val, previous);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddNull(string name, object parent, object? previous)                
        {
            if(parent is TContainer container)
                return container.CreateProperty(name, null, previous);

            throw new Transformer.SyntaxException("Invalid container");
        }

        #endregion

        #region Array Items

        /****************************************************************************/
        public object AddObject(object? parent)
        {
            if(parent is TContainer container)
                return container.CreateObject(null, null);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddArray(object? parent)
        {
            if(parent is TContainer container)
                return container.CreateArray(null);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddText(string val, object parent)      
        { 
            if(parent is TContainer container)
                return container.CreateProperty(null, val, null);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddBoolean(bool val, object parent)     
        { 
            if(parent is TContainer container)
                return container.CreateProperty(null, val, null);

            throw new Transformer.SyntaxException("Invalid container");
        }
        
        /****************************************************************************/
        public object AddNumber(double val, object parent)    
        { 
            if(parent is TContainer container)
                return container.CreateProperty(null, val, null);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddNull(object parent)                
        { 
            if(parent is TContainer container)
                return container.CreateProperty("", null, null);

            throw new Transformer.SyntaxException("Invalid container");
        }

        #endregion
    }
}
