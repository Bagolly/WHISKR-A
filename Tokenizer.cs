using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using static ParseEngine.ITokenizable.OpType;
using static ParseEngine.ITokenizable;


namespace ParseEngine
{
    internal sealed class Tokenizer
    {
        private Evaluator BaseClass { get; init; }
        public Tokenizer(Evaluator baseClass) { BaseClass = baseClass; }

        private static void DEBUG_TokenizerOutput(List<ITokenizable> tStream)
        {
            foreach (var token in tStream)
                Console.WriteLine(token);
            Console.WriteLine("TOKENIZER FINISHED. PRESS ANY KEY TO START POSTFIX CONVERSION.");
            Console.ReadKey();
        }
        private static string ThrowWarning(in string src, in int i)
        {
            int forwLength = src.Length - i > 15 ? 15 : src.Length - i;                                         //if remaining chars greater than "X" then default to "X", otherwise use max length until end
            int backwLength = i > 15 ? 15 : i;                                                                  //if previous chars greater than "Y" then default to "Y" otherwise use all char from start
            bool ellipseInserted = false;

            string errorContext = src.Substring(i - backwLength, backwLength + forwLength);

            if (errorContext[^1] == ITokenizable.EOF)                                                           //remove EOF to not confuse the user with the extra character
            {
                errorContext = errorContext.Remove(errorContext.Length - 1);
            }

            if (i + forwLength < src.Length)
            {
                errorContext += " ...";
            }

            if (i - backwLength > 0)
            {
                errorContext = errorContext.Insert(0, "... ");
                ellipseInserted = true;
            }

            errorContext += "\n";                                                                               //pointer will be on this new line

            for (int k = ellipseInserted ? -4 : 0; k < backwLength; k++) { errorContext += " "; }                             //error position is 0+backwardlength; pad until position reached. Optional -4 if ellipsis insrted to accomodate offset "... " is 4 chars

            errorContext += "^- HERE";                                                                          //append error pointer
            return errorContext;
        }

        public List<ITokenizable> Tokenize(in string input)
        {
            List<ITokenizable> tokenStream = new();
            bool doNotShowInvalidTokenWarning = false;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].IsWhiteSpace())
                {
                    doNotShowInvalidTokenWarning = false;
                    continue;
                }

                else if (input[i] is ITokenizable.EOF)
                {
                    break;
                }

                else if (input[i].IsDigit())
                {
                    tokenStream.Add(ParseNumber(in input, ref i));
                    doNotShowInvalidTokenWarning = false;
                }

                else if (input[i].IsValidOperator())
                {
                    tokenStream.Add(ParseOperator(in input, ref i, tokenStream));
                    doNotShowInvalidTokenWarning = false;
                }

                else if (TryParseFunction(in input, ref i, out ITokenizable func))
                {
                    tokenStream.Add(func);
                    doNotShowInvalidTokenWarning = false;
                }

                else if (TryParseVariable(in input, ref i, out ITokenizable var))
                {
                    tokenStream.Add(var);
                    doNotShowInvalidTokenWarning = false;
                }

                else if (!doNotShowInvalidTokenWarning)
                {
                    Console.WriteLine($"WARNING: Skipping invalid token starting at position {i + 1}\n{ThrowWarning(in input, in i)}");
                    doNotShowInvalidTokenWarning = true;
                }
            }

            /*  
             *  DEBUG TOGGLE
             */
            //DEBUG_TokenizerOutput(tokenStream);

            tokenStream.Add(new Token("$", OpType.EOF));
            return tokenStream;
        }

        #region PARSING METHODS
        private static ITokenizable ParseNumber(in string str, ref int i)
        {
            int tmpI = i;
            string buffer = string.Empty;

            while (true)
            {   
                /*
                 *  The first two branches handle decimal digits and decimal separators (as defined by the current culture)
                 *  The end of a number is either whitespace or the EOF token
                 */               
                if (str[tmpI].IsWhiteSpace() || str[tmpI] == '$')
                {
                    ++tmpI;
                    break;
                }

                else if (str[tmpI].IsDigit() || str[tmpI] == Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                {
                    buffer += str[tmpI];
                    ++tmpI;
                }

                /*
                 *  Branch for handling hexadecimal, octal and binary numbers. 
                 *  Starts with indexer increase to remove the prefixes 'x', 'b' or 'o',
                 *  as Convert.ToInt64/Int32 cannot parse them. The 0s can stay, leading 0s won't change the value. 
                 */
                else if (buffer[^1] == '0' && char.IsLetter(str[tmpI]))                                                                
                {
                    tmpI++;

                    if (str[tmpI].ToUpper() == 'X')
                    {

                        char[] validHexChar = { 'A', 'B', 'C', 'D', 'E', 'F' };

                        while (str[tmpI] != '$' && str[tmpI].IsDigit() || validHexChar.Contains(str[tmpI].ToUpper()))
                        {
                            buffer += str[tmpI];
                            tmpI++;
                        }

                        buffer = Convert.ToInt64(buffer, 16).ToString();
                        break;
                    }

                    else if (str[tmpI].ToUpper() == 'B')
                    {
                      
                        while (str[tmpI] != '$' && str[tmpI] == '0' || str[tmpI] == '1')
                        {
                            buffer += str[tmpI];
                            tmpI++;
                        }
                        buffer = Convert.ToInt32(buffer, 2).ToString();
                        break;
                    }

                    else if (str[tmpI].ToUpper() == 'O')
                    {
                  
                        while (str[tmpI] != '$' && str[tmpI].IsDigit() && str[tmpI] != '8' && str[tmpI] != '9')
                        {
                            buffer += str[tmpI];
                            tmpI++;
                        }
                        buffer = Convert.ToInt64(buffer, 8).ToString();
                        break;
                    }
                }
                
                /*
                 *  Branch for handling exponential notation, detected by any digit followed by an 'e'
                 */
                else if (str[tmpI].ToUpper() == 'E' && buffer[^1].IsDigit())
                {
                    buffer += str[tmpI];
                    tmpI++;
                    
                    while ((str[tmpI] != '$' && str[tmpI].IsDigit()) || str[tmpI] == '-' || str[tmpI] == '+')
                    {
                        buffer += str[tmpI];
                        tmpI++;
                    }
                    
                    break;
                }

                /*
                 *  No valid branches, finish parsing.
                 */
                else
                    break;
            }

            /*
             * The actual indexer is the current - 1. Without this, characters directly after numbers would be skipped.
             */
            i = tmpI - 1; 
            return new Token(buffer, Number);
        }
        
        private static ITokenizable ParseOperator(in string src, ref int i, List<ITokenizable> tStream)
        {   
            
            /*
             *  If there are operators that cannot be the last token at the end, warn the user.
             *  Possibly prevents errors with inputs such as '1 + 1/' or '4 * 2 ^'
             */
            if (src[i] is not '!' and not ')' && src[i + 1] is ITokenizable.EOF)
            {
                Console.WriteLine($"WARNING: Skipping invalid use of \"{src[i]}\" operator at last position. \n{ThrowWarning(in src, in i)}");
                return null;
            }

            
            /*
             *  Branch for handling the '-' operator. It is considered 'negative' (unary) if: 
             *      -it's either the first token,
             *      -the previous token is a binary operator
             *      -the previous token is an opening paranthesis 
             *  otherwise it will be interpreted as 'subtraction' (binary).
             */
            if (src[i] is '-')
            {
                if (tStream.Count == 0 || (tStream.Last().Type is BinaryLeft or BinaryRight or ParanthesisOpen))
                {
                    return new Token(src[i].ToString(), UnaryPrefix);
                }

                else
                {
                    return new Token(src[i].ToString(), BinaryLeft);
                }
            }

            return src[i] switch
            {
                '+' or '*' or '/' => new Token(src[i].ToString(), BinaryLeft),
                '^' => new Token(src[i].ToString(), BinaryRight),
                '!' => new Token(src[i].ToString(), UnaryPostfix),
                '(' => new Token(src[i].ToString(), ParanthesisOpen),
                ')' => new Token(src[i].ToString(), ParanthesisClose),
                ',' => new Token(src[i].ToString(), ArgumentSeparator),
                _ => (ITokenizable)new Token(string.Empty, Undefined),
            };
        }

        private bool TryParseFunction(in string src, ref int i, out ITokenizable token)
        {
            /*
             *  A temporary indexer is utilized to protect indexer from unnecessary modificiation. 
             *      -if the token isn't a function, the method returns and the main loop will try to parse it as a constant 
             *      -if it is, the main indexer will be updated before the method returns
             */
            int tmpI = i;                                                                                       
            string funcName = string.Empty;

            /*
             *  Only the name of the function will be parsed, and returned as a token.
             *  If the EOF is reached before a '(' is found, it cannot be a function,
             *  however it can still be a name for a constant, therefore no warning is thrown.
             */
            while (src[tmpI] is not '(')                                                                         
            {
                if (!src[tmpI].IsWhiteSpace())
                {
                    funcName += src[tmpI];
                }

                tmpI++;

                if (src[tmpI] is ITokenizable.EOF)
                {
                    token = null;
                    return false;
                }
            }

            /*
             *  In the case, that no providedd function name matched, the built-in methods will be checked.
             *  If no built-in functions matched the token either, the method returns false, leaving the indexer untouched.
             *  This happens usually because the token is the name for a constant or just does not exist.
             *  Performance hit for very long names unlikely, though untested.
             */
            if (!BaseClass.Functions.Any(x => x.Name == funcName))                                                   
            {
                token = BaseClass.BuiltInFunctions.Any(x => x == funcName) ? token = new Token(funcName, FunctionOP) : token = null;

                if (token is not null)
                {
                    i = tmpI - 1;
                    return true;
                }

                else
                    return false;
            }

            /*
             * One of the provided function names matched the token, return it as a function token.
             */
            else                                                                                                            
            {
                i = tmpI - 1;                                                                                    
                token = new Token(funcName, FunctionOP);
                return true;
            }
        }

        private bool TryParseVariable(in string src, ref int i, out ITokenizable token)
        {   
            /*
             *  When no built-in or provided function names matched the token, this method will try to parse it as a constant.
             *  Parsing the constant name will break when:
             *      -a valid operator has been found or
             *      -whitespace has been found or
             *      -the EOF token was found
             */

            int bufferIndexer = i;
            string key = string.Empty;

            while (!src[bufferIndexer].IsValidOperator() && !src[bufferIndexer].IsWhiteSpace() && src[bufferIndexer] is not ITokenizable.EOF)
            {
                key += src[bufferIndexer];
                bufferIndexer++;
            }

            /*
             *  If a matching name was found, it will be used as the tokens operation, the value itself will be fetched 
             *  at evaluation time.
             *  If a name was found, returns true.
             */

            token = BaseClass.Constants.ContainsKey(key) ? new Token(key, Constant) : new Token(string.Empty, Undefined);
            i = token.Type is not Undefined ? bufferIndexer - 1 : i;

            return token.Type is not Undefined;
        }
        #endregion
    }
}