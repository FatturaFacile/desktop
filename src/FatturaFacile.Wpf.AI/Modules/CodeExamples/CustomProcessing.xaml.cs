// <copyright file="CustomProcessing.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using Azure.AI.OpenAI;

using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;
using DevExpress.Xpf.Core;

using Microsoft.Extensions.AI;

namespace FatturaFacile.Modules.CodeExamples;

/// <summary>
/// Interaction logic and demonstration view for custom direct invocation of AI extensions, handling token limits, streaming continuation, and content filters.
/// </summary>
public partial class CustomProcessing : System.Windows.Controls.UserControl
{

    /// <summary>
    /// The demonstration AI extensions container.
    /// </summary>
    private AIExtensionsContainerDefault defaultAIContainer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomProcessing"/> class.
    /// </summary>
    public CustomProcessing()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the Loaded event to configure chat client endpoints and register extension handlers.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The routed event arguments.</param>
    private void OnLoaded(
        object          sender,
        RoutedEventArgs e)
    {
        defaultAIContainer = new AIExtensionsContainerDefault();
        const string azureOpenAiEndpoint = "https://api.devexpress.com/demo-openai";
        const string azureOpenAiKey      = "DEMO";
        const string deploymentName      = "demo-mini";

        var azureOpenAiClient = new AzureOpenAIClient(
                                                      endpoint: new Uri(uriString: azureOpenAiEndpoint),
                                                      credential: new System.ClientModel.ApiKeyCredential(key: azureOpenAiKey))
                               .GetChatClient(deploymentName: deploymentName)
                               .AsIChatClient();

        defaultAIContainer.RegisterChatClient(chatClient: azureOpenAiClient);
        _ = defaultAIContainer.Register<TranslateRequest, TranslateExtension>();
    }

    /// <summary>
    /// Handles the Unloaded event to clean up container resources.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The routed event arguments.</param>
    private void OnUnloaded(
        object          sender,
        RoutedEventArgs e) => defaultAIContainer = null;

    /// <summary>
    /// Handles the button click to initiate custom AI processing and streaming text updates.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The routed event arguments.</param>
    private async void OnClick(
        object          sender,
        RoutedEventArgs e)
    {
        var request        = new TranslateRequest(text: "Have fun experimenting with this code", language: "Elvish");
        var response       = await defaultAIContainer.TranslateAsync(request: request, cancellationToken: CancellationToken.None);
        var translatedText = response.Response;

        if (response.IsCompleted)
        {
        }
        else if (!response.IsRestrictedOrFailed)
        {
            while (response.IsContinuationRequired)
            {
                await response.ContinueAsync();
                translatedText += response.Response;
            }
        }
        else
        {
            switch (response.Status)
            {
                case ResponseStatus.MaxTokenLimitExceeded:
                case ResponseStatus.InputSizeLimitExceeded:
                    translatedText =
                        "The text you're trying to send within a request is too long and exceeds the limit.";

                    break;
                case ResponseStatus.ContentFiltered:
                    translatedText = "Potentially harmful content was detected in your request.";

                    break;
                case ResponseStatus.Error:
                    translatedText = "An error occurred while processing the request.";

                    break;
            }
        }

        _ = DXMessageBox.Show(messageBoxText: translatedText, caption: "Translation", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
    }

}
