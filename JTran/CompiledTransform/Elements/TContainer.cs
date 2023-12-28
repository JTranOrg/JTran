using JTran.Extensions;
using JTran.Json;
using JTran.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TContainer : TToken
    {
        internal List<TToken>                   Children    { get; }
        internal IDictionary<string, TTemplate> Templates   { get; }
        internal IDictionary<string, TFunction> Functions   { get; }
        internal CompiledTransform              CompiledTransform { get; set; }

        private TToken _previous;

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
        internal protected void Compile(CompiledTransform transform, ExpandoObject source, TContainer parent = null)
        {   
            foreach(var child in source)
            {
                var newToken = CreateToken(transform, child.Key, child.Value, parent);

                if(newToken != null)
                    this.Children.Add(newToken);

                _previous = newToken;
            }
        }

        /****************************************************************************/
        internal virtual TToken CreateObject(string name)
        {
            TToken? result = null;
            
            if(name.StartsWith("#function"))
            { 
                var function = new TFunction(name);

                this.Functions.Add(function.Name, function);
            }

            else if(name.StartsWith("#variable"))
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

            else if(name.StartsWith("#arrayitem"))
                result = new TArrayItem(name);

            else if(name.StartsWith("#array"))
                result = new TArray(name);

            else if(name.StartsWith("#try"))
               result = new TTry();

            else if(name.StartsWith("#catch"))
            { 
                if(_previous is TTry || _previous is TCatch)
                    result = new TCatch(name);

               else
                    throw new Transformer.SyntaxException("#catch must follow a #try or another #catch");
            }

            else if(name.StartsWith("#if"))
                result = new TIf(name);

            else if(name.StartsWith("#elseif"))
            { 
                if(_previous is TIf || _previous is TElseIf)
                    result = new TElseIf(name);
                else
                    throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");
            }

            else if(name.StartsWith("#else"))
            { 
                if(_previous is TIf || _previous is TElseIf)
                    result = new TElse(name);
                else 
                    throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");
            }

            else if(name.StartsWith("#") && !name.StartsWith("#("))
            { 
                name = name.SubstringBefore("(").Trim();

                throw new Transformer.SyntaxException($"Unknown element name: {name} at line number ???");
            }

            else
                result = new TObject(name);

            if(result != null)
                this.Children.Add(result);

            return result;
        }

        /****************************************************************************/
        internal virtual TToken CreateArray(string name, object? val)
        {
            TToken? result = new TExplicitArray(name);

            if(result != null)
                this.Children.Add(result);

            return result;
        }

        /****************************************************************************/
        internal virtual TToken CreateProperty(string name, object? val)
        {
            TToken? result = null;

            if(result != null)
                this.Children.Add(result);

            return result;
        }

        /****************************************************************************/
        internal TToken CreateToken(CompiledTransform transform, string name, object child, TContainer parent)
        {   
            var t = child.GetType().ToString();

            // Is the child an array "[]"
           // if(child is IEnumerable<object> array)
            //    return new TExplicitArray(name, array);

            // Is the child an object "{}"
            if(child is ExpandoObject obj)
            { 
                if(name.StartsWith("#template"))
                { 
                    var template = new TTemplate(name);

                    this.Templates.Add(template.Name, template);
                    return null;
                }

                if(name.StartsWith("#function"))
                { 
                    var function = new TFunction(name);

                    this.Functions.Add(function.Name, function);
                    return null;
                }

                if(name.StartsWith("#variable"))
                    return new TVariableObject(name);

                if(name.StartsWith("#map("))
                    return new TMap(name);

                 if(name.StartsWith("#calltemplate"))
                    return new TCallTemplate(name);

                if(name.StartsWith("#bind"))
                    return new TBind(name);

                if(name.StartsWith("#foreachgroup"))
                    return new TForEachGroup(name);

                if(name.StartsWith("#foreach"))
                    return new TForEach(name);

                if(name.StartsWith("#arrayitem"))
                    return new TArrayItem(name);

                if(name.StartsWith("#array"))
                    return new TArray(name);

                if(name.StartsWith("#try"))
                    return new TTry();

                if(name.StartsWith("#catch"))
                { 
                    if(_previous is TTry || _previous is TCatch)
                        return new TCatch(name);

                    throw new Transformer.SyntaxException("#catch must follow a #try or another #catch");
                }

                if(name.StartsWith("#if"))
                    return new TIf(name);

                if(name.StartsWith("#elseif"))
                { 
                    if(_previous is TIf || _previous is TElseIf)
                        return new TElseIf(name);

                    throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");
                }

                if(name.StartsWith("#else"))
                { 
                    if(_previous is TIf || _previous is TElseIf)
                        return new TElse(name);

                    throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");
                }

                if(name.StartsWith("#") && !name.StartsWith("#("))
                { 
                    name = name.SubstringBefore("(").Trim();

                    throw new Transformer.SyntaxException($"Unknown element name: {name} at line number ???");
                }

                return new TObject(name);
            }

            // Must be a simple property
            if(name.StartsWith("#include"))
            { 
                transform.LoadInclude(child.ToString());
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
                if(parent is TMap && (_previous is TMapItem || _previous == null))
                    return new TMapItem(name, child);

                throw new Transformer.SyntaxException("#mapitem must be a child of #map");
            }

            if(name.StartsWith("#arrayitem"))
                return new TSimpleArrayItem(child);

            if(sval.StartsWith("#copyof"))
            { 
                if(name.StartsWith("#noobject"))
                    return new TCopyOf(null, sval);

                return new TCopyOf(name, sval);
            }

            if(sval.StartsWith("#include"))
            { 
                if(name.StartsWith("#noobject"))
                    return new TInclude(null, sval);

                return new TInclude(name, sval);
            }

            if(sval.StartsWith("#exclude"))
            { 
                if(name.StartsWith("#noobject"))
                    return new TExclude(null, sval);

                return new TExclude(name, sval);
            }

            if(name.StartsWith("#if"))
                return new TPropertyIf(name, child);

            if(name.StartsWith("#elseif"))
            { 
                if(_previous is TPropertyIf || _previous is TElseIf)
                    return new TPropertyElseIf(name, child);

                throw new Transformer.SyntaxException("#elseif must follow an #if or another #elseif");
            }

            if(name.StartsWith("#else"))
            { 
                if(_previous is TPropertyIf || _previous is TPropertyElseIf)
                    return new TPropertyElse(name, child);

                throw new Transformer.SyntaxException("#elseif must follow an #if or an #elseif");
            }                
                
            return new TProperty(name, child);
        }
    }
}
