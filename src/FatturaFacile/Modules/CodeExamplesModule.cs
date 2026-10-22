using DevExpress.Xpf.DemoBase;
using AIDemo.Modules.CodeExamples;

namespace AIDemo {
    public class CodeExamplesModule : ShowcaseBrowserModule {
        protected override IShowcaseInfo[] CreateShowcases() {
            return new IShowcaseInfo[] {
                LoadShowcase("Modify default extension", null, "Modules/CodeExamples",
                    new[] { typeof(ModifyDefaultExtension), typeof(WilliamShakespeareStyleExtension) },
                    showCodeBehind: true),
                LoadShowcase("Create custom extension", null, "Modules/CodeExamples",
                    new[] {
                        typeof(CreateCustomExtension), typeof(AuthoredStyleBehavior), typeof(AuthoredStyleExtension),
                        typeof(AuthoredStyleRequest)
                    }, showCodeBehind: true),
                LoadShowcase("Configure AIExtensions composition", null, "Modules/CodeExamples",
                    new[] { typeof(ConfigureAiExtensionsComposition), typeof(ErnestHemingwayStyleExtension) },
                    showCodeBehind: true),
                LoadShowcase("Custom Processing", null, "Modules/CodeExamples", new[] { typeof(CustomProcessing) },
                    showCodeBehind: true),
                LoadShowcase("Error Logging and Handling", null, "Modules/CodeExamples",
                    new[] { typeof(ErrorLoggingAndHandling), typeof(AIExceptionHandler) },
                    showCodeBehind: true)
            };
        }
    }
}
