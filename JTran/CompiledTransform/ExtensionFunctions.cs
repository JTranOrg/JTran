using JTran.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JTran
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal class ExtensionFunctions
    {
        private readonly IDictionary<string, Function> _functions = new Dictionary<string, Function>();

        /*****************************************************************************/
        internal ExtensionFunctions(IList<object> containers)
        {
            foreach(var container in containers)
            {
                var list = Function.Extract(container);

                foreach(var func in list)
                {
                    var key = CreateKey(func.Name, func.NumParams);

                    if(!_functions.ContainsKey(key))
                        _functions.Add(key, func);
                }
            }
        }

        /*****************************************************************************/
        internal Function GetFunction(string name, int numParams)
        {
            var key = CreateKey(name, numParams);

            if(_functions.ContainsKey(key))
                return _functions[key];

            return null;
        }

        /*****************************************************************************/
        private string CreateKey(string name, int numParams)
        {
            if(name == "document")
                return name;

            return name + "_" + numParams;
        }
    }
}
