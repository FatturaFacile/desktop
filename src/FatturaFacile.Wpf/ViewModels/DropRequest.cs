// <copyright file="DropRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// What a drag and drop hands to the view model: the item that was dragged and the data context of the element it was dropped on.
/// </summary>
/// <param name="Payload">The dragged item.</param>
/// <param name="Target">The data context of the drop target.</param>
internal sealed record DropRequest(object Payload, object? Target);
