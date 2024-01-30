using System;


namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TExplicitArray : TBaseArray
    {
        /****************************************************************************/
        internal TExplicitArray(string? name)
        {
            this.Name = name != null ? CreateValue(name) : null;
        }

        /****************************************************************************/
        internal override TToken CreateObject(string name, object? previous)
        {
            var obj = new TObject(null);

            this.Children.Add(obj);

            return obj;
        }

        /****************************************************************************/
        internal override TToken CreateArray(string? name)
        {
            var obj = new TExplicitArray(null);

            this.Children.Add(obj);

            return obj;
        }

        /****************************************************************************/
        internal override TToken CreateProperty(string name, object? val, object? previous)
        {
            var obj = new TSimpleArrayItem(val);

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

                        if(string.IsNullOrWhiteSpace(name))
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
