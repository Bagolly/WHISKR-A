using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


namespace ParseEngine
{   
    /// <summary>
    /// Defines a function used by an <see cref="Evaluator"/> instance.
    /// </summary>
    public readonly struct Function
    {   

        /// <summary>
        /// The name of the function.
        /// </summary>
        public string Name { get; init; }
        
        
        /// <summary>
        /// The named parameters of the function, in the order they will be provided.
        /// </summary>
        public string[] Parameters { get; init; }
        
        
        /// <summary>
        /// The body of the function, a self-contained expression to be executed by an <see cref="Evaluator"/> instance.
        /// </summary>
        public string Body { get; init; }


        /// <summary>
        /// Creates a <see cref="Function"/> instance.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="param">The named parameters of the function, in the order they will be provided.</param>
        /// <param name="body">The body of the function, a self-contained expression to be executed by an <see cref="Evaluator"/> instance.</param>
        public Function(string name, string[] param, string body)
        {
            Name = name;
            Parameters = param.Reverse().ToArray();
            Body = body;
        }

        public override string ToString() => $"{Name}({Parameters.Length} parameters) : {Body}";

        public override bool Equals([NotNullWhen(true)] object obj) => base.Equals(obj);
        
        public override int GetHashCode() => base.GetHashCode();   
    }
}