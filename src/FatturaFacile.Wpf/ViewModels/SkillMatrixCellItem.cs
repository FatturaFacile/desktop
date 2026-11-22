// <copyright file="SkillMatrixCellItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One skill of an employee in the skill matrix, ready to show.
/// </summary>
/// <param name="Text">The skill and its proficiency.</param>
/// <param name="StateName">The state: Valid, Expired, CertificateMissing or CertificateExpired.</param>
/// <param name="ToolTip">The category and the state.</param>
internal sealed record SkillMatrixCellItem(string Text, string StateName, string ToolTip);
