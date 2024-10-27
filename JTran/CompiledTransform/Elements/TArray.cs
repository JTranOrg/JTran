using System;
using System.Threading.Tasks;

using JTran.Common;
using JTran.Expressions;
using JTran.Parser;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TBaseArray : TContainer
    {
        /****************************************************************************/
        protected TBaseArray()
        {
        }

        internal bool              IsOutputArray { get; set; } = false;
        internal protected IValue? Name          { get; set; }
      
        /****************************************************************************/
        internal ICharacterSpan? EvaluateName(ExpressionContext context)
        {
            if(this.IsOutputArray)
                return EmptyArray;
                               
            object? arrayName = null;
            
            if(this.Name != null)
                arrayName = this.Name.Evaluate(context);

            var cspan = arrayName?.AsCharacterSpan();

            if(!(cspan?.IsNullOrWhiteSpace() ?? true))
                return cspan;

            return null;
        }
      
        /****************************************************************************/
        internal protected ICharacterSpan? WriteContainerName(IJsonWriter output, ExpressionContext context)
        {
            if(this.IsOutputArray)
                return EmptyArray;

            var arrayName = this.EvaluateName(context);

            if(arrayName != null && !arrayName.Equals(EmptyObject))
            { 
                output.WriteContainerName(arrayName);
            }

            return arrayName;
        }
      
        /****************************************************************************/
        internal protected void SetName(IExpression expr)
        {
            var arrayName = expr as Value;

            if(arrayName != null)
            { 
                var name = arrayName?.Evaluate(null);

                if(name is Token token)
                {
                    if(token.Type == Token.TokenType.ExplicitArray)
                    { 
                        this.IsOutputArray = true;
                        this.Name = new SimpleValue(EmptyArray);
                    }
                    else if(token.Type == Token.TokenType.Text && token.Value.ToString() == "{}")
                         this.Name = new SimpleValue(EmptyObject);
                    else 
                         this.Name = new SimpleValue(token.Value);
                }
                else
                { 
                    this.IsOutputArray = false;
                    this.Name = new SimpleValue(name);
                }
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TArray : TBaseArray
    {
        private static ICharacterSpan _brackets = CharacterSpan.FromString("[]");

        /****************************************************************************/
        internal TArray(ICharacterSpan name)
        { 
            name = name.Substring("#array(".Length, name.Length - "#array(".Length - 1);
            
            this.Name          = CreateValue(name, true, 0);            
            this.IsOutputArray = name.Equals(_brackets);
        }

        /****************************************************************************/
        public override void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap)
        {
            base.Evaluate(output, context, (fnc)=>
            {
                wrap( ()=>
                { 
                    var arrayName   = WriteContainerName(output, context);
                    var outputArray = this.IsOutputArray || (arrayName != null && !arrayName.Equals(EmptyObject));

                    if(outputArray)
                        output.StartArray();                

                    fnc();

                    if(outputArray)
                        output.EndArray();
                });
            });
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class TArrayItem : TObject
    {
        /****************************************************************************/
        internal TArrayItem(ICharacterSpan name, long lineNumber) : base(name, lineNumber)
        {
        }
    }
}
