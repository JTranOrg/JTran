﻿using JTran.Common;
using System;
using System.Xml.Linq;


namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TExplicitArray : TBaseArray
    {
        /****************************************************************************/
        internal TExplicitArray(ICharacterSpan? name, long lineNumber)
        {
            this.Name = name != null ? CreateValue(name, true, lineNumber) : null;
        }

        /****************************************************************************/
        internal override TToken CreateObject(ICharacterSpan name, object? previous, long lineNumber)
        {
            var obj = new TObject(null, lineNumber);

            this.Children.Add(obj);

            return obj;
        }

        /****************************************************************************/
        internal override TToken CreateArray(ICharacterSpan? name, long lineNumber)
        {
            var obj = new TExplicitArray(null, lineNumber);

            this.Children.Add(obj);

            return obj;
        }

        /****************************************************************************/
        internal override TToken CreateProperty(ICharacterSpan name, object? val, object? previous, long lineNumber)
        {
            var obj = new TSimpleArrayItem(val as ICharacterSpan, lineNumber);

            this.Children.Add(obj);

            return obj;
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            base.Evaluate(output, context, (fnc)=>
            {
                wrap( ()=>
                { 
                    if(output.InObject)
                    { 
                        var name = WriteContainerName(output, context);

                        if(name?.IsNullOrWhiteSpace() ?? true)
                            throw new Transformer.SyntaxException("Array name evaluates to null or empty string");
                    }

                    output.StartArray();
                    fnc();
                    output.EndArray();
                });
            });
        }
    }
}
