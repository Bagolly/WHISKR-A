using static ParseEngine.ITokenizable.OpType;
using static ParseEngine.ITokenizable;
using System.Diagnostics.CodeAnalysis;

namespace ParseEngine
{   
    /// <summary>
    /// Represents a token, out of which expressions are built.
    /// </summary>
    internal readonly struct Token : ITokenizable
    {   
        private static int SetPrecedence(string opChar, OpType type) => type switch
        {
            ArgumentSeparator => 1,

            Number or Constant => 2,

            BinaryLeft => opChar is "+" or "-" ? 3 : opChar is "*" or "/" ? 4 : 0,

            BinaryRight => opChar is "^" ? 5 : 0,

            UnaryPrefix => opChar is "-" or "+" ? 6 : 0,

            UnaryPostfix => opChar is "!" ? 7 : 0,

            ParanthesisOpen or ParanthesisClose or OpType.FunctionOP => 8,

            _ => 0,
        };

        public readonly string Operator { get; init; }
        
        public readonly byte Precedence { get; init; }
        
        public readonly OpType Type { get; init; }
        
        
        /// <summary>
        /// Creates a new <see cref="Token"/>, using the provided operation and its type.
        /// </summary>
        public Token(string op, OpType type)
        {
            Operator = op;
            Type = type;
            Precedence = (byte)SetPrecedence(Operator, Type);
        }

        public override string ToString() => $"Operation: \"{Operator}\"    Precedence:[{Precedence}]    Type:{Type}";

        public override bool Equals([NotNullWhen(true)] object obj) => base.Equals(obj);
        
        public override int GetHashCode() => base.GetHashCode();
    }
}