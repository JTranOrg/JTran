/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: ExpressionContext.cs					    		        
 *        Class(es): ExpressionContext			         		            
 *          Purpose: Context while evaluating a transform                   
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;

using Newtonsoft.Json.Linq;

using JTran.Extensions;
using JTran.Expressions;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran
{
    /*****************************************************************************/
    /*****************************************************************************/
    public class ExpressionContext
    {
        private readonly object                                   _data;
        private readonly IDictionary<string, object>              _variables;
        private readonly IDictionary<string, IDocumentRepository> _docRepositories;
        private readonly ExpressionContext                        _parent;

        /*****************************************************************************/
        internal ExpressionContext(object                         data, 
                                   string                         name = "", 
                                   TransformerContext             transformerContext = null, 
                                   ExtensionFunctions             extensionFunctions = null,
                                   IDictionary<string, TTemplate> templates          = null)
        {
            _data            = data;
            _variables       = transformerContext?.Arguments != null ? new Dictionary<string, object>(transformerContext?.Arguments) : new Dictionary<string, object>();
            _docRepositories = transformerContext?.DocumentRepositories;
            _parent          = null;

            this.Name        = name;
            this.Templates   = templates;

            this.ExtensionFunctions = extensionFunctions;
        }

        /*****************************************************************************/
        internal ExpressionContext(object data, 
                                   ExpressionContext              parentContext,
                                   IDictionary<string, TTemplate> templates = null)
        {
            _data             = data;
            _variables        = new Dictionary<string, object>();
            _docRepositories  = parentContext?._docRepositories;
            _parent           = parentContext;
            this.CurrentGroup = parentContext?.CurrentGroup;
            this.Name         = parentContext?.Name ?? "";
            this.Templates    = templates;

            this.ExtensionFunctions = parentContext?.ExtensionFunctions;
        }

        /*****************************************************************************/
        internal object                         Data               => _data;
        internal string                         Name               { get; }
        internal bool                           PreviousCondition  { get; set; }
        internal ExtensionFunctions             ExtensionFunctions { get; }
        internal IDictionary<string, TTemplate> Templates          { get; }
        internal IList<object>                  CurrentGroup       { get; set; }

        /*****************************************************************************/
        internal object GetDocument(string repoName, string docName)
        {
            if(_docRepositories?.ContainsKey(repoName) ?? false)
            { 
                var doc = _docRepositories[repoName].GetDocument(docName);
                    
                return doc.JsonToExpando();
            }

            return null;
        }

        /*****************************************************************************/
        internal object GetVariable(string name)
        {
            if(_variables.ContainsKey(name))
                return _variables[name];

            if(_parent == null)
                throw new Transformer.SyntaxException($"A variable with that name does not exist: {name}");

            return _parent.GetVariable(name);
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
        internal void SetVariable(string name, object val)
        {
            _variables.Add(name, val);
        }

        /*****************************************************************************/
        internal object GetDataValue(string name)
        { 
            if(_data == null)
                return null;

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

            if(_data is IList<object> list)
            {
                var result = new List<object>();

                foreach(var child in list)
                { 
                    var childResult = GetDataValue(child, name);

                    if(childResult is IList<object> childList)
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
        private static object GetDataValue(object data, string name)
        { 
            var val = data.GetValue(name, null);

            if(val == null)
                return null;

            if(!val.GetType().IsClass)
            { 
                // If it's any kind of number return it as a decimal
                if(decimal.TryParse(val.ToString(), out decimal dVal))
                    return dVal;
            }

            return val;
        }
    }

}
