/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: TBind.cs					    		        
 *        Class(es): TBind			         		            
 *          Purpose: Element to change scope                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 8 Jan 2024                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                                                                                                 
 *    http://www.opensource.org/licenses/mit-license.php                    
 * 
 ****************************************************************************/

using System;

using JTran.Common;
using JTran.Expressions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TBind : TContainer
    {
        private readonly IExpression _expression;

        /****************************************************************************/
        internal TBind(CharacterSpan name) 
        {
            var parms = CompiledTransform.ParseElementParams("#bind", name, CompiledTransform.SingleFalse );

            _expression = parms[0];
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var newScope   = _expression.Evaluate(context);
            var newContext = new ExpressionContext(data: newScope, parentContext: context, templates: this.Templates, functions: this.Functions);

            base.Evaluate(output, newContext, wrap);
        }
    }
}
