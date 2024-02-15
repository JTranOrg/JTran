/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: TertiaryExpression.cs					    		        
 *        Class(es): TertiaryExpression
 *          Purpose: Tertiary expression              
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

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal class TertiaryExpression : IExpression
    {
        public IExpression Conditional    { get; set; }
        public IExpression IfTrue         { get; set; }
        public IExpression IfFalse        { get; set; }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            if(Conditional.EvaluateToBool(context))
                return IfTrue.Evaluate(context);

            return IfFalse.Evaluate(context);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext context)
        {
            return Convert.ToBoolean(Evaluate(context));
        }

        /*****************************************************************************/
        public bool IsConditional(ExpressionContext context)
        {
            return this.IfTrue.IsConditional(context);
        }
    }
}
