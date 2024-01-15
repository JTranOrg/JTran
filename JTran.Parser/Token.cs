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
        private static IDictionary<string, bool> _endBoundary = new Dictionary<string, bool>
        {
            { "]", true },
            { ")", true },
            { ",", true }
        };

        private static IDictionary<string, bool> _allBoundaries = new Dictionary<string, bool>
        {
            { "]", true },
            { ")", true },
            { "[", true },
            { "(", true },
            { ",", true }
        };

        private static IDictionary<string, bool> _conditionals = new Dictionary<string, bool>
        {
            { "&&", true },
            { "||", true }
        };
        
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

        public bool IsOperator    => this?.Type == TokenType.Operator;
        public bool IsBoundary    => this.IsOperator && _allBoundaries.ContainsKey(this.Value);
        public bool IsEndBoundary => this.IsOperator && _endBoundary.ContainsKey(this.Value);
        public bool IsComma       => this.IsOperator && this.Value == ",";
        public bool IsBeginParen  => this.IsOperator && this.Value == "(";
        public bool IsEndParen    => this.IsOperator && this.Value == ")";
        public bool IsConditional => this.IsOperator && _conditionals.ContainsKey(this.Value);
        public bool IsTertiary    => this.IsOperator && "?:".Contains(this.Value);

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
            Expression,
            Function,
            Tertiary,
            Array,
            ArrayIndexer
        }    
    }

    /*****************************************************************************/
    /*****************************************************************************/
    public class ExpressionToken : Token
    { 
        /*****************************************************************************/
        public ExpressionToken(TokenType type = TokenType.Expression )
        {
            Type = type;
        }

        public List<Token> Children  { get; set; } = new List<Token>();  
    }
}
