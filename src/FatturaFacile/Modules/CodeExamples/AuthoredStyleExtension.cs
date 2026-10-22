using System;
using DevExpress.AIIntegration.Extensions;

namespace AIDemo.Modules.CodeExamples {
    public class AuthoredStyleExtension : ChangeTextExtension<AuthoredStyleRequest> {
        public AuthoredStyleExtension(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override string GetSystemPrompt(AuthoredStyleRequest request) =>
            $"Rewrite this text in the {request.Author} style";
    }
}
