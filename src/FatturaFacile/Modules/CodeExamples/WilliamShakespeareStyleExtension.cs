using System;
using DevExpress.AIIntegration.Extensions;

namespace AIDemo.Modules.CodeExamples {
    public class WilliamShakespeareStyleExtension : ProofreadExtension {
        public WilliamShakespeareStyleExtension(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override string GetSystemPrompt(ProofreadRequest request) =>
            "Rewrite this text in the William Shakespeare style.";
    }
}
