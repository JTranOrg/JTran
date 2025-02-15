/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: ExpressionContext.cs					    		        
 *        Class(es): ExpressionContext			         		            
 *          Purpose: Context while evaluating a transform                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
 *                                                                          
 *  Copyright (c) 2020-2022 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                                                                                                 
 *    http://www.opensource.org/licenses/mit-license.php                    
 * 
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using JTran.Collections;
using JTran.Common;
using JTran.Extensions;
using JTran.Json;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran
{
    /*****************************************************************************/
    /*****************************************************************************/
    public class ExpressionContext
    {
        private readonly IDictionary<ICharacterSpan, object?>      _variables;
        private readonly IDictionary<string, IDocumentRepository>? _docRepositories;
        private readonly ExpressionContext?                        _parent;
        private readonly TransformerContext?                       _transformerContext;

        /*****************************************************************************/
        internal ExpressionContext(object?                         data, 
                                   ICharacterSpan                  name = null, 
                                   TransformerContext?             transformerContext = null, 
                                   ExtensionFunctions?             extensionFunctions = null,
                                   IDictionary<string, TTemplate>? templates          = null,
                                   IDictionary<string, TFunction>? functions          = null,
                                   IDictionary<string, TElement>?  elements           = null)
        {
            this.Data = data;

            _variables          = new Dictionary<ICharacterSpan, object?>();
            _docRepositories    = transformerContext?.DocumentRepositories;
            _parent             = null;
            _transformerContext = transformerContext;

            this.Name           = name;
            this.Templates      = templates;
            this.Functions      = functions;
            this.Elements       = elements;

            this.ExtensionFunctions = extensionFunctions;
        }

        /*****************************************************************************/
        internal ExpressionContext(object?                         data, 
                                   ExpressionContext?              parentContext,
                                   IDictionary<string, TTemplate>? templates = null,
                                   IDictionary<string, TFunction>? functions = null,
                                   IDictionary<string, TElement>?  elements = null)
        {
            this.Data = data;

            _variables         = new Dictionary<ICharacterSpan, object?>();
            _docRepositories   = parentContext?._docRepositories;
            _parent            = parentContext;
            this.CurrentGroup  = parentContext?.CurrentGroup;
            this.Name          = parentContext?.Name ?? CharacterSpan.Empty;
            this.Templates     = templates ?? parentContext?.Templates;
            this.Functions     = functions ?? parentContext?.Functions;
            this.Elements      = elements ?? parentContext?.Elements;
            this.MaxIterations = parentContext?.MaxIterations ?? int.MaxValue;

            this.ExtensionFunctions = parentContext?.ExtensionFunctions;
        }

        /*****************************************************************************/
        internal object?                          Data               { get; set; }
        internal ICharacterSpan                   Name               { get; set; }
        internal bool                             PreviousCondition  { get; set; }
        internal ExtensionFunctions?              ExtensionFunctions { get; }
        internal IDictionary<string, TElement>?   Elements           { get; }
        internal IDictionary<string, TTemplate>?  Templates          { get; }
        internal IDictionary<string, TFunction>?  Functions          { get; }
        internal IList<object>?                   CurrentGroup       { get; set; }
        internal Transformer.UserError?           UserError          { get; set; }
        internal long                             Index              { get; set; } 
        internal int                              MaxIterations      { get; set; } = int.MaxValue;

        /*****************************************************************************/
        internal object GetDocument(string repoName, string docName)
        {
            if(_docRepositories?.ContainsKey(repoName) ?? false)
            { 
                try
                { 
                    var repo = _docRepositories[repoName];

                    if(repo is IDocumentRepository2 repo2)
                    { 
                        using var doc = repo2.GetDocumentStream(docName);
                        var result = doc.ToJsonObject();

                        return result;
                    }
                    else
                    { 
                        var doc    = repo.GetDocument(docName);
                        var result = doc.ToJsonObject();

                        return result;
                    }
                }
                catch(Exception ex)
                {
                    // Eat the exception
                }
            }

            throw new FileNotFoundException($"Document not found {repoName}/{docName}");  
        }

        /*****************************************************************************/
        internal void ClearVariables()
        {
            _variables.Clear();
        }

        /*****************************************************************************/
        internal object GetVariable(ICharacterSpan name, ExpressionContext context)
        {
            if(_variables.ContainsKey(name))
            { 
                var val = _variables[name];

                if(val is IVariable variable)
                {
                    val = variable.GetActualValue(context);

                    if(context.MaxIterations != 1)
                        _variables[name] = val;
                }

                return val;
            }

            if(_parent != null)
                return _parent.GetVariable(name, context);

            if(_transformerContext?.Arguments != null)
            { 
                try
                {
                    return _transformerContext.Arguments[name.ToString()];
                }
                catch(Exception ex)
                {
                    throw new Transformer.SyntaxException($"A variable with that name does not exist: {name}", ex);
                }
            }

            throw new Transformer.SyntaxException($"A variable with that name does not exist: {name}");
        }

        /*****************************************************************************/
        internal TTemplate GetTemplate(string name)
        {
            name = name.ToLower();

            if(this.Templates != null && this.Templates.ContainsKey(name))
                return this.Templates[name];

            return _parent?.GetTemplate(name);
        }

        /*****************************************************************************/
        internal TElement GetElement(string name)
        {
            name = name.ToLower();

            if(this.Elements != null && this.Elements.ContainsKey(name))
                return this.Elements[name];

            return _parent?.GetElement(name);
        }

        /*****************************************************************************/
        internal TTemplate GetTemplateOrElement(string name, bool allowElements, long lineNumber)
        {
            TTemplate? template = null;

            if(allowElements)
                template = this.GetElement(name);

            if(template == null)
                template = this.GetTemplate(name);

            if(template == null)
                throw new Transformer.SyntaxException($"An element or template with that name was not found: {name}") { LineNumber = lineNumber};

            return  template!;
        }

        /*****************************************************************************/
        internal TFunction GetFunction(string name)
        {
            name = name.ToLower();

            if(this.Functions != null && this.Functions.ContainsKey(name))
                return this.Functions[name];

            return _parent?.GetFunction(name);
        }

        /*****************************************************************************/
        internal void SetVariable(ICharacterSpan name, object val)
        {
            if(_variables.ContainsKey(name))
                throw new Transformer.SyntaxException($"A variable with that name already exists in the same scope: {name}");

            _variables.Add(name, val);
        }

        /*****************************************************************************/
        internal void SetOutputVariable(ICharacterSpan name, object val)
        {
            if(_parent != null)
                _parent.SetOutputVariable(name, val);
            else if(_transformerContext != null)
                _transformerContext.SetOutputArgument(name.ToString(), val);
        }

        private static readonly ICharacterSpan _scopeSymbol = CharacterSpan.FromString("@");

        /*****************************************************************************/
        internal object GetDataValue(ICharacterSpan name)
        { 
            if(this.Data == null)
                return null;

            if(name.Equals(_scopeSymbol))
                return this.Data;

            if(this.Data is IObject)
                return this.Data.GetPropertyValue(name);

            if(this.Data is ICollection<object> dict1)
            { 
                if(dict1.Count > 0 && dict1.First() is KeyValuePair<string, object>)
                { 
                    foreach(KeyValuePair<string, object> kv in dict1)
                    {
                        if(name.Equals(kv.Key))
                            return kv.Value;
                    }

                    return null;
                }
            }

            if(this.Data is ICollection<KeyValuePair<string, object>> dict2)
            { 
                foreach(var kv in dict2)
                {
                    if(name.Equals(kv.Key))
                        return kv.Value;
                }

                return null;
            }

            if(this.Data is IDictionary dict)
            { 
                foreach(var key in dict.Keys)
                {
                    if(name.Equals(key.ToString()))
                        return dict[name];
                }

                return null;
            }

            if(this.Data is IEnumerable<object> list)
            {
                var result = new ChildEnumerable<object, object>(list, name); 

                if(!result.Any())
                    return null;

                if(result.IsSingle())
                    return result.First();

                return result;
            }

            return GetDataValue(this.Data, name);
        }

        /*****************************************************************************/
        private object GetDataValue(object data, ICharacterSpan name)
        { 
            var val = data.GetPropertyValue(name);

            if(val == null)
                return null;

            if(!val.GetType().IsClass)
            { 
                // If it's any kind of number return it as a decimal
                if(val.TryParseDecimal(out decimal dVal)) 
                    return dVal;
            }

            return val;
        }
    }

}
