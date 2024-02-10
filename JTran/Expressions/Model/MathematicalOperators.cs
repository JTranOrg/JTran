﻿/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: MathematicalOperarator.cs					    		        
 *        Class(es): MathematicalOperarator, AdditionOperator, SubtractionOperator, MultiplyOperator, DivisionOperator, 
 *                      ModulusOperator
 *          Purpose: Mathematical operarator s                
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
using System.Collections.Generic;
using System.Text;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal abstract class MathematicalOperator : IOperator
    {
        public abstract int Precedence { get; }

        /*****************************************************************************/
        public object Evaluate(IExpression left, IExpression right, ExpressionContext context)
        {
            var leftVal     = left.Evaluate(context);
            var rightVal    = right.Evaluate(context);
            var leftValStr  = leftVal?.ToString(); 
            var rightValStr = rightVal?.ToString(); 

            if(!(leftVal is StringValue || rightVal is StringValue))
            { 
                if(long.TryParse(leftValStr, out long leftLong))
                    if(long.TryParse(rightValStr, out long rightLong))
                        return DoLongMath(leftLong, rightLong);

                if(double.TryParse(leftValStr, out double leftdouble))
                    if(double.TryParse(rightValStr, out double rightdouble))
                        return DoDoubleMath(leftdouble, rightdouble);

                if(bool.TryParse(leftValStr, out bool leftBool))
                    if(bool.TryParse(rightValStr, out bool rightBool))
                        return DoBoolMath(leftBool, rightBool);
            }

            return DoStringMath(leftValStr, rightValStr);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            var leftVal  = left.Evaluate(context).ToString(); 
            var rightVal = right.Evaluate(context).ToString(); 

            if(long.TryParse(leftVal, out long leftLong))
                if(long.TryParse(rightVal, out long rightLong))
                    return DoLongMath(leftLong, rightLong) > 0;

            if(double.TryParse(leftVal, out double leftdouble))
                if(double.TryParse(rightVal, out double rightdouble))
                    return DoDoubleMath(leftdouble, rightdouble) > 0d;

            if(bool.TryParse(leftVal, out bool leftBool))
                if(bool.TryParse(rightVal, out bool rightBool))
                    return DoBoolMath(leftBool, rightBool);

            return !string.IsNullOrEmpty(DoStringMath(leftVal, rightVal));
        }

        protected abstract long    DoLongMath(long left, long right);
        protected abstract double  DoDoubleMath(double left, double right);
        protected abstract bool    DoBoolMath(bool left, bool right);
        protected abstract string  DoStringMath(string left, string right);
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class AdditionOperator : MathematicalOperator
    {
        public override int Precedence => OperatorPrecendence.AdditionOperator;

        protected override long    DoLongMath(long left, long right)            { return left + right; }
        protected override double  DoDoubleMath(double left, double right)      { return left + right; }
        protected override bool    DoBoolMath(bool left, bool right)            { return left && right; }
        protected override string  DoStringMath(string left, string right)      { return left + right; }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class SubtractionOperator : MathematicalOperator
    {
        public override int Precedence => OperatorPrecendence.SubtractionOperator;

        protected override long    DoLongMath(long left, long right)            { return left - right; }
        protected override double  DoDoubleMath(double left, double right)      { return left - right; }
        protected override bool    DoBoolMath(bool left, bool right)            { return left || right; }
        protected override string  DoStringMath(string left, string right)      { return left.Replace(right, ""); }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class MultiplyOperator : MathematicalOperator
    {
        public override int Precedence => OperatorPrecendence.MultiplyOperator;

        protected override long    DoLongMath(long left, long right)            { return left * right; }
        protected override double  DoDoubleMath(double left, double right)      { return left * right; }
        protected override bool    DoBoolMath(bool left, bool right)            { return left && right; }
        protected override string  DoStringMath(string left, string right)      { return left; }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class DivisionOperator : MathematicalOperator
    {
        public override int Precedence => OperatorPrecendence.DivisionOperator;

        protected override long    DoLongMath(long left, long right)            { return left / right; }
        protected override double  DoDoubleMath(double left, double right)      { return left / right; }
        protected override bool    DoBoolMath(bool left, bool right)            { return left && right; }
        protected override string  DoStringMath(string left, string right)      { return left; }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class ModuloOperator : MathematicalOperator
    {
        public override int Precedence => OperatorPrecendence.ModulusOperator;

        protected override long    DoLongMath(long left, long right)            { return left % right; }
        protected override double  DoDoubleMath(double left, double right)      { return left % right; }
        protected override bool    DoBoolMath(bool left, bool right)            { return left && right; }
        protected override string  DoStringMath(string left, string right)      { return left; }
    }
}
