// <copyright file="IHasTenant.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Abstractions;

namespace FatturaFacile.Domain.Shared.Primitives;

/// <summary>
/// Legacy interface for backwards compatibility, inheriting from IMustHaveTenant.
/// </summary>
public interface IHasTenant : IMustHaveTenant;
