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

using JTran.Extensions;
using System;
using System.ComponentModel;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal abstract class TToken
    {
        public abstract void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap);

        /****************************************************************************/
        internal protected IValue CreateValue(object? value, bool name)
        {
            return CreateValue(value?.ToString(), name);
        }  
        
        internal TContainer? Parent { get; set; }

        /****************************************************************************/
        private IValue CreateSimpleValue(string? sval)
        {
            if(double.TryParse(sval, out double val))
                return new NumberValue(val);

            return new SimpleValue(sval);
        }

        /****************************************************************************/
        private IValue CreateValue(string? sval, bool name)
        {
            if(sval == null)
                return new SimpleValue(sval);

            if(!sval.StartsWith("#") || sval.Length == 1) // Allow "#" as a string literal
                return CreateSimpleValue(sval);

            var elementName = sval.SubstringBefore("(");

            if(elementName == "#")
            { 
                var expr = sval.Substring(2, sval.Length - 3);

                return new ExpressionValue(expr);
            }

            if(name)
            { 
                switch(elementName)
                {
                    case "#arrayitem":  
                    case "#mapitem":    
                    case "#if":         
                    case "#else":       
                    case "#elseif":     return CreateSimpleValue(sval);
                    default:            break;
                }
            }

            else
            { 
                switch(elementName)
                {
                    case "#include":    return new TIncludeExcludeProperty(sval, true);
                    case "#exclude":    return new TIncludeExcludeProperty(sval, false);
                    case "#innerjoin":  return new TInnerJoinProperty(sval);
                    case "#outerjoin":   
                    default:            break;
                }
            }

            throw new Transformer.SyntaxException($"Unknown element name: {elementName}");    
        }   
    }
}
