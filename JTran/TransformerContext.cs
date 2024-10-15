/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: TransformerContext.cs					    		        
 *        Class(es): TransformerContext				         		            
 *          Purpose: Context for transforms                
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
using System.Collections.Generic;
using System.IO;
using JTran.Collections;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    public class TransformerContext
    {
        public IReadOnlyDictionary<string, object>?      Arguments               { get; set; }
        public IDictionary<string, IDocumentRepository>? DocumentRepositories    { get; set; } = new Dictionary<string, IDocumentRepository>();
        public bool                                      AllowDeferredLoading    { get; set; } = true;
        public IReadOnlyDictionary<string, object>       OutputArguments         => _internalOutputArguments;
        public Action<string, object>?                   OnOutputArgument        { get; set; }

        // Copy constructor
        public TransformerContext(TransformerContext copy)
        {
            Arguments               = copy.Arguments;
            DocumentRepositories    = copy.DocumentRepositories;
            AllowDeferredLoading    = copy.AllowDeferredLoading;
            OnOutputArgument        = copy.OnOutputArgument;
        }

        public TransformerContext()
        {
        }

        #region Internal

        private RestrictedAccessDictionary _internalOutputArguments = new();

        /****************************************************************************/
        internal void SetOutputArguments(TransformerContext otherContext)
        {
            _internalOutputArguments = otherContext._internalOutputArguments;
        }

        /****************************************************************************/
        internal void SetOutputArgument(string key, object value)   
        {
            _internalOutputArguments.SetValue(key, value);

            if(OnOutputArgument != null)
            {
                try
                {
                    OnOutputArgument(key, value);
                }
                catch
                {
                }
            }
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IDocumentRepository
    {
        string GetDocument(string name);
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IDocumentRepository2 : IDocumentRepository
    {
        Stream GetDocumentStream(string name);
    }
}
