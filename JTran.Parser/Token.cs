using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using JTran.Extensions;

namespace JTran.Parser
{
   /*****************************************************************************/
    /*****************************************************************************/
    public class Token
    { 
        /*****************************************************************************/
        public Token()
        {
        }

        /*****************************************************************************/
        public Token(string val, TokenType tokenType = TokenType.Text)
        {
           this.Value = val;
           this.Type  = tokenType;
        }

        public string    Value { get; set; } = "";
        public TokenType Type  { get; set; } = TokenType.Text;

        /*****************************************************************************/
        public override string ToString()
        {
            return this.Value;
        }

        /*****************************************************************************/
        public enum TokenType
        {
            NoToken,
            Text,
            Literal,
            DLiteral,
            Number,
            Punctuation,
            Operator,
            Expression
        }    
    }

    /*****************************************************************************/
    /*****************************************************************************/
    public class ExpressionToken : Token
    { 
        /*****************************************************************************/
        public ExpressionToken()
        {
            Type = TokenType.Expression;
        }

        public List<Token> Children  { get; set; } = new List<Token>();  
    }
}
