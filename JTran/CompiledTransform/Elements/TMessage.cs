﻿/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: TMessage.cs					    		        
 *        Class(es): TMessage			         		            
 *          Purpose: Element to output a message to the console                
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 08 Jan 2024                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                                                                                                 
 *    http://www.opensource.org/licenses/mit-license.php                    
 * 
 ****************************************************************************/

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using JTran.Common;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TMessage : TToken
    {
        private IValue _message;

        /****************************************************************************/
        internal TMessage(ICharacterSpan val, long lineNumber)
        {
            _message = CreateValue(val, true, lineNumber);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var msg = _message.Evaluate(context);

            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
    }
}
