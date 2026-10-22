using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Azure.AI.OpenAI;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;
using DevExpress.Xpf.Core;
using Microsoft.Extensions.AI;

namespace AIDemo.Modules.CodeExamples {
    public partial class CustomProcessing : UserControl {
        AIExtensionsContainerDefault defaultAIContainer;
        public CustomProcessing() {
            InitializeComponent();
        }
        void OnLoaded(object sender, RoutedEventArgs e) {
            defaultAIContainer = new AIExtensionsContainerDefault();
            string azureOpenAiEndpoint = "https://api.devexpress.com/demo-openai";
            string azureOpenAiKey = "DEMO";
            string deploymentName = "demo-mini";
            IChatClient azureOpenAiClient = new AzureOpenAIClient(new Uri(azureOpenAiEndpoint),
                    new System.ClientModel.ApiKeyCredential(azureOpenAiKey))
                .GetChatClient(deploymentName)
                .AsIChatClient();
            defaultAIContainer.RegisterChatClient(azureOpenAiClient);
            defaultAIContainer.Register<TranslateRequest, TranslateExtension>();
        }

        void OnUnloaded(object sender, RoutedEventArgs e) {
            defaultAIContainer = null;
        }

        async void OnClick(object sender, RoutedEventArgs e) {
            
            var request = new TranslateRequest("Have fun experimenting with this code", "Elvish");
            var response = await defaultAIContainer.TranslateAsync(request, CancellationToken.None);
            string translatedText = response.Response;
            if(response.IsCompleted) {
                
            }
            else if(!response.IsRestrictedOrFailed) {
                while(response.IsContinuationRequired) {
                    await response.ContinueAsync();
                    translatedText += response.Response;
                }
            }
            else { 
                switch(response.Status) {
                    case ResponseStatus.MaxTokenLimitExceeded:
                    case ResponseStatus.InputSizeLimitExceeded:
                        translatedText = "The text you're trying to send within a request is too long and exceeds the limit.";
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
}
