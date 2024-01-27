using System;
using System.Collections.Generic;
using System.Linq;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TFunction : TContainer
    {
        public List<string> Parameters { get; } = new List<string>();

        /****************************************************************************/
        internal TFunction(string name) 
        {
            var parms   = CompiledTransform.ParseElementParams("function", name, CompiledTransform.SingleTrue );
            var context = new ExpressionContext(new {});

            this.Name = parms[0].Evaluate(context).ToString().ToLower().Trim();

            this.Parameters.AddRange(parms.Select( s=> s.Evaluate(context).ToString().Trim()));
            this.Parameters.RemoveAt(0);
        }

        internal string Name { get; }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var newContext = new ExpressionContext(context.Data, context);

            base.Evaluate(output, newContext, (fnc)=> fnc());
        }    
    }}
