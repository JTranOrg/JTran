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
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
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
        public override bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return CompareTo(left, right, context) != 0;
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class AndOperator : IOperator
    {
        public int Precedence => OperatorPrecendence.AndOperator;

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
        public int Precedence => OperatorPrecendence.OrOperator;

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

    /*****************************************************************************/
    /*****************************************************************************/
    internal class DataPart : IOperator
    {
        public int Precedence => int.MaxValue;

        /*****************************************************************************/
        public object Evaluate(IExpression left, IExpression right, ExpressionContext context)
        {
            var val = left.Evaluate(context);

            return val.GetValue((right as DataValue).Name, context);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            return Convert.ToBoolean(Evaluate(left, right, context));
        }
    }
}
