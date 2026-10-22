// <copyright file="AIDemoException.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration;

namespace FatturaFacile.Wpf.AI;

/// <summary>
/// Custom exception handler for DevExpress AI operations.
/// </summary>
public class AIExceptionHandler : IAIExceptionHandler
{

    /// <inheritdoc />
    public void ProcessException(AIExceptionArgs args)
    {
        if (!(args.Exception is AIDemoException))
        {
            args.Exception = new Exception(message: "Something went wrong.", innerException: args.Exception);
        }
    }

}

/// <summary>
/// Exception thrown during AI demo or assistant processing errors.
/// </summary>
public class AIDemoException : Exception
{

    /// <summary>
    /// Initializes a new instance of the <see cref="AIDemoException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public AIDemoException(string message) : base(message: message)
    {
    }

}
