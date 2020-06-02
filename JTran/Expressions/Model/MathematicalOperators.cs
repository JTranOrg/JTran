﻿/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
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
    internal abstract class MathematicalOperarator : IOperator
    {
        public abstract int Precedence { get; }

        /*****************************************************************************/
        public object Evaluate(IExpression left, IExpression right, ExpressionContext context)
        {
            var leftVal  = left.Evaluate(context).ToString(); 
            var rightVal = right.Evaluate(context).ToString(); 

            if(long.TryParse(leftVal, out long leftLong))
                if(long.TryParse(rightVal, out long rightLong))
                    return DoLongMath(leftLong, rightLong);

            if(decimal.TryParse(leftVal, out decimal leftDecimal))
                if(decimal.TryParse(rightVal, out decimal rightDecimal))
                    return DoDecimalMath(leftDecimal, rightDecimal);

            if(bool.TryParse(leftVal, out bool leftBool))
                if(bool.TryParse(rightVal, out bool rightBool))
                    return DoBoolMath(leftBool, rightBool);

            return DoStringMath(leftVal, rightVal);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            var leftVal  = left.Evaluate(context).ToString(); 
            var rightVal = right.Evaluate(context).ToString(); 

            if(long.TryParse(leftVal, out long leftLong))
                if(long.TryParse(rightVal, out long rightLong))
                    return DoLongMath(leftLong, rightLong) > 0;

            if(decimal.TryParse(leftVal, out decimal leftDecimal))
                if(decimal.TryParse(rightVal, out decimal rightDecimal))
                    return DoDecimalMath(leftDecimal, rightDecimal) > 0M;

            if(bool.TryParse(leftVal, out bool leftBool))
                if(bool.TryParse(rightVal, out bool rightBool))
                    return DoBoolMath(leftBool, rightBool);

            return !string.IsNullOrEmpty(DoStringMath(leftVal, rightVal));
        }

        protected abstract long    DoLongMath(long left, long right);
        protected abstract decimal DoDecimalMath(decimal left, decimal right);
        protected abstract bool    DoBoolMath(bool left, bool right);
        protected abstract string  DoStringMath(string left, string right);
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class AdditionOperator : MathematicalOperarator
    {
        public override int Precedence => 13;

        protected override long    DoLongMath(long left, long right)            { return left + right; }
        protected override decimal DoDecimalMath(decimal left, decimal right)   { return left + right; }
        protected override bool    DoBoolMath(bool left, bool right)            { return left && right; }
        protected override string  DoStringMath(string left, string right)      { return left + right; }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class SubtractionOperator : MathematicalOperarator
    {
        public override int Precedence => 12;

        protected override long    DoLongMath(long left, long right)            { return left - right; }
        protected override decimal DoDecimalMath(decimal left, decimal right)   { return left - right; }
        protected override bool    DoBoolMath(bool left, bool right)            { return left || right; }
        protected override string  DoStringMath(string left, string right)      { return left.Replace(right, ""); }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class MultiplyOperator : MathematicalOperarator
    {
        public override int Precedence => 15;

        protected override long    DoLongMath(long left, long right)            { return left * right; }
        protected override decimal DoDecimalMath(decimal left, decimal right)   { return left * right; }
        protected override bool    DoBoolMath(bool left, bool right)            { return left && right; }
        protected override string  DoStringMath(string left, string right)      { return left; }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class DivisionOperator : MathematicalOperarator
    {
        public override int Precedence => 14;

        protected override long    DoLongMath(long left, long right)            { return left / right; }
        protected override decimal DoDecimalMath(decimal left, decimal right)   { return left / right; }
        protected override bool    DoBoolMath(bool left, bool right)            { return left && right; }
        protected override string  DoStringMath(string left, string right)      { return left; }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class ModulusOperator : MathematicalOperarator
    {
        public override int Precedence => 13;

        protected override long    DoLongMath(long left, long right)            { return left % right; }
        protected override decimal DoDecimalMath(decimal left, decimal right)   { return left % right; }
        protected override bool    DoBoolMath(bool left, bool right)            { return left && right; }
        protected override string  DoStringMath(string left, string right)      { return left; }
    }
}