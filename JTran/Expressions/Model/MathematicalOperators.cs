/***************************************************************************
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

using JTran.Common;
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

            if(leftVal is bool bLeft && rightVal is bool bRight)
                return DoBoolMath(bLeft, bRight);

            if(leftVal is long lLeft && rightVal is long lRight)
                return DoLongMath(lLeft, lRight);

            if(leftVal is int iLeft && rightVal is int iRight)
                return DoLongMath(iLeft, iRight);

            if(leftVal is short sLeft && rightVal is short sRight)
                return DoLongMath(sLeft, sRight);

            if(leftVal is ICharacterSpan cspanLeft)
                return DoSpanMath(cspanLeft, rightVal.AsCharacterSpan());

            if(leftVal is string strLeft)
                return DoStringMath(strLeft, rightVal?.ToString() ?? "");

            if(leftVal.TryParseDecimal(out decimal dLeft) && rightVal.TryParseDecimal(out decimal dRight))
                return DoDecimalMath(dLeft, dRight);

            return DoStringMath(leftVal?.ToString() ?? "", rightVal?.ToString() ?? "");
        }

        /*****************************************************************************/
        public bool EvaluateToBool(IExpression left, IExpression right, ExpressionContext context)
        {
            var result = Evaluate(left, right, context);

            if(result is bool bVal)
                return bVal;

            if(result is decimal dVal )
                return dVal > 0m;

            if(result is long lVal)
                return lVal > 0M;

            if(result is int iVal)
                return iVal > 0;

            if(result is short sVal)
                return sVal > 0;

            if(result is ICharacterSpan cspan)
                return cspan.IsNullOrWhiteSpace();

            if(result is string str)
                return string.IsNullOrWhiteSpace(str);

            return !string.IsNullOrWhiteSpace(result.ToString());
        }

        protected abstract long            DoLongMath(long left, long right);
        protected abstract decimal         DoDecimalMath(decimal left, decimal right);
        protected abstract bool            DoBoolMath(bool left, bool right);
        protected abstract string          DoStringMath(string left, string right);
        protected abstract ICharacterSpan  DoSpanMath(ICharacterSpan left, ICharacterSpan right);
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class AdditionOperator : MathematicalOperator
    {
        public override int Precedence => OperatorPrecendence.AdditionOperator;

        protected override long             DoLongMath(long left, long right)                       { return left + right; }
        protected override decimal          DoDecimalMath(decimal left, decimal right)              { return left + right; }
        protected override bool             DoBoolMath(bool left, bool right)                       { return left && right; }
        protected override string           DoStringMath(string left, string right)                 { return left + right; }
        protected override ICharacterSpan   DoSpanMath(ICharacterSpan left, ICharacterSpan right)   { return left.Concat(right); }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class SubtractionOperator : MathematicalOperator
    {
        public override int Precedence => OperatorPrecendence.SubtractionOperator;

        protected override long             DoLongMath(long left, long right)                       { return left - right; }
        protected override decimal          DoDecimalMath(decimal left, decimal right)              { return left - right; }
        protected override bool             DoBoolMath(bool left, bool right)                       { return left || right; }
        protected override string           DoStringMath(string left, string right)                 { return left.Replace(right, ""); }
        protected override ICharacterSpan   DoSpanMath(ICharacterSpan left, ICharacterSpan right)   { return left.Remove(right); }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class MultiplyOperator : MathematicalOperator
    {
        public override int Precedence => OperatorPrecendence.MultiplyOperator;

        protected override long             DoLongMath(long left, long right)                       { return left * right; }
        protected override decimal          DoDecimalMath(decimal left, decimal right)              { return left * right; }
        protected override bool             DoBoolMath(bool left, bool right)                       { return left && right; }
        protected override string           DoStringMath(string left, string right)                 { return left; }
        protected override ICharacterSpan   DoSpanMath(ICharacterSpan left, ICharacterSpan right)   { return left; }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class DivisionOperator : MathematicalOperator
    {
        public override int Precedence => OperatorPrecendence.DivisionOperator;

        protected override long             DoLongMath(long left, long right)                       { return left / right; }
        protected override decimal          DoDecimalMath(decimal left, decimal right)              { return left / right; }
        protected override bool             DoBoolMath(bool left, bool right)                       { return left && right; }
        protected override string           DoStringMath(string left, string right)                 { return left; }
        protected override ICharacterSpan   DoSpanMath(ICharacterSpan left, ICharacterSpan right)   { return left; }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class ModuloOperator : MathematicalOperator
    {
        public override int Precedence => OperatorPrecendence.ModulusOperator;

        protected override long             DoLongMath(long left, long right)                       { return left % right; }
        protected override decimal          DoDecimalMath(decimal left, decimal right)              { return left % right; }
        protected override bool             DoBoolMath(bool left, bool right)                       { return left && right; }
        protected override string           DoStringMath(string left, string right)                 { return left; }
        protected override ICharacterSpan   DoSpanMath(ICharacterSpan left, ICharacterSpan right)   { return left; }
    }
}
