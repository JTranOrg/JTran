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

using System.Collections.Generic;
using System.IO;

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
        Stream GetDocumentStream(string name);
    }
}
