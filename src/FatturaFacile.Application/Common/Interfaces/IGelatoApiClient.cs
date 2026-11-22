// <copyright file="IGelatoApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Gelato;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Typed client of the artisanal gelato API (Mod 398, base path /api/v1/gelato). All calls need a connection.
/// </summary>
public interface IGelatoApiClient
{
    /// <summary>
    /// Lists the recipes of the tenant.
    /// </summary>
    /// <param name="locale">The language of the names (it, de, fr, en).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The recipes.</returns>
    public Task<IReadOnlyList<GelatoRecipeSummary>> GetRecipesAsync(string locale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the lines of a recipe, with the ingredient names.
    /// </summary>
    /// <param name="recipeId">The recipe identifier.</param>
    /// <param name="locale">The language of the names.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The lines in recipe order.</returns>
    public Task<IReadOnlyList<GelatoRecipeLineInfo>> GetRecipeLinesAsync(string recipeId, string locale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a weight typed in or read from the scale.
    /// </summary>
    /// <param name="ingredientId">The ingredient weighed.</param>
    /// <param name="grams">The weight in grams.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the call.</returns>
    public Task RecordScaleReadingAsync(string ingredientId, decimal grams, CancellationToken cancellationToken = default);
}
