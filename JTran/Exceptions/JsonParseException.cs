using System;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    public abstract class JTranException : Exception
    {
        private long _lineNumber = -1L;

        /****************************************************************************/
        public JTranException(string msg, long lineNo) : base(msg)
        {
            this.LineNumber = lineNo;
        }

        /****************************************************************************/
        public JTranException(string msg, Exception inner) : base(msg, inner)
        {
        }

        /****************************************************************************/
        public long LineNumber 
        { 
            get => _lineNumber; 

            set
            {
                _lineNumber = value;
                this.Data["LineNumber"] = _lineNumber.ToString();
            }
        }

        /****************************************************************************/
        public override string Message => base.Message + $" at line {_lineNumber}";
    }

    /****************************************************************************/
    /****************************************************************************/
    public class JsonParseException : JTranException
    {
        /****************************************************************************/
        public JsonParseException(string msg, long lineNo) : base(msg, lineNo)
        {
        }

        /****************************************************************************/
        public JsonParseException(string msg, Exception inner) : base(msg, inner)
        {
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class AssertFailedException : JTranException
    {
        /****************************************************************************/
        public AssertFailedException(string msg, long lineNo) : base(msg, lineNo)
        {
        }
    }
}
