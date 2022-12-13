namespace ParseEngine
{   
    /// <summary>
    /// Defines an <see langword="interface"/> for token types used by the <see cref="Evaluator"/>.
    /// </summary>
    internal interface ITokenizable
    {   
        /// <summary>
        /// The range of possible operation types.
        /// </summary>
        public enum OpType
        {
            UnaryPrefix,
            UnaryPostfix,
            BinaryLeft,
            BinaryRight,
            ParanthesisOpen,
            ParanthesisClose,
            Number,
            FunctionOP,
            Constant,
            ArgumentSeparator,
            EOF,
            Undefined
        }

        const char EOF = '$';

        /// <summary>
        /// The operator.
        /// </summary>
        public string Operator { get; init; }

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public byte Precedence { get; init; }

        /// <summary>
        /// The type of the operation.
        /// </summary>
        public OpType Type { get; init; }
    }
}