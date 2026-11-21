// <copyright file="DssWorkingCapital.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// Working capital metrics.
/// </summary>
/// <param name="DsoDays">Days sales outstanding.</param>
/// <param name="DioDays">Days inventory outstanding.</param>
/// <param name="DpoDays">Days payables outstanding.</param>
/// <param name="CccDays">Cash conversion cycle in days.</param>
public sealed record DssWorkingCapital(double DsoDays, double DioDays, double DpoDays, double CccDays);
