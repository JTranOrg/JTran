using System;
using System.Linq;
using System.Collections.Generic;

using JTran.Expressions;
using JTran.Extensions;
using JTran.Json;
using JTran.Common;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TElement : TTemplate
    {
        /****************************************************************************/
        internal TElement(ICharacterSpan name, ICharacterSpan? val = null, long lineNumber = -1L) 
        {
            var nameStr = name.ToString();

            name = name.Substring("#element(".Length, name.Length - "#element(".Length - 1);

            var parms = name.Split(',');

            this.Name = parms![0].ToString().ToLower();

            this.Parameters.AddRange(parms.Skip(1));
        }  
    }
}
