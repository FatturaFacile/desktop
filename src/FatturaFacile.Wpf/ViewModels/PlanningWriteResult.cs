// <copyright file="PlanningWriteResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The result of a write of a resource planning view, ready to show.
/// </summary>
/// <typeparam name="T">The value the server returns.</typeparam>
/// <param name="IsSuccess">Whether the server accepted the write.</param>
/// <param name="Value">The value of the server, when the write was accepted.</param>
/// <param name="Message">The text of the refusal or of the failure; empty when the write was accepted.</param>
internal sealed record PlanningWriteResult<T>(bool IsSuccess, T? Value, string Message);
