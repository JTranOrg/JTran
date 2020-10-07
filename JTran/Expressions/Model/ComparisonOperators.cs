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
 *   Copyright (c) 2020 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal abstract class ComparisonOperarator : IOperator
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
            var leftVal  = left.Evaluate(context)?.ToString(); 
            var rightVal = right.Evaluate(context)?.ToString(); 

            if(leftVal == null && rightVal == null)
                return 0;

            if(long.TryParse(leftVal, out long leftLong))
                if(long.TryParse(rightVal, out long rightLong))
                    return leftLong.CompareTo(rightLong);

            if(decimal.TryParse(leftVal, out decimal leftDecimal))
                if(decimal.TryParse(rightVal, out decimal rightDecimal))
                    return leftDecimal.CompareTo(rightDecimal);

            if(bool.TryParse(leftVal, out bool leftBool))
                if(bool.TryParse(rightVal, out bool rightBool))
                    return leftBool.CompareTo(rightBool);

            if(leftVal != null)
                return leftVal.CompareTo(rightVal);

            return -(rightVal.CompareTo(leftVal));
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class GreaterThanOperator : ComparisonOperarator
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
    internal class GreaterThanEqualOperator : ComparisonOperarator
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
    internal class LessThanOperator : ComparisonOperarator
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
    internal class LessThanEqualOperator : ComparisonOperarator
    {
        public override int Precedence => 11;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return CompareTo(left, right, context) <= 0;
        }    
    }
}
