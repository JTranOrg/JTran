/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
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
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;

using JTran.Extensions;

namespace JTran.Expressions
{
    internal static class OperatorPrecendence
    {
        internal const int OrOperator                   = 3;
        internal const int AndOperator                  = 4;
        internal const int NotEqualOperator             = 8;
        internal const int EqualOperator                = 9;

        internal const int GreaterThanOperator          = 11;
        internal const int GreaterThanEqualOperator     = 11;
        internal const int LessThanOperator             = 11;
        internal const int LessThanEqualOperator        = 11;

        internal const int SubtractionOperator          = 13;
        internal const int AdditionOperator             = 13;
        internal const int ModulusOperator              = 13;
        internal const int DivisionOperator             = 14;
        internal const int MultiplyOperator             = 15;
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class EqualOperator : ComparisonOperator
    {
        public override int Precedence => OperatorPrecendence.EqualOperator;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext? context)
        {
            return CompareTo(left, right, context) == 0;
        }    
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class NotEqualOperator : ComparisonOperator
    {
        public override int Precedence => OperatorPrecendence.NotEqualOperator;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext? context)
        {
            return CompareTo(left, right, context) != 0;
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class AndOperator : ComparisonOperator
    {
        public override int Precedence => OperatorPrecendence.AndOperator;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return left.EvaluateToBool(context) && right.EvaluateToBool(context);
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class OrOperator : ComparisonOperator
    {
        public override int Precedence => OperatorPrecendence.OrOperator;

        /*****************************************************************************/
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return left.EvaluateToBool(context) || right.EvaluateToBool(context);
        }
    }
}
