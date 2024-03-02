using System.Collections.Generic;
using JTran.Common;
using JTran.Extensions;

namespace JTran.Parser
{
    /*****************************************************************************/
    /*****************************************************************************/
    /// <summary>
    /// Parse an expression string into a flat list of tokens
    /// </summary>
    internal class ExpressionParser
    {
        private static readonly IDictionary<string, string> _transformOperators = new Dictionary<string, string> { {"and", "&&" }, { "or", "||"} };
        private static readonly IList<string> _operators = new List<string> { "[", "]", "*", "/", "++", "--", "+=", "-=", "*=", "/=", "+", "-", "%", "<", "<=", ">", ">=", "=", "==", "===", "!=", "!==", "!", "||", "&&", "??", "?", ":", "(", ")", "~", "^", ">>", "<<", ",", ".", "=>", "=:", "and", "or", "=" };
        
        private readonly List<Token> _sb = new List<Token>();

        private Token? _token = null;
        private int _index = 0;

        /*****************************************************************************/
        internal ExpressionParser()
        {  
        }

        /*****************************************************************************/
        // ??? Need to optimize tokens to use CharacterSpan
        internal IReadOnlyList<Token> Parse(CharacterSpan expression)
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
                else if(this._token?.IsLiteral ?? false)
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
                    switch(this._token?.Type)
                    { 
                        case Token.TokenType.Number:
                            this._token.Value += ch;
                            break;

                        case Token.TokenType.Text when(ch == '.' && (!string.IsNullOrWhiteSpace(this._token?.Value) || (prev != null && char.IsPunctuation(prev!.Value)))):
                        {
                            this.PushToken();
                            this._token = CreateToken(ch.ToString(), Token.TokenType.Operator);
                            this.PushToken();
                            break;
                        }

                        case Token.TokenType.Text when(!string.IsNullOrWhiteSpace(this._token?.Value)):
                            this._token.Value += ch;
                            break;

                        default:
                        { 
                            this.PushToken();
                            this._token = CreateToken(ch.ToString(), Token.TokenType.Number);
                            break;
                        }
                    }
                }
                // Valid punctuation
                else if(opDict.ContainsKey(ch))
                {  
                    if(this._token?.Type == Token.TokenType.Punctuation)
                        this._token.Value += ch;
                    else
                    {
                        this.PushToken();
                        this._token = CreateToken(ch.ToString(), Token.TokenType.Punctuation);
                    }
                }
                else if(_punctuation.Contains(ch))
                {
                    this.PushToken();
                    this.PushToken(CreateToken(ch.ToString(), Token.TokenType.Literal));
                }
                else if(this._token == null)
                {
                    this._token = CreateToken(ch.ToString());
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

        private static Dictionary<char, bool> opDict = new Dictionary<char, bool> {{'!', true},
                                                                                   {'=', true},
                                                                                   {'<', true},
                                                                                   {'>', true},
                                                                                   {'*', true},
                                                                                   {'/', true},
                                                                                   {'+', true},
                                                                                   {'-', true},
                                                                                   {'?', true},
                                                                                   {'.', true},
                                                                                   {'~', true},
                                                                                   {'^', true}};
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
                    this._sb.Add(CreateToken(token.Value, Token.TokenType.Operator)); 
                }
                else if(token.IsLiteral && token.Value.IsQuoted())
                    this._sb.Add(CreateToken(token.Value.Substring(1, token.Value.Length - 2), Token.TokenType.Literal)); 
                else if(token.Type == Token.TokenType.Text && decimal.TryParse(token.Value, out decimal dval)) // ??? CharacterSpan
                    this._sb.Add(CreateToken(token.Value, Token.TokenType.Number)); 
                else
                    this._sb.Add(CreateToken(token.Value, token.Type)); 
            }

            this._token = CreateToken("");
        }

        /*****************************************************************************/
        private Token CreateToken(string val, Token.TokenType type = Token.TokenType.Text)
        {
            return new Token { Value = val, Type = type, Id = ++_index };
        }

        /*****************************************************************************/
        private bool CheckStringLiteral(char chCheck, char ch, char? prev, Token.TokenType tokenType)
        {
            if(ch == chCheck && prev != '\\')
            {
                if(this._token?.Type == tokenType)
                {
                    // End of literal
                    this.PushToken(CreateToken(this._token.Value + ch, Token.TokenType.Literal));
                }
                else
                {
                    // Push any previous tokens
                    this.PushToken();

                    // Beginning of literal
                    this._token  = CreateToken(ch.ToString(), tokenType);
                }

                return true;
            }

            return false;
        }

        #endregion
     }
}
