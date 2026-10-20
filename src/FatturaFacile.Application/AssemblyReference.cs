using System.Reflection;

namespace FatturaFacile.Application;

/// <summary>
/// Represents assembly reference.
/// </summary>
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
