// <copyright file="WorkflowEventContent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace FatturaFacile.Wpf.AI.Agents.WorkflowInfra;

/// <summary>
/// Encapsulates a workflow lifecycle event inside an <see cref="AIContent"/> message token.
/// </summary>
public sealed class WorkflowEventContent : AIContent
{

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowEventContent"/> class.
    /// </summary>
    /// <param name="workflowEvent">The underlying workflow event.</param>
    public WorkflowEventContent(WorkflowEvent workflowEvent)
    {
        WorkflowEvent     = workflowEvent ?? throw new ArgumentNullException(paramName: nameof(workflowEvent));
        RawRepresentation = workflowEvent;
    }

    /// <summary>
    /// Gets the workflow event instance.
    /// </summary>
    public WorkflowEvent WorkflowEvent { get; }

    /// <summary>
    /// Gets the type name of the workflow event.
    /// </summary>
    public string EventType => WorkflowEvent.GetType().Name;

    /// <inheritdoc />
    public override string ToString() => EventType;

}
