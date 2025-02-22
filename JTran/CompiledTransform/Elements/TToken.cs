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

using JTran.Common;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal interface IEvaluator
    {
        void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap);
    }

    /****************************************************************************/
    /****************************************************************************/
    internal abstract class TToken : IEvaluator
    {
        public abstract void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap);

        internal TContainer? Parent   { get; set; }
        internal bool        IsOutput { get; set; } = false;

        /****************************************************************************/
        private IValue CreateSimpleValue(ICharacterSpan? sval)
        {
            return new SimpleValue(sval);
        }
      
        private static readonly Dictionary<ICharacterSpan, bool> _names = new Dictionary<ICharacterSpan, bool>
        {
            { CharacterSpan.FromString("#arrayitem"), true }, 
            { CharacterSpan.FromString("#mapitem"),   true }, 
            { CharacterSpan.FromString("#if"),        true }, 
            { CharacterSpan.FromString("#else"),      true }, 
            { CharacterSpan.FromString("#elseif"),    true } 
        };

        private readonly static ICharacterSpan _include      = CharacterSpan.FromString("#include");
        private readonly static ICharacterSpan _exclude      = CharacterSpan.FromString("#exclude");
        private readonly static ICharacterSpan _innerjoin    = CharacterSpan.FromString("#innerjoin");
        private readonly static ICharacterSpan _outerjoin    = CharacterSpan.FromString("#outerjoin");
        private readonly static ICharacterSpan _calltemplate = CharacterSpan.FromString("#calltemplate");
        private readonly static ICharacterSpan _callelement  = CharacterSpan.FromString("#callelement");
       
        internal readonly static ICharacterSpan EmptyArray   = CharacterSpan.FromString("[]");
        internal readonly static ICharacterSpan EmptyObject  = CharacterSpan.FromString("{}");

        /****************************************************************************/
        internal protected IValue CreateValue(object? val, bool name, long lineNumber)
        {
            if(val != null)
            {
                if(val is ICharacterSpan cspan)
                { 
                    if(cspan.Length != 0)
                        return InternalCreateValue(cspan, name, lineNumber);
                }
                else if(val is decimal dval)
                    return new NumberValue(dval);
            }

            return new SimpleValue(val);
        }

        /****************************************************************************/
        private protected IValue InternalCreateValue(ICharacterSpan? sval, bool name, long lineNumber)
        {
            if(sval == null || sval.Length == 0)
                throw new ArgumentException();

            if(sval![0] != '#' || sval.Length == 1) // Allow "#" as a string literal
                return CreateSimpleValue(sval);

            var elementName = sval.SubstringBefore('(');

            if(elementName[0] == '#' && elementName.Length == 1)
            { 
                var expr = sval.Substring(2, sval.Length - 3);

                try
                { 
                    return new ExpressionValue(expr);
                }
                catch(JsonParseException ex)
                {
                    ex.LineNumber = lineNumber;
                    throw;
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
            }

            if(_callelement.Equals(elementName)) 
                return new TCallElementProperty(sval, lineNumber); 

            if(_calltemplate.Equals(elementName)) 
                return new TCallTemplateProperty(sval, lineNumber); 

            var templateCall = TCallTemplate.SubstituteCustomName(sval);

            // Will do exception on evaluation if no template found
            return new TCallTemplateProperty(templateCall, lineNumber, true);
        }   
    } 
}
