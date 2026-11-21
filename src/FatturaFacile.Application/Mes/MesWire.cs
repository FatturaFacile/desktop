// <copyright file="MesWire.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// Conversion between the enum members of the MES module and the snake_case names of the web-api.
/// </summary>
public static class MesWire
{
    /// <summary>
    /// Gets the wire name of an enum member, for example MachineBreakdown becomes machine_breakdown.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The member.</param>
    /// <returns>The snake_case name.</returns>
    public static string ToWire<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        var name = value.ToString();
        StringBuilder builder = new(name.Length + 4);
        for (var index = 0; index < name.Length; index++)
        {
            var current = name[index];
            if (char.IsUpper(current) && index > 0)
            {
                _ = builder.Append('_');
            }

            _ = builder.Append(char.ToLowerInvariant(current));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Parses a wire name into an enum member.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="wire">The snake_case name.</param>
    /// <param name="value">The member.</param>
    /// <returns><see langword="true"/> when the name is a member of the enum.</returns>
    public static bool TryParse<TEnum>(string? wire, out TEnum value)
        where TEnum : struct, Enum
    {
        foreach (var candidate in Enum.GetValues<TEnum>())
        {
            if (string.Equals(ToWire(candidate), wire, StringComparison.Ordinal))
            {
                value = candidate;
                return true;
            }
        }

        value = default;
        return false;
    }
}
