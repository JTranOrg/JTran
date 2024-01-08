using System;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    public class JsonParseException : Exception
    {
    private long _lineNumber = 0L;

        public JsonParseException(string msg, long lineNo) : base(msg)
        {
            this.LineNumber = lineNo;
        }

        public JsonParseException(string msg, Exception inner) : base(msg, inner)
        {
        }

        public long LineNumber 
        { 
            get => _lineNumber; 

            set
            {
                _lineNumber = value;
                this.Data["LineNumber"] = _lineNumber.ToString();
            }
        }
    }
}
