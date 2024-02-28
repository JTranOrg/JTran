/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: TToken.cs					    		        
 *        Class(es): TToken			         		            
 *          Purpose: Base class for all elements               
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 08 Jan 2024                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                                                                                                 
 *    http://www.opensource.org/licenses/mit-license.php                    
 * 
 ****************************************************************************/

using JTran;
using JTran.Common;
using JTran.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal abstract class TToken
    {
        public abstract void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap);

        internal TContainer? Parent { get; set; }

        /****************************************************************************/
        private IValue CreateSimpleValue(CharacterSpan? sval)
        {
            if(sval?.TryParseNumber(out double dval) ?? false)
                return new NumberValue(dval);

            return new SimpleValue(sval);
        }
      
        private static readonly Dictionary<CharacterSpan, bool> _names = new Dictionary<CharacterSpan, bool>
        {
            { CharacterSpan.FromString("#arrayitem"), true }, 
            { CharacterSpan.FromString("#mapitem"),   true }, 
            { CharacterSpan.FromString("#if"),        true }, 
            { CharacterSpan.FromString("#else"),      true }, 
            { CharacterSpan.FromString("#elseif"),    true } 
        };

        private readonly static CharacterSpan _include      = CharacterSpan.FromString("#include");
        private readonly static CharacterSpan _exclude      = CharacterSpan.FromString("#exclude");
        private readonly static CharacterSpan _innerjoin    = CharacterSpan.FromString("#innerjoin");
        private readonly static CharacterSpan _outerjoin    = CharacterSpan.FromString("#outerjoin");
        private readonly static CharacterSpan _calltemplate = CharacterSpan.FromString("#calltemplate");
       
        internal readonly static CharacterSpan EmptyArray   = CharacterSpan.FromString("[]");
        internal readonly static CharacterSpan EmptyObject  = CharacterSpan.FromString("{}");

        /****************************************************************************/
        internal protected IValue CreateValue(object? val, bool name, long lineNumber)
        {
            if(val != null)
            {
                if(val is CharacterSpan cspan)
                    return InternalCreateValue(cspan, name, lineNumber);

                if(val is double dval)
                    return new NumberValue(dval);
            }

            return new SimpleValue(val);
        }

        /****************************************************************************/
        private protected IValue InternalCreateValue(CharacterSpan? sval, bool name, long lineNumber)
        {
            if(sval![0] != '#' || sval.Length == 1) // Allow "#" as a string literal
                return CreateSimpleValue(sval);

            var elementName = sval.SubstringBefore('(');

            if(elementName[0] == '#' && elementName.Length == 1)
            { 
                var expr = sval.Substring(2, sval.Length - 3);

                try
                { 
                    return new ExpressionValue(expr.ToString());
                }
                catch(JsonParseException ex)
                {
                    ex.LineNumber = lineNumber;
                    throw ex;
                }
            }

            if(name)
            { 
                if(_names.ContainsKey(elementName))
                    return CreateSimpleValue(sval);                    
            }

            else
            { 
                if(_include.Equals(elementName))      return new TIncludeExcludeProperty(sval, true, lineNumber);
                if(_exclude.Equals(elementName))      return new TIncludeExcludeProperty(sval, false, lineNumber);
                if(_innerjoin.Equals(elementName))    return new TInnerOuterJoinProperty(sval, true, lineNumber);
                if(_outerjoin.Equals(elementName))    return new TInnerOuterJoinProperty(sval, false, lineNumber);  
                if(_calltemplate.Equals(elementName)) return new TCallTemplateProperty(sval, lineNumber); 
                    
                var templateName = sval.SubstringBefore('(', 0);
                var theRest      = sval.SubstringAfter('(');
                var parm         = CharacterSpan.FromString("#calltemplate(" + templateName.ToString() + "," + theRest.ToString()); // ??? optimize

                // Will do exception on evaluation if no template found
                return new TCallTemplateProperty(parm, lineNumber);
            }

            throw new Transformer.SyntaxException($"Unknown element name: {elementName}") { LineNumber = lineNumber };    
        }   
    }
}
