// <copyright file="AIExceptionHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using DevExpress.AIIntegration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Modules.CodeExamples; 
public class AIExceptionHandler : IAIExceptionHandler {

    readonly ILogger _logger;
    public AIExceptionHandler(ILogger logger) {
        _logger = logger;
    }
    public void ProcessException(AIExceptionArgs args) {
        _logger.LogInformation("An error occurred: {ErrorMessage}", args.Exception.Message);
        args.Response = new ChatResponse(new ChatMessage(ChatRole.Assistant, "Something went wrong. Please try again later."));
        args.Exception = new Exception("Something went wrong. Please try again later.", args.Exception);
    }
}
