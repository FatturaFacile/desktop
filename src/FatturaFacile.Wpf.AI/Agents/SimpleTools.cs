// <copyright file="SimpleTools.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Extensions.AI;

#if NET
namespace FatturaFacile.Wpf.AI.Agents;

/// <summary>
/// Provides utility AI functions for time retrieval, integer arithmetic, and string manipulation.
/// </summary>
public static class SimpleTools
{

    /// <summary>
    /// Gets the list of available demonstration AI tools.
    /// </summary>
    public static List<AITool> Tools { get; } = new List<AITool>
                                                {
                                                    AIFunctionFactory.Create(
                                                                             method: (Func<string>)GetCurrentUtcTime,
                                                                             name: "GetCurrentUtcTime",
                                                                             description: "Returns the current UTC time in ISO 8601 format."),
                                                    AIFunctionFactory.Create(
                                                                             method: (Func<int, int, int>)SumNumbers,
                                                                             name: "SumNumbers",
                                                                             description: "Returns the sum of two integers."),
                                                    AIFunctionFactory.Create(
                                                                             method: (Func<string, string>)ReverseText,
                                                                             name: "ReverseText",
                                                                             description: "Reverses the supplied text."),
                                                };

    /// <summary>
    /// Gets the current UTC time in ISO 8601 round-trip format.
    /// </summary>
    /// <returns>A string containing the current ISO 8601 UTC timestamp.</returns>
    private static string GetCurrentUtcTime() => DateTime.UtcNow.ToString(format: "O");

    /// <summary>
    /// Computes the sum of two integers.
    /// </summary>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <returns>The sum of the two integers.</returns>
    private static int SumNumbers(
        int a,
        int b) => a + b;

    /// <summary>
    /// Reverses the characters of the provided string.
    /// </summary>
    /// <param name="text">The string to reverse.</param>
    /// <returns>The reversed string.</returns>
    private static string ReverseText(string text) => string.Concat(values: (text ?? string.Empty).Reverse());

}
#endif
