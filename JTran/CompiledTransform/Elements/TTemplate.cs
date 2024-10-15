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
    internal class TTemplate : TContainer
    {
        public List<ICharacterSpan> Parameters { get; } = new();

        /****************************************************************************/
        internal TTemplate(ICharacterSpan name) 
        {
            name = name.Substring("#template(".Length, name.Length - "#template(".Length - 1);

            var parms = name.Split(',');

            this.Name = parms![0].ToString().ToLower();

            this.Parameters.AddRange(parms.Skip(1));
        }

        /****************************************************************************/
        internal protected TTemplate() 
        {
        }

        internal string Name { get; set; } = "";

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var newContext = new ExpressionContext(context.Data, context);

            base.Evaluate(output, newContext, wrap);
        }    
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TCallTemplate : TContainer
    {
        private readonly string  _templateName;
        private readonly bool    _allowElements;
        private readonly IValue? _valueExpression;
        private readonly long    _lineNumber;
        private readonly IList<IExpression> _parms;

        /****************************************************************************/
        internal TCallTemplate(ICharacterSpan name, long lineNumber = -1L, ICharacterSpan? value = null, bool allowElements = false) 
        {
            _templateName   = name.Substring("#calltemplate(".Length).ToString()!.ReplaceEnding(")", "").SubstringBefore(",");
            _allowElements = allowElements;
            _lineNumber    = lineNumber;

            if(value != null)
                _valueExpression = CreateValue(value, true, lineNumber);

            var parms = CompiledTransform.ParseElementParams("#calltemplate", name, CompiledTransform.TrueFalse);

            _parms = parms.Skip(1).ToList();
        }

        private static ICharacterSpan kValue = CharacterSpan.FromString("__value");

        /****************************************************************************/
        public static ICharacterSpan SubstituteCustomName(ICharacterSpan val) 
        {
            var templateName = val.SubstringBefore('(', 1);
            var theRest      = val.SubstringAfter('(');

            return CharacterSpan.FromString("#calltemplate(" + templateName!.ToString()!.ToLower() + "," + theRest.ToString(), true); 
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var template     = context.GetTemplateOrElement(_templateName, _allowElements, _lineNumber);
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

            var numParms = template.Parameters.Count;

            if(numParms > 0) 
            {
                for(var i = 0; i < numParms; ++i)
                { 
                    if(_parms.Count <= i)
                        break;

                    newContext.SetVariable(template.Parameters[i], _parms[i].Evaluate(context));
                }
            }

            if(_valueExpression == null)
            { 
                var jsonParams = paramsOutput.ToString().JTranToJsonObject(); 

                if(!_allowElements)
                { 
                    foreach(var paramName in template.Parameters)
                    { 
                        var val = jsonParams[paramName];

                        newContext.SetVariable(paramName, val);
                    }
                }
                else
                {
                    jsonParams.Parent = newContext.Data;
                    newContext.Data = jsonParams;
                }
            }

            template.Evaluate(output, newContext, wrap);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TCallTemplateProperty : IValue
    {
        private readonly string _templateName;
        private readonly IList<IExpression> _parms;
        private readonly long _lineNumber;
        private readonly bool _allowElements;

        /****************************************************************************/
        internal TCallTemplateProperty(ICharacterSpan name, long lineNumber, bool allowElements = false) 
        {
            var parms = CompiledTransform.ParseElementParams("#calltemplate", name, CompiledTransform.TrueFalse);

            _parms         = parms.Skip(1).ToList();
            _templateName  = parms[0].Evaluate(new ExpressionContext(null)).ToString()!;
            _lineNumber    = lineNumber;
            _allowElements = allowElements;
        }

        private static ICharacterSpan _return = CharacterSpan.FromString("return");

        /****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            var template   = context.GetTemplateOrElement(_templateName, _allowElements, _lineNumber);
            var numParms   = template.Parameters.Count;
            var newContext = new ExpressionContext(context.Data, context);

            for(var i = 0; i < numParms; ++i)
                newContext.SetVariable(template.Parameters[i], _parms[i].Evaluate(context));

            var output = new JsonStringWriter();

            output.StartObject();
            template.Evaluate(output, newContext, (f)=> f());
            output.EndObject();

            var rtnVal = output.ToString().JTranToJsonObject()!;

            // Is simple property?
            if(rtnVal.Count == 1 && rtnVal.ContainsKey(_return))
                return rtnVal[_return];

            return rtnVal;
        }
    }
}
