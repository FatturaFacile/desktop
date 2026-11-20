// <copyright file="CopilotDrawerViewTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.

using System.Windows;
using System.Windows.Controls;
using FatturaFacile.Application.Copilot;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.Copilot;
using FatturaFacile.Wpf.Views.Copilot;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Smoke test that the Copilot drawer XAML loads and renders every message shape (chart, KPI, table, action card, citations).
/// </summary>
public sealed class CopilotDrawerViewTests
{
    /// <summary>
    /// The drawer renders messages with a bar chart, a KPI card, a table and a pending action card without errors.
    /// </summary>
    [Fact]
    public void Drawer_RendersAllMessageShapes()
    {
        Exception? failure = null;
        string? rendered = null;

        Thread thread = new(() =>
        {
            try
            {
                rendered = Render();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        Assert.True(thread.Join(TimeSpan.FromSeconds(90)), "Rendering the drawer did not finish in time.");
        Assert.Null(failure);
        Assert.Contains("Risposta", rendered, StringComparison.Ordinal);
    }

    private static string Render()
    {
        using CopilotTestHost host = new();
        using CopilotViewModel vm = new(host.Sender, new NoApi(), host.Tenant, LocalizationService.Instance, new Online(), new NoShell());
        var bar = new CopilotVisualization("bar", "Fatturato", ["m", "t"], [new Dictionary<string, string> { ["m"] = "01", ["t"] = "10" }], "m", ["01"], [new CopilotDataset("t", [10d])], null, null);
        var kpi = new CopilotVisualization("kpi_card", "Totale", ["t"], [], null, [], [], "t", "1234.5");
        var table = new CopilotVisualization("table", "Elenco", ["a", "b"], [new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" }], null, [], [], null, null);
        var action = new CopilotProposedAction("t", "m", "pending", "Crea", [new CopilotArgument("q", "2", CopilotArgumentKind.Number)], new Dictionary<string, string> { ["k"] = "v" }, ["q"], null, null);
        var citations = new[] { new CopilotCitation("regulatory", "[LIVA 25]", "LIVA", "Art. 25", "CH", null, null, null) };

        vm.Messages.Add(CopilotMessageViewModel.From(new CopilotMessage("1", "c", "user", "Domanda", [], null, null, null, null), vm));
        vm.Messages.Add(CopilotMessageViewModel.From(new CopilotMessage("2", "c", "assistant", "# Risposta\n\n- uno **due** [LIVA 25]", citations, bar, action, "pending", null), vm));
        vm.Messages.Add(CopilotMessageViewModel.From(new CopilotMessage("3", "c", "assistant", "kpi", [], kpi, null, null, null), vm));
        vm.Messages.Add(CopilotMessageViewModel.From(new CopilotMessage("4", "c", "assistant", "tabella", [], table, null, null, null), vm));

        CopilotDrawerView view = new() { DataContext = vm, Width = 420, Height = 900 };
        Window window = new() { Content = view, Width = 420, Height = 900, ShowInTaskbar = false, WindowStyle = WindowStyle.None, Left = -10000, Top = -10000 };
        window.Show();
        view.UpdateLayout();
        var text = FindText(view);
        window.Close();
        return text;
    }

    private static string FindText(DependencyObject root)
    {
        System.Text.StringBuilder builder = new();
        Collect(root, builder);
        return builder.ToString();
    }

    private static void Collect(DependencyObject node, System.Text.StringBuilder builder)
    {
        if (node is FlowDocumentScrollViewer { Document: { } document })
        {
            _ = builder.Append(new System.Windows.Documents.TextRange(document.ContentStart, document.ContentEnd).Text);
        }

        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(node); i++)
        {
            Collect(System.Windows.Media.VisualTreeHelper.GetChild(node, i), builder);
        }
    }

    private sealed class Online : INetworkStatus
    {
        public event EventHandler? Changed
        {
            add => _ = value;
            remove => _ = value;
        }

        public bool IsOnline => true;
    }

    private sealed class NoShell : IShellLauncher
    {
        public void Open(string path) => _ = path;
    }

    private sealed class NoApi : FatturaFacile.Application.Common.Interfaces.ICopilotApiClient
    {
        public bool IsConfigured => false;

        public Task<IReadOnlyList<CopilotConversation>> GetConversationsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<CopilotConversation> CreateConversationAsync(string? title, string locale, CopilotContext context, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<CopilotConversation> GetConversationAsync(string conversationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task DeleteConversationAsync(string conversationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public IAsyncEnumerable<CopilotStreamEvent> StreamMessageAsync(string conversationId, string prompt, string locale, CopilotContext context, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<CopilotMessage> ConfirmActionAsync(string messageId, IReadOnlyDictionary<string, object?>? overrides, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<CopilotMessage> RejectActionAsync(string messageId, string? reason, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
