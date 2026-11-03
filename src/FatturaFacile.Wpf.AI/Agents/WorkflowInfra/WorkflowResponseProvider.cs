// <copyright file="WorkflowResponseProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using DevExpress.AIIntegration.Chat;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.InProc;
using Microsoft.Extensions.AI;

namespace FatturaFacile.Wpf.AI.Agents.WorkflowInfra;

/// <summary>
/// Specifies the start mode for workflow execution.
/// </summary>
public enum WorkflowResponseProviderStartMode
{

    /// <inheritdoc/>
    Run = 0,

    /// <inheritdoc/>
    Open = 1,

    /// <inheritdoc/>
    Resume = 2,

}

/// <summary>
/// Chat response provider wrapping an agentic workflow execution environment with streaming responses.
/// </summary>
/// <typeparam name="TInput">The input type expected by the workflow.</typeparam>
public sealed class WorkflowResponseProvider<TInput> : ChatResponseProvider
{

    /// <summary>
    /// The underlying workflow instance being executed.
    /// </summary>
    private readonly Workflow workflow;

    /// <summary>
    /// Factory delegate transforming incoming chat messages into workflow input.
    /// </summary>
    private readonly Func<IEnumerable<ChatMessage>, TInput> inputFactory;

    /// <summary>
    /// In-process execution environment hosting the workflow execution.
    /// </summary>
    private readonly InProcessExecutionEnvironment executionEnvironment;

    /// <summary>
    /// Unique session identifier for tracking conversation workflow runs.
    /// </summary>
    private readonly string sessionId;

    /// <summary>
    /// Checkpoint manager handling workflow state persistence and resumption.
    /// </summary>
    private readonly CheckpointManager checkpointManager;

    /// <summary>
    /// Checkpoint information used when resuming workflow execution.
    /// </summary>
    private readonly CheckpointInfo checkpointInfo;

    /// <summary>
    /// The start mode controlling how the workflow execution is initiated.
    /// </summary>
    private readonly WorkflowResponseProviderStartMode startMode;

    /// <summary>
    /// Indicates whether workflow intermediate events should be emitted to the response stream.
    /// </summary>
    private readonly bool emitEvents;

    /// <summary>
    /// Tracks pending external requests such as human-in-the-loop tool approvals.
    /// </summary>
    private readonly Dictionary<string, ExternalRequest> pendingRequests =
        [with(comparer: StringComparer.Ordinal)];

    /// <summary>
    /// The currently active streaming run for interactive workflows.
    /// </summary>
    private StreamingRun activeStreamingRun;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowResponseProvider{TInput}"/> class with default Run start mode.
    /// </summary>
    /// <param name="workflow">The workflow topology to execute.</param>
    /// <param name="inputFactory">Factory function constructing input objects from the chat history.</param>
    public WorkflowResponseProvider(
        Workflow workflow,
        Func<IEnumerable<ChatMessage>, TInput> inputFactory)
        : this(workflow: workflow, inputFactory: inputFactory, startMode: WorkflowResponseProviderStartMode.Run)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowResponseProvider{TInput}"/> class with execution configuration.
    /// </summary>
    /// <param name="workflow">The workflow topology to execute.</param>
    /// <param name="inputFactory">Factory function constructing input objects from the chat history.</param>
    /// <param name="startMode">The workflow lifecycle execution mode.</param>
    /// <param name="executionEnvironment">The in-process execution environment, or null for default.</param>
    /// <param name="sessionId">Unique session identifier for tracking state across invocations.</param>
    /// <param name="checkpointManager">Optional checkpoint manager for saving state checkpoints.</param>
    /// <param name="emitEvents">Whether workflow progress events are yielded into the response stream.</param>
    public WorkflowResponseProvider(
        Workflow workflow,
        Func<IEnumerable<ChatMessage>, TInput> inputFactory,
        WorkflowResponseProviderStartMode startMode,
        InProcessExecutionEnvironment executionEnvironment = null,
        string sessionId = null,
        CheckpointManager checkpointManager = null,
        bool emitEvents = true)
    {
        this.workflow = workflow ?? throw new ArgumentNullException(paramName: nameof(workflow));
        this.inputFactory = inputFactory ?? throw new ArgumentNullException(paramName: nameof(inputFactory));

        if (startMode == WorkflowResponseProviderStartMode.Resume)
        {
            throw new ArgumentException(message: "Use a checkpoint-based constructor for resume mode.", paramName: nameof(startMode));
        }

        this.executionEnvironment = executionEnvironment ?? InProcessExecution.Default;
        this.sessionId = sessionId;
        this.checkpointManager = checkpointManager;
        this.startMode = startMode;
        this.emitEvents = emitEvents;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowResponseProvider{TInput}"/> class resuming from a checkpoint.
    /// </summary>
    /// <param name="workflow">The workflow topology to resume.</param>
    /// <param name="checkpointInfo">The checkpoint metadata from which execution resumes.</param>
    /// <param name="checkpointManager">The checkpoint manager handling saved workflow states.</param>
    /// <param name="executionEnvironment">The in-process execution environment, or null for default.</param>
    /// <param name="emitEvents">Whether workflow progress events are yielded into the response stream.</param>
    public WorkflowResponseProvider(
        Workflow workflow,
        CheckpointInfo checkpointInfo,
        CheckpointManager checkpointManager,
        InProcessExecutionEnvironment executionEnvironment = null,
        bool emitEvents = true)
    {
        this.workflow = workflow ?? throw new ArgumentNullException(paramName: nameof(workflow));
        this.checkpointInfo = checkpointInfo ?? throw new ArgumentNullException(paramName: nameof(checkpointInfo));
        this.checkpointManager = checkpointManager ?? throw new ArgumentNullException(paramName: nameof(checkpointManager));

        this.executionEnvironment = executionEnvironment ?? InProcessExecution.Default;
        startMode = WorkflowResponseProviderStartMode.Resume;
        this.emitEvents = emitEvents;
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseCoreAsync(
        IEnumerable<ChatMessage> messages,
        string conversationId,
        CancellationToken cancellationToken) => GetStreamingResponseAsync(messages: messages, cancellationToken: cancellationToken);

    /// <inheritdoc />
    protected override IAsyncEnumerable<ChatResponseUpdate> GetBufferedResponseCoreAsync(
        IEnumerable<ChatMessage> messages,
        string conversationId,
        CancellationToken cancellationToken)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token: cancellationToken);

        var channel = Channel.CreateBounded<ChatResponseUpdate>(
                                                                options: new BoundedChannelOptions(capacity: 16)
                                                                {
                                                                    SingleWriter = true,
                                                                    SingleReader = true,
                                                                    FullMode = BoundedChannelFullMode.Wait,
                                                                });

        var produceTask = ProduceBufferedResponseAsync(writer: channel.Writer, messages: messages, cancellationToken: linkedCts.Token);

        _ = produceTask.ContinueWith(
                                     continuationAction: (task, state) => ((CancellationTokenSource)state).Dispose(),
                                     state: linkedCts,
                                     scheduler: TaskScheduler.Default);

        return channel.Reader.ReadAllAsync(cancellationToken: linkedCts.Token);
    }

    /// <summary>
    /// Extracts tool approval responses from chat message contents.
    /// </summary>
    /// <param name="messages">The chat messages to inspect.</param>
    /// <returns>An enumeration of tool approval response contents.</returns>
    private static IEnumerable<ToolApprovalResponseContent> GetApprovalResponses(IEnumerable<ChatMessage> messages)
    {
        foreach (var message in messages)
        {
            foreach (var response in message.Contents.OfType<ToolApprovalResponseContent>())
            {
                yield return response;
            }
        }
    }

    /// <summary>
    /// Executes the workflow in buffered mode and completes the response channel writer.
    /// </summary>
    /// <param name="writer">The response channel writer.</param>
    /// <param name="messages">The incoming chat messages.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProduceBufferedResponseAsync(
        ChannelWriter<ChatResponseUpdate> writer,
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        if (startMode == WorkflowResponseProviderStartMode.Open)
        {
            await ProduceStreamingResponseAsync(writer: writer, messages: messages, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            return;
        }

        Exception finalError = null;
        Run run = null;

        try
        {
            run = await StartBufferedRunAsync(messages: messages, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            foreach (var workflowEvent in run.NewEvents)
            {
                foreach (var update in ConvertEvent(workflowEvent: workflowEvent))
                {
                    await writer.WriteAsync(item: update, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
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
                await run.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception disposeError)
            {
                finalError ??= disposeError;
            }
            finally
            {
                writer.Complete(error: finalError);
            }
        }
        else
        {
            writer.Complete(error: finalError);
        }

        if (finalError != null)
        {
            ExceptionDispatchInfo.Capture(source: finalError).Throw();
        }
    }

    /// <summary>
    /// Streams chat response updates by executing the workflow and yielding intermediate updates.
    /// </summary>
    /// <param name="messages">The incoming chat messages.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>An async enumerable of chat response updates.</returns>
    private IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token: cancellationToken);

        var channel = Channel.CreateBounded<ChatResponseUpdate>(
                                                                options: new BoundedChannelOptions(capacity: 16)
                                                                {
                                                                    SingleWriter = true,
                                                                    SingleReader = true,
                                                                    FullMode = BoundedChannelFullMode.Wait,
                                                                });

        var produceTask = ProduceStreamingResponseAsync(writer: channel.Writer, messages: messages, cancellationToken: linkedCts.Token);

        _ = produceTask.ContinueWith(
                                     continuationAction: (task, state) => ((CancellationTokenSource)state).Dispose(),
                                     state: linkedCts,
                                     scheduler: TaskScheduler.Default);

        return channel.Reader.ReadAllAsync(cancellationToken: linkedCts.Token);
    }

    /// <summary>
    /// Produces streaming updates from the workflow execution into the response channel writer.
    /// </summary>
    /// <param name="writer">The channel writer for response updates.</param>
    /// <param name="messages">The incoming chat messages.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
                var openRun = await StartStreamingRunAsync(messages: messages, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                var shouldKeepRun = false;

                var eventEnumerator = openRun.WatchStreamAsync(cancellationToken: cancellationToken).GetAsyncEnumerator(cancellationToken: cancellationToken);
                Exception enumerationError = null;

                try
                {
                    while (await eventEnumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
                    {
                        var workflowEvent = eventEnumerator.Current;

                        foreach (var update in ConvertEvent(workflowEvent: workflowEvent))
                        {
                            await writer.WriteAsync(item: update, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
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

                await eventEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);

                if (enumerationError != null)
                {
                    ExceptionDispatchInfo.Capture(source: enumerationError).Throw();
                }

                if (!shouldKeepRun)
                {
                    await ResetActiveStreamingRunAsync().ConfigureAwait(continueOnCapturedContext: false);
                }

                return;
            }

            run = await StartStreamingRunAsync(messages: messages, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            var runEnumerator = run.WatchStreamAsync(cancellationToken: cancellationToken).GetAsyncEnumerator(cancellationToken: cancellationToken);
            Exception runEnumerationError = null;

            try
            {
                while (await runEnumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
                {
                    var workflowEvent = runEnumerator.Current;

                    foreach (var update in ConvertEvent(workflowEvent: workflowEvent))
                    {
                        await writer.WriteAsync(item: update, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                    }
                }
            }
            catch (Exception ex)
            {
                runEnumerationError = ex;
            }

            await runEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);

            if (runEnumerationError != null)
            {
                ExceptionDispatchInfo.Capture(source: runEnumerationError).Throw();
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
                await run.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception disposeError)
            {
                finalError ??= disposeError;
            }
            finally
            {
                writer.Complete(error: finalError);
            }
        }
        else
        {
            writer.Complete(error: finalError);
        }

        if (finalError != null)
        {
            ExceptionDispatchInfo.Capture(source: finalError).Throw();
        }
    }

    /// <summary>
    /// Starts or resumes a streaming workflow run.
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>A task returning the started <see cref="StreamingRun"/>.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<StreamingRun> StartStreamingRunAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        var env = GetExecutionEnvironment();

        switch (startMode)
        {
            case WorkflowResponseProviderStartMode.Run:
                var streamingRun = await env.RunStreamingAsync(
                                                                   workflow: workflow,
                                                                   input: GetInput(messages: messages),
                                                                   sessionId: sessionId,
                                                                   cancellationToken: cancellationToken)
                                                .ConfigureAwait(continueOnCapturedContext: false);

                await SendTurnTokenAsync(run: streamingRun).ConfigureAwait(continueOnCapturedContext: false);

                return streamingRun;

            case WorkflowResponseProviderStartMode.Open:
                if (activeStreamingRun != null)
                {
                    await ContinueOpenRunAsync(run: activeStreamingRun, messages: messages).ConfigureAwait(continueOnCapturedContext: false);

                    return activeStreamingRun;
                }

                var openedRun = await env.OpenStreamingAsync(workflow: workflow, sessionId: sessionId, cancellationToken: cancellationToken)
                                         .ConfigureAwait(continueOnCapturedContext: false);

                activeStreamingRun = openedRun;

                await SendInputAsync(run: openedRun, messages: messages).ConfigureAwait(continueOnCapturedContext: false);
                await SendTurnTokenAsync(run: openedRun).ConfigureAwait(continueOnCapturedContext: false);

                return openedRun;

            case WorkflowResponseProviderStartMode.Resume:
                {
                    var resumedRun = await env.ResumeStreamingAsync(workflow: workflow, fromCheckpoint: checkpointInfo, cancellationToken: cancellationToken)
                                              .ConfigureAwait(continueOnCapturedContext: false);

                    await SendTurnTokenAsync(run: resumedRun).ConfigureAwait(continueOnCapturedContext: false);

                    return resumedRun;
                }

            default:
                throw new InvalidOperationException(message: $"Unsupported workflow start mode '{startMode}'.");
        }
    }

    /// <summary>
    /// Continues an open streaming run by submitting input or approval responses.
    /// </summary>
    /// <param name="run">The active streaming run.</param>
    /// <param name="messages">The incoming chat messages.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ContinueOpenRunAsync(
        StreamingRun run,
        IEnumerable<ChatMessage> messages)
    {
        if (await SendApprovalResponsesAsync(run: run, messages: messages).ConfigureAwait(continueOnCapturedContext: false))
        {
            return;
        }

        if (HasNonSystemMessages(messages: messages))
        {
            await SendInputAsync(run: run, messages: messages).ConfigureAwait(continueOnCapturedContext: false);
            await SendTurnTokenAsync(run: run).ConfigureAwait(continueOnCapturedContext: false);
        }
    }

    /// <summary>
    /// Starts a non-streaming buffered workflow run.
    /// </summary>
    /// <param name="messages">The incoming chat messages.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>A task returning the initiated <see cref="Run"/>.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<Run> StartBufferedRunAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        var env = GetExecutionEnvironment();

        if (startMode == WorkflowResponseProviderStartMode.Run)
        {
            return await env.RunAsync(workflow: workflow, input: GetInput(messages: messages), sessionId: sessionId, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }
        else
        {
            return startMode == WorkflowResponseProviderStartMode.Resume
                ? await env.ResumeAsync(workflow: workflow, fromCheckpoint: checkpointInfo, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false)
                : throw new InvalidOperationException(message: $"Buffered execution is not supported for workflow start mode '{startMode}'.");
        }
    }

    /// <summary>
    /// Resolves the in-process execution environment for the workflow.
    /// </summary>
    /// <returns>The execution environment instance.</returns>
    private InProcessExecutionEnvironment GetExecutionEnvironment()
    {
        var env = executionEnvironment ?? InProcessExecution.Default;

        if (checkpointManager != null)
        {
            env = env.WithCheckpointing(checkpointManager: checkpointManager);
        }

        return env;
    }

    /// <summary>
    /// Converts chat messages into typed workflow input.
    /// </summary>
    /// <param name="messages">The chat messages.</param>
    /// <returns>The constructed workflow input payload.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private TInput GetInput(IEnumerable<ChatMessage> messages)
    {
        return inputFactory == null
            ? throw new InvalidOperationException(message: "An input factory is required for this workflow execution mode.")
            : inputFactory(arg: messages);
    }

    /// <summary>
    /// Sends chat input payload to an active streaming run.
    /// </summary>
    /// <param name="run">The active streaming run.</param>
    /// <param name="messages">The incoming chat messages.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SendInputAsync(
        StreamingRun run,
        IEnumerable<ChatMessage> messages) => await run.TrySendMessageAsync(message: GetInput(messages: messages)).ConfigureAwait(continueOnCapturedContext: false);

    /// <summary>
    /// Submits human tool approval responses back to pending workflow requests.
    /// </summary>
    /// <param name="run">The active streaming run.</param>
    /// <param name="messages">The chat messages containing approval responses.</param>
    /// <returns>A task returning <see langword="true"/> if approvals were sent; otherwise <c>false</c>.</returns>
    private async Task<bool> SendApprovalResponsesAsync(
        StreamingRun run,
        IEnumerable<ChatMessage> messages)
    {
        var sent = false;

        foreach (var response in GetApprovalResponses(messages: messages))
        {
            if (!pendingRequests.TryGetValue(key: response.RequestId, value: out var request))
            {
                continue;
            }

            _ = pendingRequests.Remove(key: response.RequestId);
            await run.SendResponseAsync(response: request.CreateResponse(data: response)).ConfigureAwait(continueOnCapturedContext: false);
            sent = true;
        }

        return sent;
    }

    private async Task SendTurnTokenAsync(StreamingRun run) =>
        await run.TrySendMessageAsync(message: new TurnToken(emitEvents: emitEvents)).ConfigureAwait(continueOnCapturedContext: false);

    /// <summary>
    /// Converts a workflow event into one or more chat response updates.
    /// </summary>
    /// <param name="workflowEvent">The workflow event to translate.</param>
    /// <returns>An enumeration of chat response updates.</returns>
    private IEnumerable<ChatResponseUpdate> ConvertEvent(WorkflowEvent workflowEvent)
    {
        if (workflowEvent is AgentResponseUpdateEvent agentResponseUpdateEvent)
        {
            yield return CreateAgentUpdate(agentResponseUpdateEvent: agentResponseUpdateEvent);

            yield break;
        }

        if (workflowEvent is RequestInfoEvent requestInfoEvent)
        {
            yield return CreateRequestUpdate(requestInfoEvent: requestInfoEvent);

            yield break;
        }

        if (workflowEvent is ExecutorCompletedEvent executorCompletedEvent)
        {
            yield return CreateExecutorUpdate(executorCompletedEvent: executorCompletedEvent);

            yield break;
        }

        if (workflowEvent is WorkflowOutputEvent workflowOutputEvent)
        {
            var hasOutput = false;

            foreach (var update in ConvertOutputData(data: workflowOutputEvent.Data, workflowEvent: workflowOutputEvent))
            {
                hasOutput = true;

                yield return update;
            }

            if (hasOutput)
            {
                yield break;
            }
        }

        yield return CreateWorkflowEventUpdate(workflowEvent: workflowEvent);
    }

    /// <summary>
    /// Creates a chat response update corresponding to a workflow external request info event.
    /// </summary>
    /// <param name="requestInfoEvent">The request event.</param>
    /// <returns>The chat response update.</returns>
    private ChatResponseUpdate CreateRequestUpdate(RequestInfoEvent requestInfoEvent)
    {
        if (requestInfoEvent.Request.TryGetDataAs(value: out ToolApprovalRequestContent approvalRequest) &&
            approvalRequest != null)
        {
            RegisterPendingRequest(request: requestInfoEvent.Request, requestId: approvalRequest.RequestId);

            return new ChatResponseUpdate
            {
                Contents = new AIContent[]
                                  {
                                      approvalRequest,
                                  },
                RawRepresentation = requestInfoEvent,
                Role = ChatRole.Assistant,
            };
        }

        return CreateWorkflowEventUpdate(workflowEvent: requestInfoEvent);
    }

    /// <summary>
    /// Registers an external request into the pending requests lookup.
    /// </summary>
    /// <param name="request">The external request details.</param>
    /// <param name="requestId">The request identifier.</param>
    private void RegisterPendingRequest(
        ExternalRequest request,
        string requestId)
    {
        if (string.IsNullOrWhiteSpace(value: requestId))
        {
            return;
        }

        pendingRequests[key: requestId] = request;
    }

    /// <summary>
    /// Determines whether the message sequence contains user or assistant messages.
    /// </summary>
    /// <param name="messages">The chat messages.</param>
    /// <returns><see langword="true"/> if non-system messages are present; otherwise <c>false</c>.</returns>
    private static bool HasNonSystemMessages(IEnumerable<ChatMessage> messages)
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

    /// <summary>
    /// Converts workflow output data into chat response updates.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="workflowEvent"></param>
    /// <returns>An enumeration of chat response updates.</returns>
    private static IEnumerable<ChatResponseUpdate> ConvertOutputData(
        object data,
        WorkflowEvent workflowEvent)
    {
        if (data == null)
        {
            yield break;
        }

        if (data is ChatMessage message)
        {
            yield return CreateMessageUpdate(message: message, workflowEvent: workflowEvent);

            yield break;
        }

        if (data is IEnumerable<ChatMessage> messages)
        {
            foreach (var messageItem in messages)
            {
                yield return CreateMessageUpdate(message: messageItem, workflowEvent: workflowEvent);
            }

            yield break;
        }

        if (data is AIContent content)
        {
            yield return CreateContentUpdate(
                                             contents: new AIContent[]
                                                       {
                                                           content,
                                                       },
                                             workflowEvent: workflowEvent);

            yield break;
        }

        if (data is IEnumerable<AIContent> contents)
        {
            yield return CreateContentUpdate(contents: [.. contents], workflowEvent: workflowEvent);

            yield break;
        }

        if (data is string text)
        {
            yield return CreateTextUpdate(text: text, workflowEvent: workflowEvent);
        }
    }

    /// <summary>
    /// Resets the active streaming run and releases resources.
    /// </summary>
    /// <returns>A task representing the reset operation.</returns>
    private async Task ResetActiveStreamingRunAsync()
    {
        if (activeStreamingRun == null)
        {
            return;
        }

        await activeStreamingRun.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
        activeStreamingRun = null;
        pendingRequests.Clear();
    }

    /// <summary>
    /// Creates a chat response update from an agent response update event.
    /// </summary>
    /// <param name="agentResponseUpdateEvent">The agent event.</param>
    /// <returns>The chat response update.</returns>
    private static ChatResponseUpdate CreateAgentUpdate(AgentResponseUpdateEvent agentResponseUpdateEvent)
    {
        var source = agentResponseUpdateEvent.Update;

        return new ChatResponseUpdate
        {
            AuthorName = source.AuthorName,
            ContinuationToken = source.ContinuationToken,
            Contents = source.Contents?.Count > 0
                                  ? [with(collection: source.Contents)]
                                  : CreateTextContents(text: source.Text),
            CreatedAt = source.CreatedAt,
            MessageId = source.MessageId,
            RawRepresentation = agentResponseUpdateEvent,
            ResponseId = source.ResponseId,
            Role = source.Role,
        };
    }

    /// <summary>
    /// Creates a chat response update when a workflow executor completes.
    /// </summary>
    /// <param name="executorCompletedEvent">The executor completion event.</param>
    /// <returns>The chat response update.</returns>
    private static ChatResponseUpdate CreateExecutorUpdate(ExecutorCompletedEvent executorCompletedEvent)
    {
        return new ChatResponseUpdate(
                                      role: null,
                                      contents: executorCompletedEvent.Data is string text ? CreateTextContents(text: text) : null)
        {
            AuthorName = executorCompletedEvent.ExecutorId,
            MessageId = Guid.NewGuid().ToString(),
            RawRepresentation = executorCompletedEvent,
            Role = ChatRole.Tool,
        };
    }

    /// <summary>
    /// Creates a chat response update from an agent message.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="workflowEvent"></param>
    /// <returns>The chat response update.</returns>
    private static ChatResponseUpdate CreateMessageUpdate(
        ChatMessage message,
        WorkflowEvent workflowEvent)
    {
        return new ChatResponseUpdate
        {
            AuthorName = message.AuthorName,
            Contents = message.Contents?.Count > 0
                                  ? [with(collection: message.Contents)]
                                  : CreateTextContents(text: message.Text),
            CreatedAt = message.CreatedAt,
            MessageId = message.MessageId,
            RawRepresentation = workflowEvent,
            Role = message.Role,
        };
    }

    /// <summary>
    /// Creates a chat response update containing the specified AI contents.
    /// </summary>
    /// <param name="contents">The collection of AI contents.</param>
    /// <param name="workflowEvent"></param>
    /// <returns>The chat response update.</returns>
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

    /// <summary>
    /// Creates a chat response update containing text content.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="workflowEvent"></param>
    /// <returns>The chat response update.</returns>
    private static ChatResponseUpdate CreateTextUpdate(
        string text,
        WorkflowEvent workflowEvent)
    {
        return new ChatResponseUpdate
        {
            Contents = CreateTextContents(text: text),
            RawRepresentation = workflowEvent,
            Role = ChatRole.Assistant,
        };
    }

    /// <summary>
    /// Wraps a raw workflow event into a custom chat response update.
    /// </summary>
    /// <param name="workflowEvent">The workflow event to wrap.</param>
    /// <returns>The chat response update.</returns>
    private static ChatResponseUpdate CreateWorkflowEventUpdate(WorkflowEvent workflowEvent)
    {
        return new ChatResponseUpdate
        {
            Contents = new AIContent[]
                              {
                                  new WorkflowEventContent(workflowEvent: workflowEvent),
                              },
            RawRepresentation = workflowEvent,
            Role = ChatRole.Assistant,
        };
    }

    /// <summary>
    /// Wraps a text string into an AI text content list.
    /// </summary>
    /// <param name="text">The text payload.</param>
    /// <returns>A list containing a <see cref="TextContent"/> instance.</returns>
    private static IList<AIContent> CreateTextContents(string text)
    {
        return string.IsNullOrEmpty(value: text)
            ? []
            : (new AIContent[]
               {
                   new TextContent(text: text),
               });
    }

}
