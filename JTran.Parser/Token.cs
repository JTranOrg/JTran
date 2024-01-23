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

        public string    Value { get; set; } = "";
        public TokenType Type  { get; set; } = TokenType.Text;

        protected List<Token>? _children;

        /*****************************************************************************/
        public override string ToString()
        {
            return this.Value;
        }

        public bool IsOperator     => this?.Type == TokenType.Operator;
        public bool IsBoundary     => this.IsOperator && _allBoundaries.ContainsKey(this.Value);
        public bool IsEndBoundary  => this.IsOperator && _endBoundary.ContainsKey(this.Value);
        public bool IsComma        => this.IsOperator && this.Value == ",";
        public bool IsBeginParen   => this.IsOperator && this.Value == "(";
        public bool IsEndParen     => this.IsOperator && this.Value == ")";
        public bool IsMultiDot     => this.IsOperator && this.Value == ".";
        public bool IsConditional  => this.IsOperator && _conditionals.ContainsKey(this.Value);
        public bool IsTertiary     => this.IsOperator && "?:".Contains(this.Value);
        public bool IsMathematical => this.IsOperator && _mathOperators.ContainsKey(this.Value);
        public bool IsExpression   => this.Type >= TokenType.Expression;

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

            _children.Add(item);
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

        private static IDictionary<string, bool> _mathOperators = new Dictionary<string, bool>
        {
            { "*", true },
            { "/", true },
            { "+", true },
            { "-", true },
            { "%", true }
        };

        private static IDictionary<string, bool> _conditionals = new Dictionary<string, bool>
        {
            { "&&",  true },
            { "and", true },
            { "||",  true },
            { "or",  true }
        };

        #endregion
    }
}
