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
using System.Runtime.CompilerServices;
using JTran.Common;
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
        public object AddObject(CharacterSpan name, object? parent, object? previous, long lineNumber)
        {
            if(parent == null)
            { 
                var root = _include ? new IncludedTransform(_includeSource) : new CompiledTransform(_includeSource);

                root.Parent = _parent;

                return root;
            }

            if(parent is TContainer container)
                return container.CreateObject(name, previous, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddArray(CharacterSpan name, object parent, long lineNumber)
        {
            if(parent == null)
            { 
                var root = _include ? new IncludedTransform(_includeSource) : new CompiledTransform(_includeSource);

                root.Parent = _parent;

                return root;
            }

            if(parent is TContainer container)
                return container.CreateArray(name, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddText(CharacterSpan name, CharacterSpan val, object parent, object? previous, long lineNumber)      
        { 
            if(parent is TContainer container)
                return container.CreateProperty(name, val, previous, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddBoolean(CharacterSpan name, bool val, object parent, object? previous, long lineNumber)     
        { 
            if(parent is TContainer container)
                return container.CreateProperty(name, val, previous, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }
        
        /****************************************************************************/
        public object AddNumber(CharacterSpan name, decimal val, object parent, object? previous, long lineNumber)    
        { 
            if(parent is TContainer container)
                return container.CreateProperty(name, val, previous, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddNull(CharacterSpan name, object parent, object? previous, long lineNumber)                
        {
            if(parent is TContainer container)
                return container.CreateProperty(name, null, previous, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }

        #endregion

        #region Array Items

        /****************************************************************************/
        public object AddObject(object? parent, long lineNumber)
        {
            if(parent is TContainer container)
                return container.CreateObject(null, null, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddArray(object? parent, long lineNumber)
        {
            if(parent is TContainer container)
                return container.CreateArray(null, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddText(CharacterSpan val, object parent, long lineNumber)      
        { 
            if(parent is TContainer container)
                return container.CreateProperty(null, val, null, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddBoolean(bool val, object parent, long lineNumber)     
        { 
            if(parent is TContainer container)
                return container.CreateProperty(null, val, null, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }
        
        /****************************************************************************/
        public object AddNumber(decimal val, object parent, long lineNumber)    
        { 
            if(parent is TContainer container)
                return container.CreateProperty(null, val, null, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }

        /****************************************************************************/
        public object AddNull(object parent, long lineNumber)                
        { 
            if(parent is TContainer container)
                return container.CreateProperty(CharacterSpan.Empty, null, null, lineNumber);

            throw new Transformer.SyntaxException("Invalid container");
        }

        #endregion
    }
}
