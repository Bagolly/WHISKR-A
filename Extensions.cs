using System;


namespace ParseEngine
{
    /// <summary>
    /// Contains <see langword="static"/> overloads of commonly used methods to enable shorter syntax at callsites.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Indicates wheter the specified character is a categorized as an operator.
        /// </summary>
        /// <returns><see langword="true"/> if <paramref name="c"/> is an operator; otherwise, <see langword="false"/> </returns>
        internal static bool IsValidOperator(this char c) => c is '+' or '-' or '/' or '*' or '!' or '^' or '(' or ')' or ',';


        /// <summary>
        /// Indicates whether the specified Unicode character is categorized as a decimal digit.
        /// </summary>
        ///<returns><see langword = "true" /> if <paramref name = "op" /> is a decimal digit; otherwise,<see langword="false"/> </returns>
        internal static bool IsDigit(this char c) => char.IsDigit(c);


        /// <summary>
        /// Indicates whether the specified Unicode character is categorized as a Unicode letter.
        /// </summary>
        /// <param name="num"></param>
        ///<returns><see langword = "true" /> if <paramref name = "op" /> is a letter; otherwise,<see langword="false"/> </returns>
        internal static bool IsLetter(this char c) => char.IsLetter(c);


        /// <summary>
        /// Calculates the <paramref name="x"/>th root of <paramref name="y"/>.
        /// </summary>
        /// <returns>The result as <see cref="double"/></returns>
        internal static double NthRoot(double x, double y) => Math.Pow(y, 1.0 / x);

        
        /// <summary>
        /// Indicates whether the specified Unicode character is categorized as white space.
        /// </summary>
        /// <param name="num"></param>
        ///<returns><see langword = "true" /> if <paramref name = "op" /> is white space; otherwise,<see langword="false"/> </returns>
        internal static bool IsWhiteSpace(this char c) => char.IsWhiteSpace(c);


        /// <summary>
        ///  Converts the value of a Unicode character to its uppercase equivalent.
        /// </summary>
        /// <param name="num"></param>
        ///<returns> The uppercase equivalent of <paramref name="c"/>, or the unchanged value of <paramref name="c"/> if <paramref name="c"/> is already uppercase, has no uppercase equivalent, or is not alphabetic.</returns>
        internal static char ToUpper(this char c) => char.ToUpper(c);


        /// <summary>
        ///Rounds <paramref name="x"/> to a number of fractional digits specified by <paramref name="y"/>, and rounds midpoint values to the nearest even number.
        /// </summary>
        /// <returns>The number nearest to value that contains a number of fractional digits equal to <paramref name="y"/>.</returns>
        internal static double StackRound(double y, double x) => Math.Round(x, (int)y);

        
        /// <summary>
        /// Returns a specified number raised to the specified power.
        /// </summary>
        /// <returns>The number <paramref name="x"/> raised to the power of <paramref name="y"/></returns>
        internal static double StackPow(double y, double x) => Math.Pow(x, y);


        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        internal static double Rand() => Random.Shared.NextDouble();


        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <returns>A 32-bit signed integer greater than or equal to <paramref name="min"/> and less than <paramref name="max"/>; that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.</returns>
        /*
         * The upper and lower bound parameters are provided in reverse order, because of the stack popping numbers in reverse order
         */
        internal static double RandInt(double min, double max) => Random.Shared.Next(Convert.ToInt32(max), Convert.ToInt32(min));

        /// <summary>
        /// Binary operation AND.
        /// </summary>
        /// <returns>The result of <paramref name="lhs"/> AND <paramref name="rhs"/>.</returns>
        internal static double And(double rhs, double lhs) => (long)lhs & (long)rhs;

        /// <summary>
        /// Binary operation OR.
        /// </summary>
        /// <returns>The result of <paramref name="lhs"/> OR <paramref name="rhs"/>.</returns>
        internal static double Or(double rhs, double lhs) => (long)lhs | (long)rhs;

        /// <summary>
        /// Binary operation XOR.
        /// </summary>
        /// <returns>The result of <paramref name="lhs"/> XOR <paramref name="rhs"/>.</returns>
        internal static double Xor(double rhs, double lhs) => (long)lhs ^ (long)rhs;

        /// <summary>
        /// Binary operation NOT.
        /// </summary>
        /// <returns>The result of NOT <paramref name="x"/></returns>
        internal static double Not(double x) => ~(long)x;

        /// <summary>
        /// Binary operation left shift.
        /// Discards the high-order bits that are outside the range of <see langword="long"/> and sets the low-order empty bit positions to <see langword="0"></see>.
        /// </summary>
        /// <returns>The result of <paramref name="lhs"/> shifted left by <paramref name="rhs"/></returns>
        internal static double LShift(double rhs, double lhs) => (long)lhs << (int)rhs;

        /// <summary>
        /// Binary operation right shift.
        /// Performs an arithmetic shift: the value of the most significant bit (the sign bit) of the left-hand operand is propagated to the high-order empty bit positions. That is, the high-order empty bit positions are set to zero if the left-hand operand is non-negative and set to one if it's negative.
        /// </summary>
        /// <returns>The result of <paramref name="lhs"/> shifted right by <paramref name="rhs"/></returns>
        internal static double RShift(double rhs, double lhs) => (long)lhs >> (int)rhs;
    }
}