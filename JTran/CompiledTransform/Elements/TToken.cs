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
        internal protected IValue CreateValue(object? value, bool name, long lineNumber)
        {
            return CreateValue(value?.ToString(), name, lineNumber);
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
        private IValue CreateValue(string? sval, bool name, long lineNumber)
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
                    case "#include":       return new TIncludeExcludeProperty(sval, true, lineNumber);
                    case "#exclude":       return new TIncludeExcludeProperty(sval, false, lineNumber);
                    case "#innerjoin":     return new TInnerOuterJoinProperty(sval, true, lineNumber);
                    case "#outerjoin":     return new TInnerOuterJoinProperty(sval, false, lineNumber);  
                    case "#calltemplate":  return new TCallTemplateProperty(sval, lineNumber); 
                    
                    default:            
                    {
                        var templateName = sval.SubstringBefore("(")[1..];
                        var theRest      = sval.SubstringAfter("(");
                        var parm         = "#calltemplate(" + templateName + "," + theRest;

                        // Will do exception on evaluation if no template found
                        return new TCallTemplateProperty(parm, lineNumber);
                    }
                }
            }

            throw new Transformer.SyntaxException($"Unknown element name: {elementName}");    
        }   
    }
}
