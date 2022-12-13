using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static ParseEngine.ITokenizable;


namespace ParseEngine
{
    internal sealed class Parser
    {   
 
        private Evaluator Evaluator { get; init; }

        internal Parser(Evaluator caller) => Evaluator = caller;

        private static void DEBUG_ParserOutput(Stack<double> numbers)
        {
            Console.WriteLine("Result: (all numbers):");
            foreach (double number in numbers)
                Console.WriteLine(number);

            Console.WriteLine("PARSING FINISHED.");
        }

        public double Parse(List<ITokenizable> tokens)
        {   
            /*
             *  Local function to increase code reuse for binary operators. 
             *  Simply pops the two operands and returns them, as well as dequeueing the
             *  current operator, as the correct operation is already known and selected
             *  - see Peek() on every branch.
             */
            static (double lhs, double rhs) PrepareOperation(Queue<ITokenizable> tokenStream, Stack<double> numbers) 
            => (numbers.Pop(), numbers.Pop());
            

            /*
             *  Converts the tokens recieved from the tokenizer to postfix notation
             *  using an extended shunting-yard algorithm, eliminating the problem of precedence.
             *  All numbers will be pushed to their own stack directly from the queue.
             *  The correct amount of numbers will be popped from the stack depending
             *  on the type of operation - mainly, binary or unary.
             */
            Queue<ITokenizable> pTokens = ConvertToPostfix(tokens);
            Stack<double> numbers = new();

            while (pTokens.Count > 0)
            {
                ITokenizable token = pTokens.Dequeue();
               

                if (token.Type is OpType.Number)
                {
                    numbers.Push(double.Parse(token.Operator));
                }
            
                /*
                 *  When a constant token is found, look up the value from the constants
                 *  dictionary. Push the value with the matching key to the numbers stack,
                 *  then dequeue the token, as it is completely processed.
                 */
                else if (token.Type is OpType.Constant)
                {
                    numbers.Push(Evaluator.Constants.Where(x => x.Key.ToLower() == token.Operator).First().Value);                                                                                          
                }

                /*
                 *  All binary operators use the same logic
                 */
                else if(token.Type is OpType.BinaryLeft or OpType.BinaryRight)
                {
                    var (lhs, rhs) = PrepareOperation(pTokens, numbers);

                    switch (token.Operator)
                    {
                        case "+":
                            numbers.Push(lhs + rhs);
                            continue;
                        case "-":
                            numbers.Push(rhs - lhs);
                            continue;
                        case "*":
                            numbers.Push(lhs * rhs);
                            continue;
                        case "/":
                            numbers.Push(rhs / lhs);
                            continue;
                        case "^":
                            numbers.Push(Math.Pow(rhs, lhs));
                            continue;
                    }
                }
                
                /*
                 *  Similarly, unary operations can be done in a single switch
                 */
                else if(token.Type is OpType.UnaryPrefix or OpType.UnaryPostfix)
                {
                    switch (token.Operator)
                    {
                        case "-":
                            numbers.Push(-numbers.Pop());
                            continue;
                        
                        case "!":
                            numbers.Push(Factor(numbers.Pop()));
                            continue;
                    }
                }

                else if (token.Type is OpType.ArgumentSeparator)
                {
                    continue;
                }

                /*
                 *  While the tokenizer looks up if the function exists, it does not relay 
                 *  wether it is a built-in or a custom one, necessitating another check.
                 *  After testing for built-in functions, (as they currently have higher precedence)
                 *  it runs the function as a custom function, in the case that no built-in
                 *  function name matched.
                 */
                else if (token.Type is OpType.FunctionOP)
                {
                    if (Evaluator.BuiltInFunctions.Any(x => x == token.Operator))            
                    {
                        numbers.Push(RunBuiltinFunction(token, numbers));
                    }

                    else                                                                               
                    {
                        numbers.Push(RunCustomFunction(token, numbers));
                    }
                }
            }

            /*DEBUG TOGGLE*/
            DEBUG_ParserOutput(numbers);
            
            /*
             * Returns the final result, the single last number in case of a successful parse.
             */
            return numbers.Pop();                             
        }

        #region PARSING METHODS
        private static double Factor(double factor)
        {
            double result = 1;

            if (factor > 170)
            {
                Console.WriteLine($"WARNING: Factors are currently limited to double.MaxValue ({double.MaxValue}) which translates to a max base value of 170. Current parameter {factor} overflowed!");
            }
            while (factor != 1)
            {
                result *= factor;
                --factor;
            }
            return result;
        }
       
        /*
         * Executes a built-in function. These are functions defined in the System.Math 
         * library, requiring little parser-side code to implement.
         */
        private static double RunBuiltinFunction(ITokenizable token, Stack<double> numbers)
        {
            return token.Operator switch
            {
                "sqrt" => Math.Sqrt(numbers.Pop()),
                "cbrt" => Math.Cbrt(numbers.Pop()),
                "abs" => Math.Abs(numbers.Pop()),
                "bitdcr" => Math.BitDecrement(numbers.Pop()),
                "bitinc" => Math.BitIncrement(numbers.Pop()),
                "floor" => Math.Floor(numbers.Pop()),
                "ceil" => Math.Ceiling(numbers.Pop()),
                "exp" => Math.Exp(numbers.Pop()),
                "ilogb" => Math.ILogB(numbers.Pop()),
                "log10" => Math.Log10(numbers.Pop()),
                "log2" => Math.Log2(numbers.Pop()),
                "log" => Math.Log(numbers.Pop()),
                "truncate" => Math.Truncate(numbers.Pop()),
                "sin" => Math.Sin(numbers.Pop()),
                "sinh" => Math.Sinh(numbers.Pop()),
                "asin" => Math.Asin(numbers.Pop()),
                "asinh" => Math.Asinh(numbers.Pop()),
                "cos" => Math.Cos(numbers.Pop()),
                "cosh" => Math.Cosh(numbers.Pop()),
                "acos" => Math.Acos(numbers.Pop()),
                "acosh" => Math.Acosh(numbers.Pop()),
                "tan" => Math.Tan(numbers.Pop()),
                "tanh" => Math.Tanh(numbers.Pop()),
                "atan" => Math.Atan(numbers.Pop()),
                "atanh" => Math.Atanh(numbers.Pop()),
                "pow" => Extensions.StackPow(numbers.Pop(), numbers.Pop()),
                "max" => Math.Max(numbers.Pop(), numbers.Pop()),
                "min" => Math.Min(numbers.Pop(), numbers.Pop()),
                "maxmg" => Math.MaxMagnitude(numbers.Pop(), numbers.Pop()),
                "minmg" => Math.MinMagnitude(numbers.Pop(), numbers.Pop()),
                "rnd" => Extensions.StackRound(numbers.Pop(), numbers.Pop()),
                "nrt" => Extensions.NthRoot(numbers.Pop(), numbers.Pop()),
                "rand" => Extensions.Rand(),
                "randint" => Extensions.RandInt(numbers.Pop(), numbers.Pop()),
                _ => 0,
            };
        }

        /*
         *  Executes a custom function defined at initialization time.
         *  Since custom functions are first-class citizens, ie. self-contained expressions
         *  they need to be parsed as such.
         *  
         *  In practice this involves creating a new dictionary for the constants, with the
         *  functions parameters added in as new constants, creating a new Evaluator instance
         *  and executing the function as a separate expression, returning its result.
         *  
         *  When functions are nested inside other functions, ie. pow(pow(2)), however, name collisions 
         *  will happen, as the parser tries to add the function parameters again.
         *  For example: pow(pow(2)) 
         *      I. the parser adds 'x' as a constant and executes the function
         *      II. in the second, recursive, run the parameter 'x' already exists, and since
         *          duplicate names are not allowed in dictionaries, a name collision will occur.
         *  
         *  To mitigate this problem with recursive function calls, the parser will first check
         *  for constant names with the same name, and if it exists, replace it with the current
         *  value, essentially 'updating' it.
         *  
         *  
         *  The method pops the correct number of numbers from the numbers stack to fill 
         *  every parameter of the provided function.
         */
        private double RunCustomFunction(ITokenizable token, Stack<double> numbers)
        {
            Function function = Evaluator.Functions.First(x => x.Name == token.Operator);       
            Dictionary<string, double> originalAndFuncParams = Evaluator.Constants;      
                                                                                                  
            for (int i = 0; i < function.Parameters.Length; i++)                                     
            {
                double num = numbers.Pop();                                                    
                
                if (originalAndFuncParams.ContainsKey(function.Parameters[i]))                 
                {
                    originalAndFuncParams.Remove(function.Parameters[i]);
                    originalAndFuncParams.Add(function.Parameters[i], num);
                }

                else                                                                            
                {
                    originalAndFuncParams.Add(function.Parameters[i], num);
                }
            }

            Evaluator funcEval = new(Evaluator.Functions, originalAndFuncParams);     

            return funcEval.Evaluate(function.Body);                                                
        }


        private static Queue<ITokenizable> ConvertToPostfix(List<ITokenizable> tokenStream)
        {
            Queue<ITokenizable> outStream = new();
            Stack<ITokenizable> opStack = new();

            if (tokenStream.Where(x => x.Type is ITokenizable.OpType.ParanthesisOpen).Count() != tokenStream.Where(y => y.Type is ITokenizable.OpType.ParanthesisClose).Count())
            {
                Console.WriteLine($"WARNING: Mismatched paranthesis in input!");
            }

            for (int i = 0; i < tokenStream.Count; i++)
            {
                // Console.WriteLine("to postfix:" + tokenStream[i]);
                if (tokenStream[i].Type is ITokenizable.OpType.ArgumentSeparator)
                {
                    outStream.Enqueue(tokenStream[i]);
                }

                else if (tokenStream[i].Type is ITokenizable.OpType.EOF)                                                                    //Safety branch in case of duplicate EOF by moronic end-user
                {
                    break;
                }

                else if (tokenStream[i].Type is ITokenizable.OpType.Number or ITokenizable.OpType.Constant or ITokenizable.OpType.UnaryPostfix)
                {
                    outStream.Enqueue(tokenStream[i]);
                }

                else if (tokenStream[i].Type is ITokenizable.OpType.ParanthesisOpen or ITokenizable.OpType.UnaryPrefix)
                {
                    opStack.Push(tokenStream[i]);
                }

                else if (tokenStream[i].Type is ITokenizable.OpType.FunctionOP)
                {
                    opStack.Push(tokenStream[i]);
                }

                else if (tokenStream[i].Type == ITokenizable.OpType.ParanthesisClose)
                {
                    while (opStack.Count != 0 && opStack.Peek().Type is not ITokenizable.OpType.ParanthesisOpen)
                        outStream.Enqueue(opStack.Pop());

                    if (opStack.Peek().Type is ITokenizable.OpType.ParanthesisOpen)
                    {
                        opStack.Pop();
                    }                                                                                                      //should only be the remaining '(' anyways
                }

                else                                                                                                                        //should only be the "normal" operators left
                {
                    if (opStack.Count == 0)
                    {
                        opStack.Push(tokenStream[i]);
                    }

                    else
                    {
                        if (tokenStream[i].Type == ITokenizable.OpType.BinaryLeft)
                        {
                            while (opStack.Count != 0 && opStack.Peek().Precedence >= tokenStream[i].Precedence && opStack.Peek().Type is not ITokenizable.OpType.ParanthesisOpen)
                                outStream.Enqueue(opStack.Pop());
                        }

                        else if (tokenStream[i].Type == ITokenizable.OpType.BinaryRight)
                        {
                            while (opStack.Count != 0 && opStack.Peek().Precedence > tokenStream[i].Precedence && opStack.Peek().Type != ITokenizable.OpType.ParanthesisOpen)
                                outStream.Enqueue(opStack.Pop());
                        }

                        opStack.Push(tokenStream[i]);
                    }
                }
            }

            while (opStack.Count != 0)
                outStream.Enqueue(opStack.Pop());

            foreach (var t in outStream)
                Console.WriteLine(t);

            Console.WriteLine("POSTFIX PARSING FINISHED. PRESS ANY KEY TO START EVALUATION.");
            Console.ReadKey();
            return outStream;
        }
        #endregion
    }
}