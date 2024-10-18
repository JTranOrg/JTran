/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: TMessage.cs					    		        
 *        Class(es): TMessage			         		            
 *          Purpose: Element to output a message to the console                
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 11 Oct 2024                                             
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
using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TAssert : TToken
    {
        private IValue _message;
        private readonly IExpression _expression;
        private readonly long _lineNumber;

        /****************************************************************************/
        internal TAssert(ICharacterSpan condition, ICharacterSpan val, long lineNumber)
        {
            var parms = CompiledTransform.ParseElementParams("#assert", condition, CompiledTransform.SingleFalse );

            _expression = parms[0];
            _message = CreateValue(val, true, lineNumber);

            _lineNumber = lineNumber;
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            if(!_expression.EvaluateToBool(context))
            { 
                var msg = _message.Evaluate(context);

                var foregroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg);
                Console.ForegroundColor = foregroundColor;

                throw new JTran.AssertFailedException(msg.ToString(), _lineNumber);
            }
        }
    }
}
