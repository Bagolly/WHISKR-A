using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using ParseEngine;

namespace Parse_2
{
    class Program
    {
        static void Main()
        {
            Evaluator parser = new
                (
                    new List<Function>()
                    {
                        new Function
                        (
                            name: "tripleParam",
                            param: new[] {"x","y","z"},
                            body: "x*y*z"
                        ),
                        
                        new Function
                        (
                            name: "lawOfCosines", 
                            param: new[] {"a", "b", "c"}, 
                            body: "sqrt(a ^ 2 + b ^ 2 - 2 * a * b * cos(c))"
                        ),
                        
                        new Function                                                        //zsiga hídépítő funckiója  -o-_-o-
                        (
                            name: "sin", 
                            param: new[]{"a", "b", "c", "d"}, 
                            body: "a*b*c*d"
                        ),

                        new Function                                                        //zsiga hídépítő funckiója  -o-_-o-
                        (
                            name: "sin",
                            param: new[]{ "x", "y"  },
                            body: "x*y"
                        )
                    },

                    new Dictionary<string, double>()
                    {
                        ["π"] = Math.PI,
                        ["e"] = Math.E,
                        ["ε"] = double.Epsilon
                    }
                );
            
            List<string> overloads = new()
            {
                "sin(0.5)",         //BUILT-IN
                "sin(3, -2)",       //CUSTOM 1
                "sin(1,2,3,4)"      //CUSTOM 2
            };


            foreach(var expression in overloads)
                Console.WriteLine(parser.Evaluate(expression));
        }


        private static void PerformanceTest(Evaluator parser, ref string expression, int runs)
        {
            Stopwatch sw = new();
            long[] lapTime = new long[runs];

            for (int i = 0; i < runs; i++)
            {
                sw.Start();
                parser.Evaluate(expression);
                sw.Stop();

                lapTime[i] = sw.ElapsedTicks;

                sw.Reset();

            }

            Console.WriteLine($"Showing laptime for {runs} runs:");
            foreach (long lap in lapTime)
                Console.WriteLine($"{lap} ticks");

            Console.WriteLine($"Average runtime ({runs} runs): {lapTime.Average() / 10000} ms \n" +
                              $"Runtime variance: {(lapTime.Max() - lapTime.Min())} ticks \n" +
                              $"Best: {lapTime.Min()} ticks \n" +
                              $"Worst: {lapTime.Max()} ticks ({lapTime.Max() / 10000} ms)");
        }
    }
}

