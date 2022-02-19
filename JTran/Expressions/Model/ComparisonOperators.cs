/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: ComparisonOperarator.cs					    		        
 *        Class(es): ComparisonOperarator, GreaterThanOperator, GreaterThanEqualOperator,
 *                      LessThanOperator, LessThanEqualOperator, 
 *          Purpose: Comparison operators                   
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

using JTran.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal abstract class ComparisonOperator : IOperator
    {
        public abstract int Precedence { get; }

        /*****************************************************************************/
        public object Evaluate(IExpression left, IExpression right, ExpressionContext context)
        {
            return EvaluateToBool(left, right, context);
        }

        /*****************************************************************************/
        public abstract bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context);

        /*****************************************************************************/
        protected int CompareTo(IExpression left, IExpression right, ExpressionContext context)
        {
            var leftVal  = left.Evaluate(context);
            var rightVal = right.Evaluate(context);

            return leftVal.CompareTo(rightVal, out Type type);
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class GreaterThanOperator : ComparisonOperator
    {
        public override int Precedence => 11;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return CompareTo(left, right, context) > 0;
        }    
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class GreaterThanEqualOperator : ComparisonOperator
    {
        public override int Precedence => 11;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return CompareTo(left, right, context) >= 0;
        }    
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class LessThanOperator : ComparisonOperator
    {
        public override int Precedence => 11;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return CompareTo(left, right, context) < 0;
        }    
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class LessThanEqualOperator : ComparisonOperator
    {
        public override int Precedence => 11;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return CompareTo(left, right, context) <= 0;
        }    
    }
}
