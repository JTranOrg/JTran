/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: EqualOperator.cs					    		        
 *        Class(es): EqualOperator, NotEqualOperator, AndOperator,
 *                      OrOperator
 *          Purpose: Equality operators                   
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

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal class EqualOperator : ComparisonOperator
    {
        public override int Precedence => 9;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return CompareTo(left, right, context) == 0;
        }    
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class NotEqualOperator : ComparisonOperator
    {
        public override int Precedence => 8;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return CompareTo(left, right, context) != 0;
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class AndOperator : IOperator
    {
        public int Precedence => 4;

        /*****************************************************************************/
        public object Evaluate(IExpression left, IExpression right, ExpressionContext context)
        {
            return EvaluateToBool(left, right, context);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return left.EvaluateToBool(context) && right.EvaluateToBool(context);
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class OrOperator : IOperator
    {
        public int Precedence => 3;

        /*****************************************************************************/
        public object Evaluate(IExpression left, IExpression right, ExpressionContext context)
        {
            return EvaluateToBool(left, right, context);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return left.EvaluateToBool(context) || right.EvaluateToBool(context);
        }
    }
}
