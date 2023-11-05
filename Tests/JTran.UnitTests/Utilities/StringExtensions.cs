using System;
using System.Collections.Generic;
using System.Text;

namespace JTran.UnitTests
{
    public static class StringExtensions
    {
        public static string SubstringBefore(this string val, string before)
        {
            var index = val.IndexOf(before);

            if(index == -1)
                return val;

            return val.Substring(0, index);
        }       
    }
}
