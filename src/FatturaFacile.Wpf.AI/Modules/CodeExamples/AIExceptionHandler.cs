// <copyright file="AIExceptionHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Wpf.AI.Modules.CodeExamples;

/// <summary>
/// Custom AI exception handler demonstrating logging and fallback user message generation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AIExceptionHandler"/> class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
public class AIExceptionHandler(ILogger logger) : IAIExceptionHandler
{

    /// <summary>
    /// The logger used for logging caught AI errors.
    /// </summary>
    private readonly ILogger _logger = logger;

    /// <inheritdoc />
    public void ProcessException(AIExceptionArgs exception)
    {
        _logger.LogInformation(message: "An error occurred: {ErrorMessage}", args: exception.Exception.Message);

        exception.Response = new ChatResponse(
                                         message: new ChatMessage(
                                                                  role: ChatRole.Assistant,
                                                                  content: "Something went wrong. Please try again later."));

        exception.Exception = new Exception(message: "Something went wrong. Please try again later.", innerException: exception.Exception);
    }

}
