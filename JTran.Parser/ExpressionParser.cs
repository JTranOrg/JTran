using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using JTran.Extensions;

namespace JTran.Parser
{
    /*****************************************************************************/
    /*****************************************************************************/
    /// <summary>
    /// Parse an expression string into a flat list of tokens
    /// </summary>
    public class ExpressionParser
    {
        private static readonly IDictionary<string, string> _transformOperators = new Dictionary<string, string> { {"and", "&&" }, { "or", "||"} };
        private static readonly IList<string> _operators = new List<string> { "[", "]", "*", "/", "++", "--", "+=", "-=", "*=", "/=", "+", "-", "%", "<", "<=", ">", ">=", "=", "==", "===", "!=", "!==", "!", "||", "&&", "??", "?", ":", "(", ")", "~", "^", ">>", "<<", ",", ".", "=>", "=:", "and", "or", "=" };
        
        private readonly List<Token> _sb = new List<Token>();

        private Token? _token = null;

        /*****************************************************************************/
        public ExpressionParser()
        {  
        }

        /*****************************************************************************/
        public IReadOnlyList<Token> Parse(string expression)
        {
            var   n    = expression.Length;
            char? prev = null;

            for(var i = 0; i < n; ++i)
            {
                var ch = expression[i];
          
                #pragma warning disable CS0642  

                // String literals
                if(this.CheckStringLiteral('\'', ch, prev, Token.TokenType.Literal))
                    ;
                else if(this.CheckStringLiteral('\"', ch, prev, Token.TokenType.DLiteral))
                    ;
                else if(this._token?.Type == Token.TokenType.Literal || this._token?.Type == Token.TokenType.DLiteral)
                {
                    this._token.Value += ch;
                }
                else if(ch == ' ')
                {
                    this.PushToken();
                }
                // Numeric literals
                else if(ch.IsNumberChar())
                {
                    if(this._token?.Type == Token.TokenType.Number)
                    {
                        this._token.Value += ch;
                    }
                    else if(this._token?.Type == Token.TokenType.Text && ch == '.')
                    {
                        this.PushToken();
                        this._token = new Token { Value = ch.ToString(), Type  = Token.TokenType.Operator };
                        this.PushToken();
                    }
                    else
                    {
                        this.PushToken();
                        this._token = new Token { Value = ch.ToString(), Type  = Token.TokenType.Number };
                    }
                }
                // Valid punctuation
                else if(opRegex.Matches(ch.ToString()).Count > 0)
                {  
                    if(this._token?.Type == Token.TokenType.Punctuation)
                        this._token.Value += ch;
                    else
                    {
                        this.PushToken();
                        this._token = new Token { Value = ch.ToString(), Type  = Token.TokenType.Punctuation };
                    }
                }
                else if(_punctuation.Contains(ch))
                {
                    this.PushToken();
                    this.PushToken(new Token(ch.ToString(), Token.TokenType.Literal));
                }
                else if(this._token == null)
                {
                    this._token = new Token(ch.ToString());
                }
                else 
                {
                    this._token.Value += ch.ToString();
                    this._token.Type = Token.TokenType.Text;
                }
    
               #pragma warning restore CS0642  

                prev = ch;
            }

            this.PushToken();

            return _sb.AsReadOnly();
        }

        #region Private 

        private readonly Regex opRegex   = new Regex(@"/[!=<>\|&/\*\^\+\~\?\:]/");
        private const string _punctuation = @"[](),";

        /*****************************************************************************/
        private void PushToken(Token? token = null, bool? literal = null)
        {
            if(literal.HasValue)
                literal = this._token!.Type > Token.TokenType.NoToken;

            if(token == null)
                token = this._token;

            if(!string.IsNullOrEmpty(token?.Value))
            {
                if(_operators.Contains(token.Value))
                {
                    this._sb.Add(new Token(token.Value, Token.TokenType.Operator)); 
                }
                else if((token.Type == Token.TokenType.Literal || token.Type == Token.TokenType.DLiteral) && token.Value.IsQuoted())
                    this._sb.Add(new Token(token.Value.Substring(1, token.Value.Length - 2), Token.TokenType.Literal)); 
                else if(token.Type == Token.TokenType.Text && double.TryParse(token.Value, out double dval))
                    this._sb.Add(new Token(token.Value, Token.TokenType.Number)); 
                else
                    this._sb.Add(new Token(token.Value, token.Type)); 
            }

            this._token = new Token("");
        }

        /*****************************************************************************/
        private bool CheckStringLiteral(char chCheck, char ch, char? prev, Token.TokenType tokenType)
        {
            if(ch == chCheck && prev != '\\')
            {
                if(this._token?.Type == tokenType)
                {
                    // End of literal
                    this.PushToken(new Token(this._token.Value + ch, Token.TokenType.Literal));
                }
                else
                {
                    // Push any previous tokens
                    this.PushToken();

                    // Beginning of literal
                    this._token  = new Token { Value = ch.ToString(), Type = tokenType };
                }

                return true;
            }

            return false;
        }

        #endregion
     }

}
