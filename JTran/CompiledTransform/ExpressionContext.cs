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
 *   Copyright (c) 2020-2022 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                                                                                                 
 *    http://www.opensource.org/licenses/mit-license.php                    
 * 
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using JTran.Extensions;
using JTran.Json;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran
{
    /*****************************************************************************/
    /*****************************************************************************/
    public class ExpressionContext
    {
        private readonly object                                    _data;
        private readonly IDictionary<string, object>               _variables;
        private readonly IDictionary<string, IDocumentRepository>? _docRepositories;
        private readonly ExpressionContext?                        _parent;

        /*****************************************************************************/
        internal ExpressionContext(object                          data, 
                                   string                          name = "", 
                                   TransformerContext?             transformerContext = null, 
                                   ExtensionFunctions?             extensionFunctions = null,
                                   IDictionary<string, TTemplate>? templates          = null,
                                   IDictionary<string, TFunction>? functions          = null)
        {
            _data            = data;
            _variables       = transformerContext?.Arguments ?? new Dictionary<string, object>();
            _docRepositories = transformerContext?.DocumentRepositories;
            _parent          = null;

            this.Name        = name;
            this.Templates   = templates;
            this.Functions   = functions;

            this.ExtensionFunctions = extensionFunctions;
        }

        /*****************************************************************************/
        internal ExpressionContext(object data, 
                                   ExpressionContext              parentContext,
                                   IDictionary<string, TTemplate> templates = null,
                                   IDictionary<string, TFunction> functions = null)
        {
            _data             = data;
            _variables        = new Dictionary<string, object>();
            _docRepositories  = parentContext?._docRepositories;
            _parent           = parentContext;
            this.CurrentGroup = parentContext?.CurrentGroup;
            this.Name         = parentContext?.Name ?? "";
            this.Templates    = templates ?? parentContext?.Templates;
            this.Functions    = functions ?? parentContext?.Functions;

            this.ExtensionFunctions = parentContext?.ExtensionFunctions;
        }

        /*****************************************************************************/
        internal object                           Data               => _data;
        internal string                           Name               { get; }
        internal bool                             PreviousCondition  { get; set; }
        internal ExtensionFunctions?              ExtensionFunctions { get; }
        internal IDictionary<string, TTemplate>?  Templates          { get; }
        internal IDictionary<string, TFunction>?  Functions          { get; }
        internal IList<object>?                   CurrentGroup       { get; set; }
        internal Transformer.UserError?           UserError          { get; set; }

        /*****************************************************************************/
        internal object GetDocument(string repoName, string docName)
        {
            if(_docRepositories?.ContainsKey(repoName) ?? false)
            { 
                try
                { 
                    using var doc = _docRepositories[repoName].GetDocumentStream(docName);

                    return doc.JsonToExpando();
                }
                catch(Exception ex)
                {
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
        internal object GetVariable(string name, ExpressionContext context)
        {
            if(_variables.ContainsKey(name))
            { 
                var val = _variables[name];

                if(val is IVariable variable)
                {
                    val = variable.GetActualValue(context);

                    _variables[name] = val;
                }

                return val;
            }

            if(_parent == null)
                throw new Transformer.SyntaxException($"A variable with that name does not exist: {name}");

            return _parent.GetVariable(name, context);
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
        internal TFunction GetFunction(string name)
        {
            name = name.ToLower();

            if(this.Functions != null && this.Functions.ContainsKey(name))
                return this.Functions[name];

            return _parent?.GetFunction(name);
        }

        /*****************************************************************************/
        internal void SetVariable(string name, object val)
        {
            if(_variables.ContainsKey(name))
                throw new Transformer.SyntaxException($"A variable with that name already exists in the same scope: {name}");

            _variables.Add(name, val);
        }

        /*****************************************************************************/
        internal object GetDataValue(string name)
        { 
            if(_data == null)
                return null;

            if(name == "@")
                return _data;

            var otype = _data.GetType();

            if(_data is ExpandoObject)
                return GetDataValue(_data, name);

            if(_data is ICollection<object> dict1)
            { 
                if(dict1.Count > 0 && dict1.First() is KeyValuePair<string, object>)
                { 
                    foreach(KeyValuePair<string, object> kv in dict1)
                    {
                        if(kv.Key == name)
                            return kv.Value;
                    }

                    return null;
                }
            }

            if(_data is ICollection<KeyValuePair<string, object>> dict2)
            { 
                foreach(var kv in dict2)
                {
                    if(kv.Key == name)
                        return kv.Value;
                }

                return null;
            }

            if(_data is IDictionary dict)
            { 
                foreach(var key in dict.Keys)
                {
                    if(key.ToString() == name)
                        return dict[name];
                }

                return null;
            }

            if(_data is IEnumerable<object> list)
            {
                var result = new List<object>();

                foreach(var child in list)
                { 
                    var childResult = GetDataValue(child, name);

                    if(childResult is IEnumerable<object> childList)
                        result.AddRange(childList);
                    else
                        result.Add(childResult);
                }

                if(result.Count == 0)
                    return null;

                if(result.Count == 1)
                    return result[0];

                return result;
            }

            return GetDataValue(_data, name);
        }

        /*****************************************************************************/
        private object GetDataValue(object data, string name)
        { 
            var val = data.GetValue(name, this);

            if(val == null)
                return null;

            if(!val.GetType().IsClass)
            { 
                // If it's any kind of number return it as a double
                if(double.TryParse(val.ToString(), out double dVal))
                    return dVal;
            }

            return val;
        }
    }

}
