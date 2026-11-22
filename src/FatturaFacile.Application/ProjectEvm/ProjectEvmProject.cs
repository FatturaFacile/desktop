// <copyright file="ProjectEvmProject.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// A project of the picker.
/// </summary>
/// <param name="Id">The project identifier on the server.</param>
/// <param name="Number">The project number.</param>
/// <param name="Name">The project name.</param>
public sealed record ProjectEvmProject(string Id, string Number, string Name)
{
    /// <summary>
    /// Gets the label of the picker: the number and the name.
    /// </summary>
    public string Label => Number.Length == 0 ? Name : Number + " · " + Name;
}
