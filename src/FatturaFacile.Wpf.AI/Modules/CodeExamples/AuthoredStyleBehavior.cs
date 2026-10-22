// <copyright file="AuthoredStyleBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Input;

using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Wpf;
using DevExpress.AIIntegration.Wpf.Internal;
using DevExpress.Xpf.Editors;

namespace FatturaFacile.Wpf.AI.Modules.CodeExamples;

/// <summary>
/// Custom editor behavior attaching literary author style rewriting operations to DevExpress text editors.
/// </summary>
class AuthoredStyleBehavior : TextModificationBehavior
{

    static AuthoredStyleBehavior()
    {
        RegisterBehaviorSource<TextEdit, AuthoredStyleBehavior>(sourceFactory: te => new TextEditBehaviorSource(control: te));
        _ = AIExtensionsContainerDesktop.Default.Register<AuthoredStyleRequest, AuthoredStyleExtension>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthoredStyleBehavior"/> class.
    /// </summary>
    public AuthoredStyleBehavior()
    {
        AuthoredStyleCommand = CreateCommand<string>(executeAsync: AuthoredStyleAsync);
    }

    private ICommand AuthoredStyleCommand { get; set; }

    private Task AuthoredStyleAsync(string author) => ExecuteAsync(
                                                                   makeRequest: input => new AuthoredStyleRequest(author: author, text: input),
                                                                   requestAsync: (container, request, token) =>
                                                                                 {
                                                                                     var extension = (AuthoredStyleExtension)container
                                                                                        .GetExtension(
                                                                                                      key: typeof(
                                                                                                          AuthoredStyleRequest));

                                                                                     var result =
                                                                                         extension.ExecuteAsync(request: request, cancellationToken: token);

                                                                                     return result;
                                                                                 });

    /// <inheritdoc />
    protected override ItemInfo BuildItem()
    {
        return new ItemInfo
               {
                   Caption = "Authored Style",
                   Items = new[]
                           {
                               "Mark Twain", "Ernest Hemingway", "Maya Angelou",
                           }.Select(selector: author => new ItemInfo
                                                        {
                                                            Caption          = author,
                                                            Command          = AuthoredStyleCommand,
                                                            CommandParameter = author,
                                                        })
                            .ToList(),
               };
    }

}
