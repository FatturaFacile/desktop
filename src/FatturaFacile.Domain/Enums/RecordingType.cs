// <copyright file="RecordingType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the working hours tracking modality applied to an employee or assignment, balancing administrative overhead with labor compliance requirements.
/// </summary>
public enum RecordingType
{
    /// <summary>
    /// Precise tracking capturing individual start, end, and break timestamps throughout each working shift, required for hourly staff and overtime auditing.
    /// </summary>
    Detailed = 1,

    /// <summary>
    /// Simplified working hours recording (<c>vereinfachte Zeiterfassung</c>) under Swiss labor law (ArGV 1 Art. 73b), recording only total daily hours worked without itemized clock-in/out stamps.
    /// </summary>
    SimplifiedSwissVereinfacht = 2,
}
