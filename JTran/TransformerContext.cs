/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: TransformerContext.cs					    		        
 *        Class(es): TransformerContext				         		            
 *          Purpose: Does a tranformation in context (not stateless)                   
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
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JTran.Extensions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    public class TransformerContext
    {
        public IDictionary<string, object>              Arguments             { get; set; }
        public IDictionary<string, IDocumentRepository> DocumentRepositories  { get; set; } = new Dictionary<string, IDocumentRepository>();
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IDocumentRepository
    {
        string GetDocument(string name);
    }
}
