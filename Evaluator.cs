using System;
using System.Collections.Generic;


namespace ParseEngine
{   
    /// <summary>
    /// The main Whisker-A arithmetic expression parser, all features are contained in this class.
    /// </summary>
    public sealed class Evaluator
    {
        internal HashSet<string> BuiltInFunctions { get; } = new() 
        { "sqrt", "cbrt", "ntrt", "abs", "bitdcr", "bitinc", "floor", "ceil", 
          "exp", "logb", "log10", "log2", "log", "trunc", "sin", "sinh", "asin", 
          "asinh", "cos", "cosh", "acos", "acosh", "tan", "tanh", "atan", "rnd",
          "atanh", "pow", "max", "min", "maxmg", "minmg" 
        };

        
        /// <summary>
        /// Contains constants provided at initialization time.
        /// </summary>
        public Dictionary<string, double> Constants { get; init; }
        

        /// <summary>
        /// Contains function definitions provided at initialization time.
        /// </summary>
        public List<Function> Functions { get; init; }


        private readonly Tokenizer Tokenizer;
        

        private readonly Parser Parser;


        /// <summary>
        /// Create an instance of the <see cref="Evaluator"/>, optionally providing custom
        /// <paramref name="functions"/> and <paramref name="constants"/>definitions.
        /// </summary>
        /// <param name="functions">Function definitions to use while parsing expressions.</param>
        /// <param name="constants">Constant definitions to use while parsing expressions.</param>
        /// <remarks>Note: custom definitions cannot be added or removed after initialization.</remarks>
        public Evaluator(List<Function> functions = null, Dictionary<string, double> constants = null)
        {
            Tokenizer = new Tokenizer(this);                                                                            

            Parser = new Parser(this);

            Functions = functions ?? new List<Function>();

            Constants = constants ?? new Dictionary<string, double>();
        }

        /// <summary>
        /// Evaluates the provided <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">An expression, such as '3 * 2 + 1'</param>
        /// <returns>The result of the expression as <see cref="double"/></returns>
        /// <remarks>Expressions with a value of <see cref="string.Empty"/> or <see langword="null"/> will result in <see cref="double.NaN"/>.</remarks>
        public double Evaluate(string expression)                                       
        {
            if (expression is null) 
            {
                throw new ArgumentNullException(expression, "The provided expression was null");
            }

            else if (expression is "") 
            {
                throw new ArgumentException("The provided expression was empty", expression);
            }

            else 
            { 
                expression += ITokenizable.EOF; 
                return Parser.Parse(Tokenizer.Tokenize(expression)); 
            }
        }

        /// <summary>
        /// Evaluates the provided <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">An expression, such as '3 * 2 + 1'</param>
        /// <param name="result">The result of the expression as <see cref="double"/></param>
        /// <returns><see langword="true"/> if the expression was parsed successfully; otherwise <see langword="false"/></returns>
        /// <remarks>Note: no exceptions encountered during parsing will be thrown, instead returning <see langword="false"/>.</remarks>
        public bool TryEvaluate(string expression, out double result)
        {
            try 
            { 
                result = Parser.Parse(Tokenizer.Tokenize(expression + ITokenizable.EOF));
                return result is not double.NaN; 
            }

            catch 
            { 
                result = double.NaN; 
                return false; 
            }
        }
    }
}