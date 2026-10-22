// <copyright file="AIExceptionHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using DevExpress.AIIntegration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Modules.CodeExamples;

/// <summary>
/// Custom AI exception handler demonstrating logging and fallback user message generation.
/// </summary>
public class AIExceptionHandler : IAIExceptionHandler
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIExceptionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public AIExceptionHandler(
        ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void ProcessException(
        AIExceptionArgs args)
    {
        _logger.LogInformation("An error occurred: {ErrorMessage}", args.Exception.Message);
        args.Response = new ChatResponse(
            new ChatMessage(ChatRole.Assistant, "Something went wrong. Please try again later."));
        args.Exception = new Exception("Something went wrong. Please try again later.", args.Exception);
    }
}
