// <copyright file="CodeExamplesModule.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Xpf.DemoBase;
using FatturaFacile.Modules.CodeExamples;
using FatturaFacile.Wpf.AI.Modules.CodeExamples;

namespace FatturaFacile.Wpf.AI.Modules;

/// <summary>
/// Demonstrates code examples and customization patterns for DevExpress AI integrations.
/// </summary>
public class CodeExamplesModule : ShowcaseBrowserModule
{

    /// <inheritdoc />
    protected override IShowcaseInfo[] CreateShowcases()
    {
        return
               [
                   LoadShowcase(
                                name: "Modify default extension",
                                helpUri: null,
                                path: "Modules/CodeExamples",
                                types:
                                       [
                                           typeof(ModifyDefaultExtension), typeof(WilliamShakespeareStyleExtension),
                                       ],
                                showCodeBehind: true),
                   LoadShowcase(
                                name: "Create custom extension",
                                helpUri: null,
                                path: "Modules/CodeExamples",
                                types:
                                       [
                                           typeof(CreateCustomExtension), typeof(AuthoredStyleBehavior),
                                           typeof(AuthoredStyleExtension), typeof(AuthoredStyleRequest),
                                       ],
                                showCodeBehind: true),
                   LoadShowcase(
                                name: "Configure AIExtensions composition",
                                helpUri: null,
                                path: "Modules/CodeExamples",
                                types:
                                       [
                                           typeof(ConfigureAiExtensionsComposition), typeof(ErnestHemingwayStyleExtension),
                                       ],
                                showCodeBehind: true),
                   LoadShowcase(
                                name: "Custom Processing",
                                helpUri: null,
                                path: "Modules/CodeExamples",
                                types:
                                       [
                                           typeof(CustomProcessing),
                                       ],
                                showCodeBehind: true),
                   LoadShowcase(
                                name: "Error Logging and Handling",
                                helpUri: null,
                                path: "Modules/CodeExamples",
                                types:
                                       [
                                           typeof(ErrorLoggingAndHandling), typeof(AIExceptionHandler),
                                       ],
                                showCodeBehind: true),
               ];
    }

}
