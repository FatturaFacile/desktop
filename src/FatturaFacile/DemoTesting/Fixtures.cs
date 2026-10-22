using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Wpf;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.DemoBase;
using DevExpress.Xpf.DemoBase.DemoTesting;
using DevExpress.Xpf.DemoBase.Helpers.TextColorizer;
using DevExpress.Xpf.Editors;

namespace AIDemo.Tests {
    public class AICheckAllDemosFixture : CheckAllDemosFixture {
#if NET 
        Type[] skipMemoryLeaksCheckModules = { typeof(SemanticSearchModule), typeof(AIToolsModule) };
#else
        Type[] skipMemoryLeaksCheckModules = { typeof(SemanticSearchModule) };
#endif

        protected override bool CheckMemoryLeaks(Type moduleType) {
            return !skipMemoryLeaksCheckModules.Contains(moduleType);
        }
#if NET
        protected override bool CheckMemoryLeaksWhenFinishingTesting() {
            return false;
        }
#endif
        protected override bool CanRunModule(Type moduleType) {
            return moduleType != typeof(RichEditModule);
        }
    }

    public class CheckAIFeaturesFixture : BaseDemoTestingFixture {
        readonly AIDemoModulesAccessor modulesAccessor;

        public CheckAIFeaturesFixture() {
            modulesAccessor = new AIDemoModulesAccessor(this);
        }

        protected override void CreateActions() {
            base.CreateActions();
            CreateTextEditActions();
        }

        void CreateTextEditActions() {
            AddLoadModuleActions(typeof(TextEditModule));
            Dispatch(async () => {
                var te = modulesAccessor.TextEdit;
                te.SelectionStart = 0;
                te.SelectionLength = te.Text.Length;
                DispatcherHelper.UpdateLayoutAndDoEvents();

                AIExtensionsContainerDesktop.Default.UnregisterChatClient();
                AIExtensionsContainerDesktop.Default.RegisterChatClient(new ChatClient("a"));

                var expandBehavior = Interaction.GetBehaviors(te).OfType<ExpandBehavior>().Single();
                expandBehavior.BehaviorSource.ShowResultDialog = false;
                await expandBehavior.ExpandAsync();
                Assert.AreEqual("a", te.Text);
            });
        }
    }

    class AIDemoModulesAccessor : DemoModulesAccessor<DemoModule> {
        public AIDemoModulesAccessor(BaseDemoTestingFixture fixture) : base(fixture) { }

        public TextEdit TextEdit => ((TextEditModule)DemoModule).mainTextEdit;
    }

    class ChatClient : IChatClient {
        readonly ChatResponse chatResponse;

        public ChatClient(string response) {
            var message = new ChatMessage(ChatRole.Assistant, response);
            chatResponse = new ChatResponse(message) { FinishReason = ChatFinishReason.Stop };
        }

        public void Dispose() {
            throw new NotImplementedException();
        }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(chatResponse);

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> chatMessages, ChatOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken)) {
            throw new NotImplementedException();
        }

        object IChatClient.GetService(Type serviceType, object serviceKey) {
            throw new NotImplementedException();
        }

    }
}
