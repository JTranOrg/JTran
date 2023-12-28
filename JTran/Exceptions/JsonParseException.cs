using System;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    public class JsonParseException : Exception
    {
        public JsonParseException(string msg, long lineNo) : base(msg)
        {
            this.LineNumber = lineNo;
            this.Data.Add("LineNumber", lineNo.ToString());
        }

        public long LineNumber { get; }
    }
}
