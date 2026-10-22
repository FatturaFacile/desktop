using System;
using DevExpress.AIIntegration.Extensions;

namespace AIDemo.Modules.CodeExamples {
    public class ErnestHemingwayStyleExtension : ProofreadExtension {
        public ErnestHemingwayStyleExtension(IServiceProvider serviceProvider)
            : base(serviceProvider) { }
        protected override string GetSystemPrompt(ProofreadRequest request) {
            return "Rewrite this text in the Ernest Hemingway style.";
        }
    }

}
