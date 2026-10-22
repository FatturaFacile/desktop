// <copyright file="SimpleTools.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Extensions.AI;

#if NET
namespace FatturaFacile.Agents;

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
            (Func<string>)GetCurrentUtcTime, "GetCurrentUtcTime", "Returns the current UTC time in ISO 8601 format."),
        AIFunctionFactory.Create((Func<int, int, int>)SumNumbers, "SumNumbers", "Returns the sum of two integers."),
        AIFunctionFactory.Create((Func<string, string>)ReverseText, "ReverseText", "Reverses the supplied text."),
    };

    private static string GetCurrentUtcTime() => DateTime.UtcNow.ToString("O");

    private static int SumNumbers(
        int a,
        int b) => a + b;

    private static string ReverseText(
        string text) => string.Concat((text ?? string.Empty).Reverse());
}
#endif
