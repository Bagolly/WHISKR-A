using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                            name: "zsormula", 
                            param: new[]{ "x1", "x2", "y1", "y2" , "z1" , "z2" }, 
                            body: "((z1 ^ 2) + (z2^2)) * (x1 * y1 + x2 * y2)"
                        )       
                    },

                    new Dictionary<string, double>()
                    {
                        ["half"] = 0.5,
                        ["year"] = 2022,
                        ["first"] = 1,
                        ["second"] = 2,
                        ["third"] = 3,
                        ["a"] = 97
                    }
                );


            string expression = "zsormula(1,2,3,4,5,6)";
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

