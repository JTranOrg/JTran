using System;
using System.Collections.Generic;

using JTran.Common;
using JTran.Expressions;
using JTran.Extensions;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TContainer : TToken
    {
        internal List<TToken>                   Children    { get; }
        internal IDictionary<string, TTemplate> Templates   { get; }
        internal IDictionary<string, TFunction> Functions   { get; }
        internal CompiledTransform?             CompiledTransform { get; set; }

        /****************************************************************************/
        internal TContainer()
        {
            this.Children  = new List<TToken>();
            this.Templates = new Dictionary<string, TTemplate>();
            this.Functions = new Dictionary<string, TFunction>();
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            var numChildren = this.Children.Count;

            wrap( ()=>
            {
                foreach(var child in this.Children)
                { 
                    child.Evaluate(output, context, (cb)=>
                    {                  
                        cb();
                    });
                }
            });
        }

        /****************************************************************************/
        internal protected virtual TTemplate AddTemplate(TTemplate template)
        {
            this.Templates.Add(template.Name, template);

            return template;
        }

        /****************************************************************************/
        internal protected virtual TFunction AddFunction(TFunction function)
        {
            this.Functions.Add(function.Name, function);

            return function;
        }

        /****************************************************************************/
        internal virtual TToken CreateObject(CharacterSpan name, object? previous, long lineNumber)
        {
            TToken? result = null;
            
            if(name == null)
                name = CharacterSpan.Empty;
            
            var elementName = name.SubstringBefore('(');

            if(elementName.Equals(_template))
                return AddTemplate(new TTemplate(name));

            if(elementName.Equals(_function))
                return AddFunction(new TFunction(name));

            if(elementName.Equals(_variable))
                result = new TVariableObject(name);

            else if(elementName.Equals(_map))
                result = new TMap(name);

            else if(elementName.Equals(_calltemplate))
                result = result = new TCallTemplate(name);

            else if(elementName.Equals(_bind))
                result = new TBind(name);

            else if(elementName.Equals(_foreachgroup))
                result = new TForEachGroup(name);

            else if(elementName.Equals(_foreach))
                result = new TForEach(name, lineNumber);

            else if(elementName.Equals(_iterate))
                result = new TIterate(name);

            else if(elementName.Equals(_arrayitem))
                result = new TArrayItem(name, lineNumber);

            else if(elementName.Equals(_array))
                result = new TArray(name);

            else if(elementName.Equals(_try))
                result = new TTry();

            else if(elementName.Equals(_catch))
            { 
                if(previous is TTry || previous is TCatch)
                    result = new TCatch(name);
                else
                    throw new Transformer.SyntaxException("#catch must follow a #try or another #catch");
            }

            else if(elementName.Equals(_if))
                result = new TIf(name);

            else if(elementName.Equals(_elseif))
            { 
                if(previous is TIf || previous is TElseIf)
                    result = new TElseIf(name);
                else
                    throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");
            }

            else if(elementName.Equals(_else))
            { 
                if(previous is TIf || previous is TElseIf)
                    result = new TElse();
                else 
                    throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");
            }

            else if(name.Length > 0 && name[0] == '#' && name[1] != '(')
                throw new Transformer.SyntaxException($"Unknown element name: {elementName}");

            else
                result = new TObject(name, lineNumber);

            if(result != null)
            { 
                this.Children.Add(result);
                result.Parent = this;
            }

            return result;
        }

        /****************************************************************************/
        internal virtual TToken CreateArray(CharacterSpan? name, long lineNumber)
        {
            TToken? result;

            if(name?.StartsWith("#variable") ?? false)
                result = new TVariableArray(name);
            else 
                result = new TExplicitArray(name, lineNumber);

            this.Children.Add(result);

            return result;
        }

        /****************************************************************************/
        internal virtual TToken CreateProperty(CharacterSpan name, object? val, object? previous, long lineNumber)
        {
            var result = CreatePropertyToken(name, val, previous, lineNumber);

            if(result != null)
            { 
                this.Children.Add(result);
                result.Parent = this;
            }

            return result;
        }

        private static readonly CharacterSpan _bind         = CharacterSpan.FromString("#bind");
        private static readonly CharacterSpan _break        = CharacterSpan.FromString("#break");
        private static readonly CharacterSpan _function     = CharacterSpan.FromString("#function");
        private static readonly CharacterSpan _foreach      = CharacterSpan.FromString("#foreach");
        private static readonly CharacterSpan _foreachgroup = CharacterSpan.FromString("#foreachgroup");
        private static readonly CharacterSpan _include      = CharacterSpan.FromString("#include");
        private static readonly CharacterSpan _iterate      = CharacterSpan.FromString("#iterate");
        private static readonly CharacterSpan _message      = CharacterSpan.FromString("#message");
        private static readonly CharacterSpan _throw        = CharacterSpan.FromString("#throw");
        private static readonly CharacterSpan _map          = CharacterSpan.FromString("#map");
        private static readonly CharacterSpan _mapitem      = CharacterSpan.FromString("#mapitem");
        private static readonly CharacterSpan _array        = CharacterSpan.FromString("#array");
        private static readonly CharacterSpan _arrayitem    = CharacterSpan.FromString("#arrayitem");
        private static readonly CharacterSpan _if           = CharacterSpan.FromString("#if");
        private static readonly CharacterSpan _elseif       = CharacterSpan.FromString("#elseif");
        private static readonly CharacterSpan _else         = CharacterSpan.FromString("#else");
        private static readonly CharacterSpan _try          = CharacterSpan.FromString("#try");
        private static readonly CharacterSpan _catch        = CharacterSpan.FromString("#catch");
        private static readonly CharacterSpan _variable     = CharacterSpan.FromString("#variable");
        private static readonly CharacterSpan _template     = CharacterSpan.FromString("#template");
        private static readonly CharacterSpan _calltemplate = CharacterSpan.FromString("#calltemplate");
                                                            
        private static readonly CharacterSpan _copyof       = CharacterSpan.FromString("#copyof");
        private static readonly CharacterSpan _exclude      = CharacterSpan.FromString("#exclude");
        private static readonly CharacterSpan _iif          = CharacterSpan.FromString("#iif");

        /****************************************************************************/
        protected virtual TToken CreatePropertyToken(CharacterSpan name, object child, object? previous, long lineNumber)
        {   
            if(!name.IsNullOrWhiteSpace()) 
            { 
                var searchStr = name.SubstringBefore('(');
                
                if(_include.Equals(searchStr))
                { 
                    this.CompiledTransform.LoadInclude(child.ToString(), this, lineNumber);

                    return null;
                }

                if(_break.Equals(searchStr))
                    return new TBreak();

                if(_variable.Equals(searchStr))
                    return new TVariable(name, child, lineNumber);

                if(_message.Equals(searchStr))
                    return new TMessage(child as CharacterSpan, lineNumber);

                if(_throw.Equals(searchStr))
                    return new TThrow(name, child, lineNumber);

                if(_mapitem.Equals(searchStr))
                {                
                    if(this is TMap && (previous is TMapItem || previous == null))
                        return new TMapItem(name, child, lineNumber);

                    throw new Transformer.SyntaxException("#mapitem must be a child of #map");
                }

                if(_arrayitem.Equals(searchStr))
                    return new TSimpleArrayItem(child as CharacterSpan, lineNumber);

                if(_if.Equals(searchStr))
                    return new TPropertyIf(name, child, lineNumber);

                if(_elseif.Equals(searchStr))
                { 
                    if(previous is TPropertyIf || previous is TElseIf)
                        return new TPropertyElseIf(name, child, lineNumber);

                    throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");
                }

                if(_else.Equals(searchStr))
                { 
                    if(previous is TPropertyIf || previous is TPropertyElseIf)
                        return new TPropertyElse(name, child, lineNumber);

                    throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");
                }   

                if(child is CharacterSpan sval)
                { 
                    searchStr = sval.SubstringBefore('(');

                    if(_copyof.Equals(searchStr))
                        return new TCopyOf(name, sval);
 
                    if(_include.Equals(searchStr))
                        return new TIncludeExclude(name, sval, true, lineNumber);

                     if(_exclude.Equals(searchStr))
                        return new TIncludeExclude(name, sval, false, lineNumber);

                     if(_iif.Equals(searchStr))
                        return new TIif(name, sval, lineNumber);
                }
            }

            return new TProperty(name, child, lineNumber);
       }
    }
}
