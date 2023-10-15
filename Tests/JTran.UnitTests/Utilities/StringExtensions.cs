using System;
using System.Collections.Generic;
using System.Text;

namespace JTran.UnitTests
{
    internal static class StringExtensions
    {
        internal static string SubstringBefore(this string val, string before)
        {
            var index = val.IndexOf(before);

            if(index == -1)
                return val;

            return val.Substring(0, index);
        }       
    }
}
