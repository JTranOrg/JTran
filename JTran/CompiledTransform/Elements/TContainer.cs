using System;
using System.Collections.Generic;

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
        internal virtual TToken CreateObject(string name, object? previous, long lineNumber)
        {
            TToken? result = null;
            
            if(name == null)
                name = "";
            
            var elementName = name.SubstringBefore("(");

            switch(elementName)
            { 
                case "#template":
                    return AddTemplate(new TTemplate(name));

                case "#function":
                    return AddFunction(new TFunction(name));

                case "#variable":
                    result = new TVariableObject(name);
                    break;

                case "#map":
                    result = new TMap(name);
                    break;

                case "#calltemplate":
                    result = result = new TCallTemplate(name);
                    break;

                case "#bind":
                    result = new TBind(name);
                    break;

                case "#foreachgroup":
                    result = new TForEachGroup(name);
                    break;

                case "#foreach":
                    result = new TForEach(name, lineNumber);
                    break;

                case "#iterate":
                    result = new TIterate(name);
                    break;

                case "#arrayitem":
                    result = new TArrayItem(name, lineNumber);
                    break;

                case "#array":
                    result = new TArray(name);
                    break;

                case "#try":
                    result = new TTry();
                    break;

                case "#catch":
                { 
                    if(previous is TTry || previous is TCatch)
                        result = new TCatch(name);
                    else
                        throw new Transformer.SyntaxException("#catch must follow a #try or another #catch");

                    break;
                }

                case "#if":
                    result = new TIf(name);
                    break;

                case "#elseif":
                { 
                    if(previous is TIf || previous is TElseIf)
                        result = new TElseIf(name);
                    else
                        throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");

                    break;
                }

                case "#else":
                { 
                    if(previous is TIf || previous is TElseIf)
                        result = new TElse(name);
                    else 
                        throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");

                    break;
                }

                default:
                { 
                    if(name.StartsWith("#") && !name.StartsWith("#("))
                        throw new Transformer.SyntaxException($"Unknown element name: {elementName}");

                    result = new TObject(name, lineNumber);
                    break;
                }
            }

            if(result != null)
            { 
                this.Children.Add(result);
                result.Parent = this;
            }

            return result;
        }

        /****************************************************************************/
        internal virtual TToken CreateArray(string? name, long lineNumber)
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
        internal virtual TToken CreateProperty(string name, object? val, object? previous, long lineNumber)
        {
            var result = CreatePropertyToken(name, val, previous, lineNumber);

            if(result != null)
            { 
                this.Children.Add(result);
                result.Parent = this;
            }

            return result;
        }

        /****************************************************************************/
        protected virtual TToken CreatePropertyToken(string name, object child, object? previous, long lineNumber)
        {   
            if(!string.IsNullOrEmpty(name)) 
            { 
                switch(name.SubstringBefore("("))
                { 
                    case "#include":
                    { 
                        this.CompiledTransform.LoadInclude(child.ToString(), this, lineNumber);

                        return null;
                    }

                    case "#break":
                        return new TBreak();

                    case "#variable":
                        return new TVariable(name, child, lineNumber);

                    case "#message":
                        return new TMessage(child, lineNumber);

                    case "#throw":
                        return new TThrow(name, child, lineNumber);

                    case "#mapitem":
                    {                
                        if(this is TMap && (previous is TMapItem || previous == null))
                            return new TMapItem(name, child, lineNumber);

                        throw new Transformer.SyntaxException("#mapitem must be a child of #map");
                    }

                    case "#arrayitem":
                        return new TSimpleArrayItem(child, lineNumber);

                    case "#if":
                        return new TPropertyIf(name, child, lineNumber);

                    case "#elseif":
                    { 
                        if(previous is TPropertyIf || previous is TElseIf)
                            return new TPropertyElseIf(name, child, lineNumber);

                        throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");
                    }

                    case "#else":
                    { 
                        if(previous is TPropertyIf || previous is TPropertyElseIf)
                            return new TPropertyElse(name, child, lineNumber);

                        throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");
                    }   

                    default:
                        break;
                }

                var sval = child.ToString();

                switch(sval.SubstringBefore("("))
                { 
                    case "#copyof":
                        return new TCopyOf(name, sval);
 
                    case "#include":
                        return new TIncludeExclude(name, sval, true, lineNumber);

                    case "#exclude":
                        return new TIncludeExclude(name, sval, false, lineNumber);

                    case "#iif":
                        return new TIif(name, sval, lineNumber);

                    default:
                        break;
                }
            }

            return new TProperty(name, child, lineNumber);
       }
    }
}
