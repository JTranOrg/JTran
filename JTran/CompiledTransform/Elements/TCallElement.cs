using System;
using System.Linq;
using System.Collections.Generic;

using JTran.Expressions;
using JTran.Extensions;
using JTran.Json;
using JTran.Common;
using System.Diagnostics;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TCallElement : TContainer
    {
        private readonly IExpression  _elementName;
        private readonly IValue?      _valueExpression;
        private readonly long         _lineNumber;
        private readonly IList<IExpression> _parms;
        private TElement?             _element;

        /****************************************************************************/
        internal TCallElement(ICharacterSpan name, long lineNumber = -1L, ICharacterSpan? value = null) 
        {
            var expr = name.Substring("#callelement(".Length).ToString()!.ReplaceEnding(")", "").SubstringBefore(",");

            _elementName  = Compiler.Compile(CharacterSpan.FromString(expr));
            _lineNumber    = lineNumber;

            if(value != null)
                _valueExpression = CreateValue(value, true, lineNumber);

            var parms = CompiledTransform.ParseElementParams("#callelement", name, CompiledTransform.TrueFalse);

            _parms = parms.Skip(1).ToList();
        }

        /****************************************************************************/
        private TTemplate GetElement(ExpressionContext context)
        {
            if(_element == null)
            { 
                var elementName = _elementName.Evaluate(context);
                
                _element = context.GetElement(elementName.ToString()!);
            }

            return _element;
        }

        private readonly static ICharacterSpan kValue = CharacterSpan.FromString("__value");

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var element     = GetElement(context);
            var newContext   = new ExpressionContext(context.Data, context);
            var paramsOutput = new JsonStringWriter();

            paramsOutput.StartObject();
            base.Evaluate(paramsOutput, newContext, (fnc)=> fnc()); 
            paramsOutput.EndObject();

            if(_valueExpression != null)
            {
                var val = _valueExpression.Evaluate(context);

                newContext.SetVariable(kValue, val);
            }

            var numParms = element.Parameters.Count;

            if(numParms > 0) 
            {
                for(var i = 0; i < numParms; ++i)
                { 
                    if(_parms.Count <= i)
                        break;

                    newContext.SetVariable(element.Parameters[i], _parms[i].Evaluate(context));
                }
            }

            if(_valueExpression == null)
            { 
                var jsonParams = paramsOutput.ToString().JTranToJsonObject(); 

                jsonParams.Parent = newContext.Data;
                newContext.Data = jsonParams;
            }

            element.Evaluate(output, newContext, wrap);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TCallElementProperty : IValue, IEvaluator
    {
        private readonly IExpression  _elementName;
        private readonly IList<IExpression> _parms;
        private readonly long _lineNumber;
        private TElement?     _element;
        private bool?         _isReturnValue;

        internal TCallElementProperty(ICharacterSpan name, long lineNumber, bool allowElements = false) 
        {
            var parms = CompiledTransform.ParseElementParams("#callelement", name, CompiledTransform.SingleFalse);

            _parms         = parms.Skip(1).ToList();
            _elementName   = parms[0];
            _lineNumber    = lineNumber;
        }

        #region IValue

        public object Evaluate(ExpressionContext context)
        {
            var element    = GetElement(context);
            var numParms   = element.Parameters.Count;
            var newContext = new ExpressionContext(context.Data, context);

            for(var i = 0; i < numParms; ++i)
                newContext.SetVariable(element.Parameters[i], _parms[i].Evaluate(context));

            if(IsReturnValue(context))
            {
                var output = new JsonStringWriter();

                output.StartObject();
                element.Evaluate(output, newContext, (f)=> f());
                output.EndObject();

                var rtnVal = output.ToString().JTranToJsonObject()!;

                return rtnVal[_return];
            }

            return null;
        }

        public bool? IsSimpleValue(ExpressionContext context)
        {
            return IsReturnValue(context);
        }

        #endregion

        #region IEvaluator

        public void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var element    = GetElement(context);
            var numParms   = element.Parameters.Count;
            var newContext = new ExpressionContext(context.Data, context);

            for(var i = 0; i < numParms; ++i)
                newContext.SetVariable(element.Parameters[i], _parms[i].Evaluate(context));

            element.Evaluate(output, newContext, (f)=> f());
        }

        #endregion

        #region Private

        private static ICharacterSpan _return = CharacterSpan.FromString("return");

        private TElement GetElement(ExpressionContext context)
        {
            if(_element == null)
            { 
                var elementName = _elementName.Evaluate(context);
                
                _element = context.GetElement(elementName.ToString()!);
            }

            return _element;
        }

        private bool IsReturnValue(ExpressionContext context)
        {
            if(_isReturnValue == null)
            { 
                var element = GetElement(context);
                var children = element.Children.Where(c => c.IsOutput);

                if(children.Count() == 1 && children.First() is TProperty prop)
                {
                    ICharacterSpan propName = prop.Name.Evaluate(context).AsCharacterSpan();

                    _isReturnValue = propName.Equals(_return);
                }
                else
                    _isReturnValue = false;
            }

            return _isReturnValue.Value;
        }
        
        #endregion
    }
}
