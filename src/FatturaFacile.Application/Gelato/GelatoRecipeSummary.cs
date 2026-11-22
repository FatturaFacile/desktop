// <copyright file="GelatoRecipeSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Gelato;

/// <summary>
/// A gelato recipe of the ERP (Mod 398) with its calculated indexes per 1,000 g.
/// </summary>
/// <param name="Id">The recipe identifier.</param>
/// <param name="Code">The recipe code.</param>
/// <param name="Name">The name in the language asked for.</param>
/// <param name="BatchWeightGrams">The stored batch weight.</param>
/// <param name="Pod">The sweetening power.</param>
/// <param name="Pac">The anti-freezing power.</param>
/// <param name="FoodCostPerKg">The cost per kg.</param>
public sealed record GelatoRecipeSummary(string Id, string Code, string Name, decimal BatchWeightGrams, decimal Pod, decimal Pac, decimal FoodCostPerKg);

/// <summary>
/// A line of a gelato recipe: an ingredient with its name and grams.
/// </summary>
/// <param name="IngredientId">The ingredient identifier.</param>
/// <param name="Name">The ingredient name in the language asked for.</param>
/// <param name="Grams">The grams in the stored batch.</param>
public sealed record GelatoRecipeLineInfo(string IngredientId, string Name, decimal Grams);
