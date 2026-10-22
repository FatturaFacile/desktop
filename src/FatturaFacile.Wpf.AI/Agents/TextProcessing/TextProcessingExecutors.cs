// <copyright file="TextProcessingExecutors.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;

namespace FatturaFacile.Agents.TextProcessing;

/// <summary>
/// Workflow executor transforming input text into its invariant uppercase equivalent.
/// </summary>
public sealed class UppercaseExecutor : Executor<string, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UppercaseExecutor"/> class.
    /// </summary>
    public UppercaseExecutor() : base("UppercaseExecutor")
    {
    }

    /// <inheritdoc />
    public override ValueTask<string> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(message.ToUpperInvariant());
}

/// <summary>
/// Workflow executor transforming input text by reversing its character sequence.
/// </summary>
public sealed class ReverseTextExecutor : Executor<string, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReverseTextExecutor"/> class.
    /// </summary>
    public ReverseTextExecutor() : base("ReverseTextExecutor")
    {
    }

    /// <inheritdoc />
    public override ValueTask<string> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(string.Concat(message.Reverse()));
}
