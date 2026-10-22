// <copyright file="AgentFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net.Http;
using FatturaFacile.Agents.TextProcessing;

using DevExpress.AIIntegration.Agents;
using DevExpress.AIIntegration.Chat;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using FatturaFacile.Wpf.AI.Agents.Shakespearean;
using FatturaFacile.Wpf.AI.Agents.ToolApprovals;

namespace FatturaFacile.Wpf.AI.Agents;

/// <summary>
/// Factory for constructing specialized AI agents, multi-agent workflows, and response providers.
/// </summary>
public class AgentFactory
{

    /// <summary>
    /// The underlying chat client used by constructed agents.
    /// </summary>
    private readonly IChatClient chatClient;

    /// <summary>
    /// System prompt instructions for the weather assistant agent.
    /// </summary>
    private const string WeatherAgentInstructions =
        "You are a weather assistant that provides instant weather information.\n\n" +
        "CRITICAL RULES:\n" +
        "1. When a user asks about weather, IMMEDIATELY call the GetWeather tool with the city name - do NOT ask for clarification.\n" +
        "2. If the city name is ambiguous (e.g., 'Paris'), assume the most well-known location (Paris, France).\n" +
        "3. Extract the city name from the user's question even if it's informal (e.g., 'weather in london' → 'London').\n" +
        "4. NEVER ask follow-up questions like 'Which Paris?' or 'Can you provide more details?'\n" +
        "5. If the city cannot be determined, make your best guess based on context.\n" +
        "6. After calling the tool, provide a brief, friendly summary like 'Here\\'s the current weather for [City]!' - keep it SHORT.\n" +
        "7. Do NOT repeat the raw weather data in your text response since a weather widget is rendered automatically from the tool result.";

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentFactory"/> class.
    /// </summary>
    /// <param name="chatClient">The underlying chat client.</param>
    public AgentFactory(IChatClient chatClient)
    {
        this.chatClient = chatClient;
    }

    /// <summary>
    /// Creates a Shakespearean poet multi-agent workflow response provider.
    /// </summary>
    /// <returns>An <see cref="IChatResponseProvider"/> orchestrating the poet and Shakespearean rewriter agents.</returns>
    public IChatResponseProvider CreateShakespeareanPoetAgenticWorkflow()
    {
        var poemAgent          = CreatePoemAgent();
        var shakespeareanAgent = CreateShakespeareanAgent();

        var workflow = new WorkflowBuilder(start: poemAgent)
                      .AddEdge(source: poemAgent, target: shakespeareanAgent)
                      .Build();

        return workflow
              .AsAIAgent(name: "ShakespeareanPoetWorkflow")
              .AsIChatResponseProvider();
    }

    /// <summary>
    /// Creates a Shakespearean poet workflow.
    /// </summary>
    /// <returns>A configured multi-agent <see cref="Workflow"/>.</returns>
    public Workflow CreateShakespeareanPoetWorkflow()
    {
        var poemAgent          = CreatePoemAgent();
        var shakespeareanAgent = CreateShakespeareanAgent();

        return new WorkflowBuilder(start: poemAgent)
              .AddEdge(source: poemAgent, target: shakespeareanAgent)
              .Build();
    }

    /// <summary>
    /// Creates an AI agent equipped with CRM tool approval functions.
    /// </summary>
    /// <returns>A configured <see cref="AIAgent"/> instance.</returns>
    public AIAgent CreateToolApprovalAgent()
    {
        const string instructions =
            "You are an AI assistant for a CRM demo system. The following data is currently in the system:\n" +
            "- CRM Accounts: 8 inactive accounts in Texas at the Tier 1 service level.\n" +
            "- Overdue Invoices: 4 clients with unpaid invoices. Total outstanding balance: $321.00.\n" +
            "- Order History: 1,234 orders placed between 1999 and 2019 (before 2020). Archive destination: Secure archive vault.\n\n" +
            "When calling tools, always use these exact values:\n" +
            "- UpdateAccountTier: accountCount=8 for inactive Texas accounts.\n" +
            "- SendBulkEmail: clientCount=4, totalBalance=\"$321.00\" for overdue invoice reminders.\n" +
            "- ArchiveOrders: recordCount=1234, startYear=1999, destination=\"Secure archive vault\" for pre-2020 orders.";

        return chatClient.AsAIAgent(
                                    instructions: instructions,
                                    tools: new[]
                                           {
                                               ToolApprovalAIFunctions.UpdateAccountTierTool,
                                               ToolApprovalAIFunctions.SendBulkEmailTool,
                                               ToolApprovalAIFunctions.ArchiveOrdersTool,
                                           });
    }

    /// <summary>
    /// Creates a text processing workflow that converts text to uppercase and reverses it.
    /// </summary>
    /// <returns>A configured <see cref="Workflow"/>.</returns>
    public static Workflow CreateTextProcessingWorkflow()
    {
        var uppercase = new UppercaseExecutor();
        var reverse   = new ReverseTextExecutor();

        return new WorkflowBuilder(start: uppercase)
              .AddEdge(source: uppercase, target: reverse)
              .Build();
    }

    /// <summary>
    /// Creates a creative poet agent.
    /// </summary>
    /// <returns>A configured <see cref="ChatClientAgent"/>.</returns>
    public ChatClientAgent CreatePoemAgent()
    {
        const string instructions =
            "You are a creative poet AI. Your task is to generate a short, lyrical poem (5-7 lines) " +
            "from a few keywords provided by the user.\n\n"                                           +
            "Rules:\n"                                                                                +
            "- The user will send 2-3 words separated by spaces or commas.\n"                         +
            "- Weave ALL provided keywords into the poem naturally.\n"                                +
            "- Use vivid imagery, optional rhyme, and pleasant rhythm.\n"                             +
            "- Output ONLY the poem text - no titles, no explanations, no extra commentary.\n"        +
            "- In the beginning of your response always add [Response from the PoemAgent] and a new line.";

        return chatClient.AsAIAgent(
                                    name: "PoemAgent",
                                    instructions: instructions);
    }

    /// <summary>
    /// Creates a literary agent that rewrites text into Elizabethan Shakespearean English.
    /// </summary>
    /// <returns>A configured <see cref="ChatClientAgent"/>.</returns>
    public ChatClientAgent CreateShakespeareanAgent()
    {
        const string instructions =
            "You are a literary AI that transforms text into the style of William Shakespeare.\n\n"   +
            "Rules:\n"                                                                                +
            "- Before rewriting, call the GetShakespeareanDictionary tool to retrieve a list of "     +
            "authentic Elizabethan words and their meanings.\n"                                       +
            "- Use as many words from the dictionary as naturally fit in the rewritten text.\n"       +
            "- Rewrite the provided text using Elizabethan English (thee, thou, hath, doth, etc.).\n" +
            "- Preserve the original meaning and imagery of the text.\n"                              +
            "- Add dramatic flair, poetic cadence, and Shakespearean vocabulary.\n"                   +
            "- Output ONLY the rewritten text - no titles, no explanations, no extra commentary.\n"   +
            "- In the beginning of your response always add [Response from the ShakespeareanAgent] and a new line.";

        return chatClient.AsAIAgent(
                                    name: "ShakespeareanAgent",
                                    instructions: instructions,
                                    tools: new[]
                                           {
                                               ShakespeareanTools.GetDictionaryTool(),
                                           });
    }

    /// <summary>
    /// Creates a weather reporting assistant agent.
    /// </summary>
    /// <param name="weatherTool">The weather inquiry function tool.</param>
    /// <param name="name">The name identifier of the agent.</param>
    /// <returns>A configured <see cref="AIAgent"/>.</returns>
    public AIAgent CreateWeatherAgent(
        AIFunction weatherTool,
        string     name)
    {
        return chatClient.AsAIAgent(
                                    name: name,
                                    instructions: WeatherAgentInstructions,
                                    tools: new[]
                                           {
                                               weatherTool,
                                           });
    }

    #pragma warning disable DX0006
    /// <summary>
    /// Creates an AG-UI weather response provider connected to a remote AG-UI service endpoint.
    /// </summary>
    /// <param name="agUiEndpoint">The service endpoint URL.</param>
    /// <returns>An <see cref="IChatResponseProvider"/> instance.</returns>
    public static IChatResponseProvider CreateAGUIWeatherResponseProvider(string agUiEndpoint)
    {
        var httpClient = new HttpClient
                         {
                             Timeout = TimeSpan.FromSeconds(seconds: 60),
                         };

        return CreateAGUIWeatherResponseProvider(httpClient: httpClient, agUiEndpoint: agUiEndpoint);
    }

    /// <summary>
    /// Creates an AG-UI weather response provider using the specified HTTP client.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance.</param>
    /// <param name="agUiEndpoint">The service endpoint URL.</param>
    /// <returns>An <see cref="IChatResponseProvider"/> instance.</returns>
    public static IChatResponseProvider CreateAGUIWeatherResponseProvider(
        HttpClient httpClient,
        string     agUiEndpoint)
    {
        var aguiChatClient = new AGUIChatClient(httpClient: httpClient, endpoint: agUiEndpoint);

        var agent = aguiChatClient.AsAIAgent(
                                             name: "weather-client",
                                             description: "AG-UI Weather Client Agent",
                                             instructions: null,
                                             tools: Array.Empty<AIFunction>());

        return agent.AsIChatResponseProvider();
    }
    #pragma warning restore DX0006

}
