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

public partial class CustomProcessing : System.Windows.Controls.UserControl
{
    private AIExtensionsContainerDefault defaultAIContainer;

    public CustomProcessing()
    {
        InitializeComponent();
    }

    private void OnLoaded(
        object sender,
        RoutedEventArgs e)
    {
        defaultAIContainer = new AIExtensionsContainerDefault();
        const string azureOpenAiEndpoint = "https://api.devexpress.com/demo-openai";
        const string azureOpenAiKey = "DEMO";
        const string deploymentName = "demo-mini";
        var azureOpenAiClient = new AzureOpenAIClient(
                new Uri(azureOpenAiEndpoint),
                new System.ClientModel.ApiKeyCredential(azureOpenAiKey))
            .GetChatClient(deploymentName)
            .AsIChatClient();
        defaultAIContainer.RegisterChatClient(azureOpenAiClient);
        defaultAIContainer.Register<TranslateRequest, TranslateExtension>();
    }

    private void OnUnloaded(
        object sender,
        RoutedEventArgs e) => defaultAIContainer = null;

    private async void OnClick(
        object sender,
        RoutedEventArgs e)
    {
        var request = new TranslateRequest("Have fun experimenting with this code", "Elvish");
        var response = await defaultAIContainer.TranslateAsync(request, CancellationToken.None);
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

        DXMessageBox.Show(translatedText, "Translation", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
