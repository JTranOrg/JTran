using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JTran.Parser
{
   /*****************************************************************************/
    /*****************************************************************************/
    public class Token : IList<Token>
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

        public int       Id    { get; set; } = 0;
        public string    Value { get; set; } = "";
        public TokenType Type  { get; set; } = TokenType.Text;

        protected List<Token>? _children;

        /*****************************************************************************/
        public override string ToString()
        {
            switch(Type)
            {
                case TokenType.Literal:        return "'" + this.Value + "'";
                case TokenType.Function:       return this.Value + "(" + string.Join(", ", _children.Select(c=> c.ToString())) + ")";
                case TokenType.Expression:     return this[0].ToString() + " " + this[1].ToString() + " " + this[2].ToString();
                case TokenType.Tertiary:       return this[0].ToString() + " ? " + this[1].ToString() + " : " + this[2].ToString();
                case TokenType.ArrayIndexer: 
                case TokenType.ExplicitArray:  return "[" + string.Join(", ", _children.Select(c=> c.ToString())) + "]";
                case TokenType.Multipart:      return string.Join(".", _children.Select(c=> c.ToString()));
                case TokenType.Array:          return this.Value + string.Join("", _children.Select(c=> c.ToString()));

                default:                       return this.Value;
            }
        }

        /*****************************************************************************/
        public const string BeginParen = "(";
        public const string BeginArray = "[";
        public const string EndParen = ")";
        public const string EndArray = "]";

        /*****************************************************************************/
        public bool IsOperator       => this?.Type == TokenType.Operator;
        public bool IsPunctuation    => this?.Type == TokenType.Punctuation;
        public bool IsComma          => this.IsOperator && this.Value == ",";
        public bool IsBeginParen     => this.IsOperator && this.Value == BeginParen;
        public bool IsEndParen       => this.IsOperator && this.Value == EndParen;
        public bool IsBeginArray     => this.IsOperator && this.Value == BeginArray;
        public bool IsBeginBoundary  => this.IsOperator && (this.Value == BeginArray || this.Value == BeginParen);
        public bool IsEndBoundary    => this.IsOperator && (this.Value == EndArray || this.Value == EndParen || this.Value == ",");
        public bool IsMultiDot       => this.IsOperator && this.Value == ".";
        public bool IsTertiary       => this.IsOperator && "?:".Contains(this.Value);
        public bool IsExpression     => this.Type >= TokenType.Expression;
        public bool IsLiteral        => this == null ? false : this?.Type == TokenType.Literal || this?.Type == TokenType.DLiteral;

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
            ArrayIndexer,
            CommaDelimited,
            ExplicitArray,
            Multipart
        }    
        
        #region IList

        public IEnumerator<Token> GetEnumerator()
        {
            if(_children == null)
                return Enumerable.Empty<Token>().GetEnumerator();

            return _children!.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if(_children == null)
                return Enumerable.Empty<Token>().GetEnumerator();

            return _children!.GetEnumerator();
        }

        public int IndexOf(Token item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Token item)
        {
            if(_children == null)
                _children = new List<Token>();

            _children.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if(_children != null)
                _children.RemoveAt(index);
        }

        public void Add(Token item)
        {
            if(_children == null)
                _children = new List<Token>();

            _children.Add(item);
        }

        public void Merge(Token token)
        {
            CheckChildren();

            if(token.IsExpression && token.Count > 0)
                _children!.AddRange(token);
            else
                _children!.Add(token);
        }
        
        public void Clear()
        {
            if(_children != null)
                _children.Clear();
        }

        public bool Contains(Token item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Token[] array, int arrayIndex)
        {
            if(_children != null)
                _children.CopyTo(array, arrayIndex);
        }

        public bool Remove(Token item)
        {
            throw new NotImplementedException();
        }

        public int Count => _children?.Count ?? 0;

        public bool IsReadOnly => false;

        public Token this[int index] 
        { 
            get => _children != null ? _children[index] : throw new ArgumentOutOfRangeException(); 

            set
            { 
                CheckChildren();

                _children![index] = value;
            }
        }

        #endregion

        #region Private

        private void CheckChildren()
        {
            if(_children == null)
                _children = new List<Token>();
        }

        #endregion
    }
}
