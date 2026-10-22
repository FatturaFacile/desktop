// <copyright file="WorkflowResponseProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using DevExpress.AIIntegration.Chat;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.InProc;
using Microsoft.Extensions.AI;

namespace FatturaFacile.Agents.WorkflowInfra;

/// <summary>
/// Specifies the start mode for workflow execution.
/// </summary>
public enum WorkflowResponseProviderStartMode
{
    Run,
    Open,
    Resume,
}

/// <summary>
/// Chat response provider wrapping an agentic workflow execution environment with streaming responses.
/// </summary>
/// <typeparam name="TInput">The input type expected by the workflow.</typeparam>
public sealed class WorkflowResponseProvider<TInput> : ChatResponseProvider
{
    private readonly Workflow workflow;
    private readonly Func<IEnumerable<ChatMessage>, TInput> inputFactory;
    private readonly InProcessExecutionEnvironment executionEnvironment;
    private readonly string sessionId;
    private readonly CheckpointManager checkpointManager;
    private readonly CheckpointInfo checkpointInfo;
    private readonly WorkflowResponseProviderStartMode startMode;
    private readonly bool emitEvents;
    private StreamingRun? activeStreamingRun;

    private readonly Dictionary<string, ExternalRequest> pendingRequests =
        new Dictionary<string, ExternalRequest>(StringComparer.Ordinal);

    public WorkflowResponseProvider(
        Workflow workflow,
        Func<IEnumerable<ChatMessage>, TInput> inputFactory)
        : this(workflow, inputFactory, WorkflowResponseProviderStartMode.Run)
    {
    }

    public WorkflowResponseProvider(
        Workflow workflow,
        Func<IEnumerable<ChatMessage>, TInput> inputFactory,
        WorkflowResponseProviderStartMode startMode,
        InProcessExecutionEnvironment executionEnvironment = null,
        string sessionId = null,
        CheckpointManager checkpointManager = null,
        bool emitEvents = true)
    {
        if (workflow == null)
        {
            throw new ArgumentNullException(nameof(workflow));
        }

        if (inputFactory == null)
        {
            throw new ArgumentNullException(nameof(inputFactory));
        }

        this.workflow = workflow;
        this.inputFactory = inputFactory;

        if (startMode == WorkflowResponseProviderStartMode.Resume)
        {
            throw new ArgumentException("Use a checkpoint-based constructor for resume mode.", nameof(startMode));
        }

        this.executionEnvironment = executionEnvironment ?? InProcessExecution.Default;
        this.sessionId = sessionId;
        this.checkpointManager = checkpointManager;
        this.startMode = startMode;
        this.emitEvents = emitEvents;
    }

    public WorkflowResponseProvider(
        Workflow workflow,
        CheckpointInfo checkpointInfo,
        CheckpointManager checkpointManager,
        InProcessExecutionEnvironment executionEnvironment = null,
        bool emitEvents = true)
    {
        if (workflow == null)
        {
            throw new ArgumentNullException(nameof(workflow));
        }

        if (checkpointInfo == null)
        {
            throw new ArgumentNullException(nameof(checkpointInfo));
        }

        if (checkpointManager == null)
        {
            throw new ArgumentNullException(nameof(checkpointManager));
        }

        this.workflow = workflow;
        this.checkpointInfo = checkpointInfo;
        this.checkpointManager = checkpointManager;

        this.executionEnvironment = executionEnvironment ?? InProcessExecution.Default;
        startMode = WorkflowResponseProviderStartMode.Resume;
        this.emitEvents = emitEvents;
    }

    protected override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseCoreAsync(
        IEnumerable<ChatMessage> messages,
        string conversationId,
        CancellationToken cancellationToken) => GetStreamingResponseAsync(messages, cancellationToken);

    protected override IAsyncEnumerable<ChatResponseUpdate> GetBufferedResponseCoreAsync(
        IEnumerable<ChatMessage> messages,
        string conversationId,
        CancellationToken cancellationToken)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var channel = Channel.CreateBounded<ChatResponseUpdate>(
            new BoundedChannelOptions(16)
            {
                SingleWriter = true,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });
        var produceTask = ProduceBufferedResponseAsync(channel.Writer, messages, linkedCts.Token);
        produceTask.ContinueWith(
            (task, state) => ((CancellationTokenSource)state).Dispose(), linkedCts, TaskScheduler.Default);
        return channel.Reader.ReadAllAsync(linkedCts.Token);
    }

    private async Task ProduceBufferedResponseAsync(
        ChannelWriter<ChatResponseUpdate> writer,
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        if (startMode == WorkflowResponseProviderStartMode.Open)
        {
            await ProduceStreamingResponseAsync(writer, messages, cancellationToken).ConfigureAwait(false);
            return;
        }

        Exception finalError = null;
        Run run = null;
        try
        {
            run = await StartBufferedRunAsync(messages, cancellationToken).ConfigureAwait(false);
            foreach (var workflowEvent in run.NewEvents)
            {
                foreach (var update in ConvertEvent(workflowEvent))
                {
                    await writer.WriteAsync(update, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            finalError = ex;
        }

        if (run != null)
        {
            try
            {
                await run.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception disposeError)
            {
                if (finalError == null)
                {
                    finalError = disposeError;
                }
            }
            finally
            {
                writer.Complete(finalError);
            }
        }
        else
        {
            writer.Complete(finalError);
        }

        if (finalError != null)
        {
            ExceptionDispatchInfo.Capture(finalError).Throw();
        }
    }

    private IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var channel = Channel.CreateBounded<ChatResponseUpdate>(
            new BoundedChannelOptions(16)
            {
                SingleWriter = true,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });
        var produceTask = ProduceStreamingResponseAsync(channel.Writer, messages, linkedCts.Token);
        produceTask.ContinueWith(
            (task, state) => ((CancellationTokenSource)state).Dispose(), linkedCts, TaskScheduler.Default);
        return channel.Reader.ReadAllAsync(linkedCts.Token);
    }

    private async Task ProduceStreamingResponseAsync(
        ChannelWriter<ChatResponseUpdate> writer,
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        Exception finalError = null;
        StreamingRun run = null;
        try
        {
            if (startMode == WorkflowResponseProviderStartMode.Open)
            {
                var openRun = await StartStreamingRunAsync(messages, cancellationToken).ConfigureAwait(false);
                var shouldKeepRun = false;

                var eventEnumerator = openRun.WatchStreamAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
                Exception enumerationError = null;
                try
                {
                    while (await eventEnumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        var workflowEvent = eventEnumerator.Current;
                        foreach (var update in ConvertEvent(workflowEvent))
                        {
                            await writer.WriteAsync(update, cancellationToken).ConfigureAwait(false);
                        }

                        if (workflowEvent is RequestInfoEvent)
                        {
                            shouldKeepRun = true;
                            break;
                        }

                        if (workflowEvent is WorkflowOutputEvent)
                        {
                            shouldKeepRun = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    enumerationError = ex;
                }

                await eventEnumerator.DisposeAsync().ConfigureAwait(false);
                if (enumerationError != null)
                {
                    ExceptionDispatchInfo.Capture(enumerationError).Throw();
                }

                if (!shouldKeepRun)
                {
                    await ResetActiveStreamingRunAsync().ConfigureAwait(false);
                }

                return;
            }

            run = await StartStreamingRunAsync(messages, cancellationToken).ConfigureAwait(false);
            var runEnumerator = run.WatchStreamAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
            Exception runEnumerationError = null;
            try
            {
                while (await runEnumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    var workflowEvent = runEnumerator.Current;
                    foreach (var update in ConvertEvent(workflowEvent))
                    {
                        await writer.WriteAsync(update, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                runEnumerationError = ex;
            }

            await runEnumerator.DisposeAsync().ConfigureAwait(false);
            if (runEnumerationError != null)
            {
                ExceptionDispatchInfo.Capture(runEnumerationError).Throw();
            }
        }
        catch (Exception ex)
        {
            finalError = ex;
        }

        if (run != null)
        {
            try
            {
                await run.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception disposeError)
            {
                if (finalError == null)
                {
                    finalError = disposeError;
                }
            }
            finally
            {
                writer.Complete(finalError);
            }
        }
        else
        {
            writer.Complete(finalError);
        }

        if (finalError != null)
        {
            ExceptionDispatchInfo.Capture(finalError).Throw();
        }
    }

    private async Task<StreamingRun> StartStreamingRunAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        var env = GetExecutionEnvironment();

        switch (startMode)
        {
            case WorkflowResponseProviderStartMode.Run:
                {
                    var streamingRun = await env.RunStreamingAsync(
                        workflow, GetInput(messages), sessionId, cancellationToken).ConfigureAwait(false);
                    await SendTurnTokenAsync(streamingRun).ConfigureAwait(false);
                    return streamingRun;
                }
            case WorkflowResponseProviderStartMode.Open:
                {
                    if (activeStreamingRun != null)
                    {
                        await ContinueOpenRunAsync(activeStreamingRun, messages).ConfigureAwait(false);
                        return activeStreamingRun;
                    }

                    var openedRun = await env.OpenStreamingAsync(workflow, sessionId, cancellationToken)
                        .ConfigureAwait(false);

                    activeStreamingRun = openedRun;

                    await SendInputAsync(openedRun, messages).ConfigureAwait(false);
                    await SendTurnTokenAsync(openedRun).ConfigureAwait(false);

                    return openedRun;
                }
            case WorkflowResponseProviderStartMode.Resume:
                {
                    var resumedRun = await env.ResumeStreamingAsync(workflow, checkpointInfo, cancellationToken)
                        .ConfigureAwait(false);

                    await SendTurnTokenAsync(resumedRun).ConfigureAwait(false);

                    return resumedRun;
                }
            default:
                throw new InvalidOperationException($"Unsupported workflow start mode '{startMode}'.");
        }
    }

    private async Task ContinueOpenRunAsync(
        StreamingRun run,
        IEnumerable<ChatMessage> messages)
    {
        if (await SendApprovalResponsesAsync(run, messages).ConfigureAwait(false))
        {
            return;
        }

        if (HasNonSystemMessages(messages))
        {
            await SendInputAsync(run, messages).ConfigureAwait(false);
            await SendTurnTokenAsync(run).ConfigureAwait(false);
        }
    }

    private async Task<Run> StartBufferedRunAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        var env = GetExecutionEnvironment();
        if (startMode == WorkflowResponseProviderStartMode.Run)
        {
            return await env.RunAsync(workflow, GetInput(messages), sessionId, cancellationToken).ConfigureAwait(false);
        }

        if (startMode == WorkflowResponseProviderStartMode.Resume)
        {
            return await env.ResumeAsync(workflow, checkpointInfo, cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException(
            $"Buffered execution is not supported for workflow start mode '{startMode}'.");
    }

    private InProcessExecutionEnvironment GetExecutionEnvironment()
    {
        var env = executionEnvironment ?? InProcessExecution.Default;
        if (checkpointManager != null)
        {
            env = env.WithCheckpointing(checkpointManager);
        }

        return env;
    }

    private TInput GetInput(
        IEnumerable<ChatMessage> messages)
    {
        if (inputFactory == null)
        {
            throw new InvalidOperationException("An input factory is required for this workflow execution mode.");
        }

        return inputFactory(messages);
    }

    private async Task SendInputAsync(
        StreamingRun run,
        IEnumerable<ChatMessage> messages) => await run.TrySendMessageAsync(GetInput(messages)).ConfigureAwait(false);

    private async Task<bool> SendApprovalResponsesAsync(
        StreamingRun run,
        IEnumerable<ChatMessage> messages)
    {
        var sent = false;
        foreach (var response in GetApprovalResponses(messages))
        {
            if (!pendingRequests.TryGetValue(response.RequestId, out var request))
            {
                continue;
            }

            pendingRequests.Remove(response.RequestId);
            await run.SendResponseAsync(request.CreateResponse(response)).ConfigureAwait(false);
            sent = true;
        }

        return sent;
    }

    private async Task SendTurnTokenAsync(
        StreamingRun run) => await run.TrySendMessageAsync(new TurnToken(emitEvents)).ConfigureAwait(false);

    private IEnumerable<ChatResponseUpdate> ConvertEvent(
        WorkflowEvent workflowEvent)
    {
        if (workflowEvent is AgentResponseUpdateEvent agentResponseUpdateEvent)
        {
            yield return CreateAgentUpdate(agentResponseUpdateEvent);
            yield break;
        }

        if (workflowEvent is RequestInfoEvent requestInfoEvent)
        {
            yield return CreateRequestUpdate(requestInfoEvent);
            yield break;
        }

        if (workflowEvent is ExecutorCompletedEvent executorCompletedEvent)
        {
            yield return CreateExecutorUpdate(executorCompletedEvent);
            yield break;
        }

        if (workflowEvent is WorkflowOutputEvent workflowOutputEvent)
        {
            var hasOutput = false;
            foreach (var update in ConvertOutputData(workflowOutputEvent.Data, workflowOutputEvent))
            {
                hasOutput = true;
                yield return update;
            }

            if (hasOutput)
            {
                yield break;
            }
        }

        yield return CreateWorkflowEventUpdate(workflowEvent);
    }

    private ChatResponseUpdate CreateRequestUpdate(
        RequestInfoEvent requestInfoEvent)
    {
        if (requestInfoEvent.Request.TryGetDataAs(out ToolApprovalRequestContent? approvalRequest) &&
            approvalRequest != null)
        {
            RegisterPendingRequest(requestInfoEvent.Request, approvalRequest.RequestId);
            return new ChatResponseUpdate
            {
                Contents = new AIContent[] { approvalRequest },
                RawRepresentation = requestInfoEvent,
                Role = ChatRole.Assistant,
            };
        }

        return CreateWorkflowEventUpdate(requestInfoEvent);
    }

    private void RegisterPendingRequest(
        ExternalRequest request,
        string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return;
        }

        pendingRequests[requestId] = request;
    }

    private static IEnumerable<ToolApprovalResponseContent> GetApprovalResponses(
        IEnumerable<ChatMessage> messages)
    {
        foreach (var message in messages)
        foreach (var response in message.Contents.OfType<ToolApprovalResponseContent>())
        {
            yield return response;
        }
    }

    private static bool HasNonSystemMessages(
        IEnumerable<ChatMessage> messages)
    {
        foreach (var message in messages)
        {
            if (message.Role != ChatRole.System)
            {
                return true;
            }
        }

        return false;
    }

    private async Task ResetActiveStreamingRunAsync()
    {
        if (activeStreamingRun == null)
        {
            return;
        }

        await activeStreamingRun.DisposeAsync().ConfigureAwait(false);
        activeStreamingRun = null;
        pendingRequests.Clear();
    }

    private static IEnumerable<ChatResponseUpdate> ConvertOutputData(
        object? data,
        WorkflowEvent workflowEvent)
    {
        if (data == null)
        {
            yield break;
        }

        if (data is ChatMessage message)
        {
            yield return CreateMessageUpdate(message, workflowEvent);
            yield break;
        }

        if (data is IEnumerable<ChatMessage> messages)
        {
            foreach (var messageItem in messages)
            {
                yield return CreateMessageUpdate(messageItem, workflowEvent);
            }

            yield break;
        }

        if (data is AIContent content)
        {
            yield return CreateContentUpdate(new AIContent[] { content }, workflowEvent);
            yield break;
        }

        if (data is IEnumerable<AIContent> contents)
        {
            yield return CreateContentUpdate(contents.ToList(), workflowEvent);
            yield break;
        }

        if (data is string text)
        {
            yield return CreateTextUpdate(text, workflowEvent);
            yield break;
        }
    }

    private static ChatResponseUpdate CreateAgentUpdate(
        AgentResponseUpdateEvent agentResponseUpdateEvent)
    {
        var source = agentResponseUpdateEvent.Update;
        return new ChatResponseUpdate
        {
            AuthorName = source.AuthorName,
            ContinuationToken = source.ContinuationToken,
            Contents = source.Contents != null && source.Contents.Count > 0
                ? new List<AIContent>(source.Contents)
                : CreateTextContents(source.Text),
            CreatedAt = source.CreatedAt,
            MessageId = source.MessageId,
            RawRepresentation = agentResponseUpdateEvent,
            ResponseId = source.ResponseId,
            Role = source.Role,
        };
    }

    private static ChatResponseUpdate CreateExecutorUpdate(
        ExecutorCompletedEvent executorCompletedEvent)
    {
        return new ChatResponseUpdate(
            null, executorCompletedEvent.Data is string text ? CreateTextContents(text) : null)
        {
            AuthorName = executorCompletedEvent.ExecutorId,
            MessageId = Guid.NewGuid().ToString(),
            RawRepresentation = executorCompletedEvent,
            Role = ChatRole.Tool,
        };
    }

    private static ChatResponseUpdate CreateMessageUpdate(
        ChatMessage message,
        WorkflowEvent workflowEvent)
    {
        return new ChatResponseUpdate
        {
            AuthorName = message.AuthorName,
            Contents = message.Contents != null && message.Contents.Count > 0
                ? new List<AIContent>(message.Contents)
                : CreateTextContents(message.Text),
            CreatedAt = message.CreatedAt,
            MessageId = message.MessageId,
            RawRepresentation = workflowEvent,
            Role = message.Role,
        };
    }

    private static ChatResponseUpdate CreateContentUpdate(
        IList<AIContent> contents,
        WorkflowEvent workflowEvent)
    {
        return new ChatResponseUpdate
        {
            Contents = contents,
            RawRepresentation = workflowEvent,
            Role = ChatRole.Assistant,
        };
    }

    private static ChatResponseUpdate CreateTextUpdate(
        string text,
        WorkflowEvent workflowEvent)
    {
        return new ChatResponseUpdate
        {
            Contents = CreateTextContents(text),
            RawRepresentation = workflowEvent,
            Role = ChatRole.Assistant,
        };
    }

    private static ChatResponseUpdate CreateWorkflowEventUpdate(
        WorkflowEvent workflowEvent)
    {
        return new ChatResponseUpdate
        {
            Contents = new AIContent[] { new WorkflowEventContent(workflowEvent) },
            RawRepresentation = workflowEvent,
            Role = ChatRole.Assistant,
        };
    }

    private static IList<AIContent> CreateTextContents(
        string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<AIContent>();
        }

        return new AIContent[] { new TextContent(text) };
    }
}
