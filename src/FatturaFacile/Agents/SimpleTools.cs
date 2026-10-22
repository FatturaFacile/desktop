using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.AI;

#if NET
namespace AIDemo.Agents {
    public static class SimpleTools {
        public static List<AITool> Tools { get; } = new List<AITool> {
            AIFunctionFactory.Create((Func<string>)GetCurrentUtcTime, "GetCurrentUtcTime", "Returns the current UTC time in ISO 8601 format."),
            AIFunctionFactory.Create((Func<int, int, int>)SumNumbers, "SumNumbers", "Returns the sum of two integers."),
            AIFunctionFactory.Create((Func<string, string>)ReverseText, "ReverseText", "Reverses the supplied text."),
        };

        static string GetCurrentUtcTime() => DateTime.UtcNow.ToString("O");

        static int SumNumbers(int a, int b) => a + b;

        static string ReverseText(string text) => string.Concat((text ?? string.Empty).Reverse());
    }
}
#endif
