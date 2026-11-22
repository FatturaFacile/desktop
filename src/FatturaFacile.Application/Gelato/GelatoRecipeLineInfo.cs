// <copyright file="GelatoRecipeLineInfo.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Gelato;

/// <summary>
/// A line of a gelato recipe: an ingredient with its name and grams.
/// </summary>
/// <param name="IngredientId">The ingredient identifier.</param>
/// <param name="Name">The ingredient name in the language asked for.</param>
/// <param name="Grams">The grams in the stored batch.</param>
public sealed record GelatoRecipeLineInfo(string IngredientId, string Name, decimal Grams);
