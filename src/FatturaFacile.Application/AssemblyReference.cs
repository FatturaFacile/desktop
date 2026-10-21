// <copyright file="AssemblyReference.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Reflection;

namespace FatturaFacile.Application;

/// <summary>
/// Marker type used to obtain a reference to the Application assembly.
/// </summary>
/// <remarks>
/// Commonly used for assembly scanning scenarios such as:
/// <list type="bullet">
/// <item><description>MediatR handler registration</description></item>
/// <item><description>FluentValidation validator discovery</description></item>
/// <item><description>AutoMapper profile registration</description></item>
/// </list>
/// </remarks>
public static class AssemblyReference
{
    /// <summary>
    /// Gets the Application assembly.
    /// </summary>
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
