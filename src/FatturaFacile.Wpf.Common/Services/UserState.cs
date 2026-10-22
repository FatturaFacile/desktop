// <copyright file="UserState.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Common.Services;

public sealed record UserState(
    string LogicalState = "",
    string VisualState = "",
    string LayoutVersion = "1.0");
