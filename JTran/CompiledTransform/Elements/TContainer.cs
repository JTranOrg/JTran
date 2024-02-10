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
        internal virtual TToken CreateObject(string name, object? previous)
        {
            TToken? result = null;
            
            if(name == null)
                name = "";
            
            if(name.StartsWith("#template"))
                return AddTemplate(new TTemplate(name));

            if(name.StartsWith("#function"))
                return AddFunction(new TFunction(name));

            if(name.StartsWith("#variable"))
                result = new TVariableObject(name);

            else if(name.StartsWith("#map("))
                result = new TMap(name);

            else if(name.StartsWith("#calltemplate"))
                result = result = new TCallTemplate(name);

            else if(name.StartsWith("#bind"))
                result = new TBind(name);

            else if(name.StartsWith("#foreachgroup"))
                result = new TForEachGroup(name);

            else if(name.StartsWith("#foreach"))
                result = new TForEach(name);

            else if(name.StartsWith("#iterate"))
                result = new TIterate(name);

            else if(name.StartsWith("#arrayitem"))
                result = new TArrayItem(name);

            else if(name.StartsWith("#array"))
                result = new TArray(name);

            else if(name.StartsWith("#try"))
               result = new TTry();

            else if(name.StartsWith("#catch"))
            { 
                if(previous is TTry || previous is TCatch)
                    result = new TCatch(name);

               else
                    throw new Transformer.SyntaxException("#catch must follow a #try or another #catch");
            }

            else if(name.StartsWith("#if"))
                result = new TIf(name);

            else if(name.StartsWith("#elseif"))
            { 
                if(previous is TIf || previous is TElseIf)
                    result = new TElseIf(name);
                else
                    throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");
            }

            else if(name.StartsWith("#else"))
            { 
                if(previous is TIf || previous is TElseIf)
                    result = new TElse(name);
                else 
                    throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");
            }

            else if(name.StartsWith("#") && !name.StartsWith("#("))
            { 
                name = name.SubstringBefore("(").Trim();

                throw new Transformer.SyntaxException($"Unknown element name: {name}");
            }

            else
                result = new TObject(name);

            if(result != null)
            { 
                this.Children.Add(result);
                result.Parent = this;
            }

            return result;
        }

        /****************************************************************************/
        internal virtual TToken CreateArray(string? name)
        {
            TToken? result;

            if(name?.StartsWith("#variable") ?? false)
                result = new TVariableArray(name);
            else 
                result = new TExplicitArray(name);

            this.Children.Add(result);

            return result;
        }

        /****************************************************************************/
        internal virtual TToken CreateProperty(string name, object? val, object? previous)
        {
            var result = CreatePropertyToken(name, val, previous);

            if(result != null)
            { 
                this.Children.Add(result);
                result.Parent = this;
            }

            return result;
        }

        /****************************************************************************/
        protected virtual TToken CreatePropertyToken(string name, object child, object? previous)
        {   
            if(!string.IsNullOrEmpty(name)) 
            { 
                if(name.StartsWith("#include"))
                { 
                    this.CompiledTransform.LoadInclude(child.ToString(), this);

                    return null;
                }

                if(name.StartsWith("#break"))
                    return new TBreak();

                if(name.StartsWith("#variable"))
                    return new TVariable(name, child);

                if(name.StartsWith("#message"))
                    return new TMessage(child);

                if(name.StartsWith("#throw"))
                    return new TThrow(name, child);

                var sval = child.ToString();

                if(name.StartsWith("#mapitem"))
                {                
                    if(this is TMap && (previous is TMapItem || previous == null))
                        return new TMapItem(name, child);

                    throw new Transformer.SyntaxException("#mapitem must be a child of #map");
                }

                if(name.StartsWith("#arrayitem"))
                    return new TSimpleArrayItem(child);

                if(name.StartsWith("#if"))
                    return new TPropertyIf(name, child);

                if(name.StartsWith("#elseif"))
                { 
                    if(previous is TPropertyIf || previous is TElseIf)
                        return new TPropertyElseIf(name, child);

                    throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");
                }

                if(name.StartsWith("#else"))
                { 
                    if(previous is TPropertyIf || previous is TPropertyElseIf)
                        return new TPropertyElse(name, child);

                    throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");
                }   

                if(sval.StartsWith("#copyof"))
                    return new TCopyOf(name, sval);
 
                if(sval.StartsWith("#include"))
                    return new TIncludeExclude(name, sval, true);

                if(sval.StartsWith("#exclude"))
                    return new TIncludeExclude(name, sval, false);

                if(sval.StartsWith("#iif"))
                { 
                    return new TIif(name, sval);
                }

                if(name.StartsWith("#") && !name.StartsWith("#("))
                { 
                    name = name.SubstringBefore("(").Trim();

                    throw new Transformer.SyntaxException($"Unknown element name: {name}");
                }

                if(sval.StartsWith("#") && !sval.StartsWith("#("))
                { 
                    sval = sval.SubstringBefore("(").Trim();

                    throw new Transformer.SyntaxException($"Unknown element name: {sval}");
                }
            }

            return new TProperty(name, child);
       }
    }
}
