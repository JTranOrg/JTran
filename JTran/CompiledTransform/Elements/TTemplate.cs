using System;
using System.Linq;
using System.Collections.Generic;

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

}
