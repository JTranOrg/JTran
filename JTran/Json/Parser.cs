using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;

using JTran.Common;
using static JTran.Json.Parser;

namespace JTran.Json
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Parse a text file and convert into the JTran data object model
    /// </summary>
    internal interface IJsonParser
    {
        ExpandoObject Parse(Stream stream);
    }
    
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Parse a text file and convert into the JTran data object model
    /// </summary>
    internal class Parser : IJsonParser
    {
        private long _lineNumber = 1;

        /****************************************************************************/
        internal Parser()
        { 
        }

        /****************************************************************************/
        public ExpandoObject Parse(Stream stream) 
        {
            var reader = new CharacterReader(stream);
            var ex     = new ExpandoObject();
            var exd    = ex as IDictionary<string, object>;
            var stack  = new Stack<Context>();

            stack.Push(new Context(State.Ether, State.Ether, ex));

            StringBuilder? sb = null;
            char? previousChar = null;

            try
            { 
                while(true)
                {
                    var ch = reader.ReadNext();
                    var currentState = stack.Peek().State;

                    // If we're inside quotes just add character to stringbuilder
                    if(currentState == State.SingleQuotes || currentState == State.DoubleQuotes)
                    { 
                        if(ch == '\"' || ch == '\'') // ??? Check for escaped characters an check for quote mismatch
                        { 
                            var currentContext = stack.Pop();

                            if(stack.Peek().State == State.Property)
                            {
                                var propContext = stack.Peek();

                                propContext.Object.TryAdd(propContext.StringValue, sb.ToString());
                            }
                            else
                            {
                                stack.Peek().StringValue = sb.ToString();
                            }

                            sb = null;
                        }
                        else
                            sb.Append(ch);
                    }

                    // Increase line number
                    else if(ch == '\r')
                    {
                        ++_lineNumber;
                    }

                    // Increase line number if not newline after carriage return
                    else if(ch == '\n')
                    {
                        if(previousChar != '\r') 
                            ++_lineNumber;
                    }

                    // Ignore other whitespace
                    else if(char.IsWhiteSpace(ch))
                    {
                    }

                    // Start new object
                    else if(ch == '{')
                    { 
                        switch(currentState)
                        {
                            case State.Ether:
                                stack.Push(new Context(State.Object, stack.Peek().State, ex));
                                break;

                            case State.Property:
                                stack.Push(new Context(State.Object, stack.Peek().State, new ExpandoObject()));
                                break;

                            default:
                                break;
                        }
                    }

                    // Start new array
                    else if(ch == '[')
                    { 
                        switch(currentState)
                        {
                            case State.Property:
                                stack.Push(new Context(State.Array, stack.Peek().State));
                                break;

                            default:
                                break;
                        }
                    }

                    // End object
                    else if(ch == '}')
                    { 
                        if(stack.Peek().State == State.Property)
                            stack.Pop();

                        var newObj = stack.Pop();

                        if(stack.Peek().State == State.Property)
                            stack.Peek().Object.TryAdd(newObj.StringValue, newObj.Object);
                    }

                    // End array
                    else if(ch == ']')
                    { 
                        stack.Pop();
                    }

                    // Start single quotes
                    else if(ch == '\'')
                    { 
                        sb = new StringBuilder();
                        stack.Push(new Context(State.SingleQuotes, stack.Peek().State));
                    }

                    // Start double quotes
                    else if(ch == '\"')
                    { 
                        sb = new StringBuilder();
                        stack.Push(new Context(State.DoubleQuotes, stack.Peek().State));
                    }

                    // Start property
                    else if(ch == ':')
                    { 
                        var context = new Context(State.Property, stack.Peek().State);

                        context.StringValue = stack.Peek().StringValue;
                        context.Object = stack.Peek().Object;

                        stack.Push(context);
                    }


                    // End property
                    else if(ch == ',')
                    { 
                        stack.Pop();
                    }

                    previousChar = ch;
                }
            }
            catch(ArgumentOutOfRangeException)
            {
                // We're done
            }

            if(stack.Peek().State != State.Ether) 
                throw new Exception($"Syntax error. Line number: {_lineNumber}");

            return ex;
        }

        internal class Context
        {
            internal Context(State state, State? previousState = null, ExpandoObject ex = null)
            {
                this.State         = state;
                this.PreviousState = previousState;
                this.Object        = ex;
            }

            internal State           State         { get; }
            internal State?          PreviousState { get; }
            internal string          StringValue   { get; set; } = "";
            internal ExpandoObject   Object        { get; set; }
        }

        internal enum State
        {
            Array,
            Ether,
            Object,
            SingleQuotes,
            DoubleQuotes,
            Property
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal interface ICharacterReader
    {
        char ReadNext();    
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class CharacterReader : ICharacterReader
    {
        private readonly string _json;
        private int _position = 0;

        internal CharacterReader(Stream stream) 
        { 
            _json = stream.ReadString();
        }

        public char ReadNext()
        {
            if(_position >= _json.Length)
                throw new ArgumentOutOfRangeException(nameof(ReadNext));

            return _json[_position++];
        }
    }
}
