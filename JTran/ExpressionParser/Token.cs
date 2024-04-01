using JTran.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JTran.Parser
{
   /*****************************************************************************/
    /*****************************************************************************/
    internal class Token : IList<Token>
    { 
        /*****************************************************************************/
        internal Token()
        {
        }

        /*****************************************************************************/
        internal Token(string val, TokenType tokenType = TokenType.Text)
        {
           this.Value = val;
           this.Type  = tokenType;
        }

        internal int           Id    { get; set; } = 0;
        internal string        Value { get; set; } = "";
        internal TokenType     Type  { get; set; } = TokenType.Text;

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

                default:                       return this.Value.ToString();
            }
        }

        /*****************************************************************************/
        internal const string BeginParen = "(";
        internal const string BeginArray = "[";
        internal const string EndParen = ")";
        internal const string EndArray = "]";

        /*****************************************************************************/
        internal bool IsOperator       => this?.Type == TokenType.Operator;
        internal bool IsPunctuation    => this?.Type == TokenType.Punctuation;
        internal bool IsComma          => this.IsOperator && this.Value == ",";
        internal bool IsBeginParen     => this.IsOperator && this.Value == BeginParen;
        internal bool IsEndParen       => this.IsOperator && this.Value == EndParen;
        internal bool IsBeginArray     => this.IsOperator && this.Value == BeginArray;
        internal bool IsBeginBoundary  => this.IsOperator && (this.Value == BeginArray || this.Value == BeginParen);
        internal bool IsEndBoundary    => this.IsOperator && (this.Value == EndArray || this.Value == EndParen || this.Value == ",");
        internal bool IsMultiDot       => this.IsOperator && this.Value == ".";
        internal bool IsTertiary       => this.IsOperator && "?:".Contains(this.Value);
        internal bool IsExpression     => this.Type >= TokenType.Expression;
        internal bool IsLiteral        => this == null ? false : this?.Type == TokenType.Literal || this?.Type == TokenType.DLiteral;

        /*****************************************************************************/
        internal enum TokenType
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
