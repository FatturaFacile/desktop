// <copyright file="ShakespeareanTools.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace FatturaFacile.Wpf.AI.Agents.Shakespearean;

/// <summary>
/// Provides AI function tools for accessing authentic Shakespearean and Elizabethan vocabulary.
/// </summary>
public static class ShakespeareanTools
{

    /// <summary>
    /// Creates and returns an <see cref="AIFunction"/> tool that provides Shakespearean vocabulary definitions.
    /// </summary>
    /// <returns>A configured <see cref="AIFunction"/>.</returns>
    public static AIFunction GetDictionaryTool() => AIFunctionFactory.Create(method: (Func<string>)GetShakespeareanDictionary);

    /// <summary>
    /// Returns a curated dictionary of authentic Shakespearean and Elizabethan English words together with modern meanings.
    /// </summary>
    /// <returns>A formatted markdown list of archaic words and definitions.</returns>
    [Description(
                    description: "Returns a curated dictionary of authentic Shakespearean and Elizabethan English words " +
                                 "together with their modern meanings. Call this tool to enrich a rewritten poem with "   +
                                 "period-accurate vocabulary before producing the final output.")]
    public static string GetShakespeareanDictionary()
    {
        var words = new Dictionary<string, string>
                    {
                        [key: "thee"]      = "you (object form, e.g. 'I love thee')",
                        [key: "thou"]      = "you (subject form, e.g. 'thou art brave')",
                        [key: "thy"]       = "your (e.g. 'thy name')",
                        [key: "thine"]     = "yours / your (before a vowel, e.g. 'thine eyes')",
                        [key: "hath"]      = "has (third-person singular, e.g. 'he hath spoken')",
                        [key: "doth"]      = "does (third-person singular, e.g. 'she doth protest')",
                        [key: "art"]       = "are (e.g. 'thou art my light')",
                        [key: "wherefore"] = "why (e.g. 'wherefore art thou Romeo?' means 'why are you Romeo?')",
                        [key: "ere"]       = "before (poetic, e.g. 'ere the sun sets')",
                        [key: "methinks"]  = "it seems to me (e.g. 'methinks the lady doth protest too much')",
                        [key: "forsooth"]  = "indeed / in truth (e.g. 'forsooth, it is a wondrous sight')",
                        [key: "perchance"] = "perhaps / possibly (e.g. 'perchance to dream')",
                        [key: "alas"]      = "an exclamation of grief or pity (e.g. 'alas, poor Yorick')",
                        [key: "hence"]     = "from this place / from now (e.g. 'get thee hence')",
                        [key: "hark"]      = "listen / pay attention (e.g. 'hark! what light through yonder window breaks')",
                    };

        return string.Join(separator: "\n", values: words.Select(selector: kv => $"- **{kv.Key}**: {kv.Value}"));
    }

}
