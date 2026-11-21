// <copyright file="WorkbenchViewModel.Commands.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Manufacturing.Features;
using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Wpf.ViewModels;

/// <content>
/// Commands of the workbench: every mutation is queued locally first, then synchronised.
/// </content>
internal sealed partial class WorkbenchViewModel
{
    [RelayCommand]
    private Task SyncNowAsync() => RunAsync(async () =>
    {
        var result = await Sender.Send(new SyncWorkbenchCommand());
        await LoadAsync();
        StatusMessage = result switch
        {
            { Push.AuthRequired: true } => Localization["Wb_SyncAuth"],
            { Offline: true } => Localization["Wb_SyncOffline"],
            _ => string.Format(
                CultureInfo.CurrentCulture,
                Localization["Wb_SyncResult"],
                result.Push.Sent,
                result.Push.Replayed,
                result.Push.Conflicts,
                result.Push.Discarded,
                result.Push.Failed,
                result.Pulled),
        };
    });

    [RelayCommand]
    private Task RescheduleAsync() => RunAsync(RebuildScheduleAsync);

    [RelayCommand]
    private Task ReleaseAsync() => QueueTransitionAsync(ManufacturingMutationKind.Release);

    [RelayCommand]
    private Task StartOrderAsync() => QueueTransitionAsync(ManufacturingMutationKind.Start);

    [RelayCommand]
    private Task CompleteAsync() => QueueTransitionAsync(ManufacturingMutationKind.Complete);

    [RelayCommand]
    private Task CloseOrderAsync() => QueueTransitionAsync(ManufacturingMutationKind.Close);

    [RelayCommand]
    private Task ConfirmOperationAsync() => RunAsync(async () =>
    {
        if (SelectedOrder is not { } order || SelectedOperation is not { } operation)
        {
            return;
        }

        var postingUuid = await Sender.Send(new QueueOperationConfirmationCommand(new OperationConfirmationInput(
            order.Id,
            operation.Id,
            SetupMinutes,
            RunMinutes,
            LaborMinutes,
            DowntimeMinutes,
            GoodQuantity,
            ScrapQuantity,
            ScrapReason,
            IsFinal,
            DateTime.Today)));

        StatusMessage = string.Format(CultureInfo.CurrentCulture, Localization["Wb_PostingQueued"], postingUuid);
        ResetConfirmationForm();
        await LoadAsync();
    });

    [RelayCommand]
    private Task CreateOrderAsync() => RunAsync(async () =>
    {
        if (SelectedBom is not { } bom)
        {
            return;
        }

        var routingId = _routing.Where(r => r.ItemId == bom.ItemId).Select(r => (Guid?)r.RoutingId).FirstOrDefault();
        var id = await Sender.Send(new QueueCreateProductionOrderCommand(
            bom.ItemId,
            bom.ItemCode,
            bom.ItemName,
            bom.Id,
            routingId,
            NewOrderQuantity,
            NewOrderStart,
            NewOrderEnd));

        await LoadAsync();
        SelectedOrder = Orders.FirstOrDefault(o => o.Id == id) ?? SelectedOrder;
        StatusMessage = Localization["Wb_OrderQueued"];
    });

    private Task QueueTransitionAsync(ManufacturingMutationKind kind) => RunAsync(async () =>
    {
        if (SelectedOrder is not { } order)
        {
            return;
        }

        _ = await Sender.Send(new QueueOrderTransitionCommand(order.Id, kind));
        await LoadAsync();
        StatusMessage = Localization["Wb_TransitionQueued"];
    });

    private void ResetConfirmationForm()
    {
        SetupMinutes = 0m;
        RunMinutes = 0m;
        LaborMinutes = 0m;
        DowntimeMinutes = 0m;
        GoodQuantity = 0m;
        ScrapQuantity = 0m;
        ScrapReason = string.Empty;
        IsFinal = false;
    }
}
