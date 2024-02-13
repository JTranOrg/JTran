using System;
using System.Linq;
using System.Collections.Generic;

using JTran.Expressions;
using JTran.Extensions;
using JTran.Json;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TTemplate : TContainer
    {
        public List<string> Parameters { get; } = new List<string>();

        /****************************************************************************/
        internal TTemplate(string name) 
        {
            name = name.Substring("#template(".Length);

            var parms = name.Substring(0, name.Length - 1).Split(new char[] { ',' });

            this.Name = parms[0].ToLower().Trim();

            this.Parameters.AddRange(parms.Select( s=> s.Trim()));
            this.Parameters.RemoveAt(0);
        }

        internal string Name      { get; }

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
        private readonly string _templateName;

        /****************************************************************************/
        internal TCallTemplate(string name) 
        {
            _templateName = name.Substring("#calltemplate(".Length).ReplaceEnding(")", "");
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var template = context.GetTemplate(_templateName);

            if(template == null)
                throw new Transformer.SyntaxException($"A template with that name was not found: {_templateName}");

            var newContext = new ExpressionContext(context.Data, context);
            var paramsOutput = new JsonStringWriter();

            paramsOutput.StartObject();
            base.Evaluate(paramsOutput, newContext, (fnc)=> fnc()); 
            paramsOutput.EndObject();

            var jsonParams = paramsOutput.ToString().JTranToExpando(); // ???

            foreach(var paramName in template.Parameters)
                newContext.SetVariable(paramName, (jsonParams as IDictionary<string, object>)[paramName].ToString());

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

        /****************************************************************************/
        internal TCallTemplateProperty(string name, long lineNumber) 
        {
            var parms = CompiledTransform.ParseElementParams("calltemplate", name, CompiledTransform.TrueFalse);

            _parms        = parms.Skip(1).ToList();
            _templateName = parms[0].Evaluate(new ExpressionContext(null)).ToString();
            _lineNumber   = lineNumber;
        }

        /****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            var template = context.GetTemplate(_templateName);

            if(template == null)
                throw new Transformer.SyntaxException($"An element or template with that name was not found: {_templateName}") { LineNumber = _lineNumber};

            var numParms = template.Parameters.Count;
            var newContext = new ExpressionContext(context.Data, context);

            for(var i = 0; i < numParms; ++i)
                newContext.SetVariable(template.Parameters[i], _parms[i].Evaluate(context));

            var output = new JsonStringWriter();

            output.StartObject();
            template.Evaluate(output, newContext, (f)=> f());
            output.EndObject();

            return output.ToString().JTranToExpando();
        }
    }
}
