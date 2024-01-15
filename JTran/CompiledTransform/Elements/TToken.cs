﻿/***************************************************************************
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

using System;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    internal abstract class TToken
    {
        public abstract void Evaluate(IJsonWriter output, ExpressionContext context, Action<Action> wrap);

        /****************************************************************************/
        internal protected IValue CreateValue(object? value)
        {
            return CreateValue(value?.ToString());
        }  
        
        internal TContainer? Parent { get; set; }

        /****************************************************************************/
        private IValue CreateValue(string? sval)
        {
            if(sval == null)
                return new SimpleValue(sval);

            if(!sval.StartsWith("#("))
            { 
                if(double.TryParse(sval, out double val))
                    return new NumberValue(val);

                return new SimpleValue(sval);
            }

            if(!sval.EndsWith(")"))
                throw new Transformer.SyntaxException("Missing closing parenthesis");

            var expr = sval.Substring(2, sval.Length - 3);

            return new ExpressionValue(expr);
        }   
    }
}
