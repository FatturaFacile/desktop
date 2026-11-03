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
    public void ProcessException(AIExceptionArgs exception)
    {
        if (exception.Exception is not AIDemoException)
        {
            exception.Exception = new Exception(message: "Something went wrong.", innerException: exception.Exception);
        }
    }

}

/// <summary>
/// Exception thrown during AI demo or assistant processing errors.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AIDemoException"/> class.
/// </remarks>
/// <param name="message">The exception message.</param>
public class AIDemoException(string message) : Exception(message: message);
