// <copyright file="IProcurementService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Offline-first purchase order fulfilment (Mod 046): reads fall back to the last cached answer, writes are saved in the outbox first
/// and replayed in order when the server is reachable.
/// </summary>
public interface IProcurementService
{
    /// <summary>
    /// Loads the overdue orders.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The orders, or null when neither the server nor the cache can answer.</returns>
    public Task<ProcurementLoad<IReadOnlyList<OverdueOrderView>>?> LoadOverdueAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Loads the purchase orders.
    /// </summary>
    /// <param name="status">The wire status to filter by, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The orders, or null when neither the server nor the cache can answer.</returns>
    public Task<ProcurementLoad<IReadOnlyList<PurchaseOrderView>>?> LoadOrdersAsync(string? status, CancellationToken cancellationToken);

    /// <summary>
    /// Loads one purchase order with its lines.
    /// </summary>
    /// <param name="id">The order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The order, or null when neither the server nor the cache can answer.</returns>
    public Task<ProcurementLoad<PurchaseOrderView>?> LoadOrderAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the supplier confirmations of an order.
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The confirmations, or null when neither the server nor the cache can answer.</returns>
    public Task<ProcurementLoad<IReadOnlyList<SupplierConfirmationView>>?> LoadConfirmationsAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the dispatch log of an order.
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dispatches, or null when neither the server nor the cache can answer.</returns>
    public Task<ProcurementLoad<IReadOnlyList<DispatchView>>?> LoadDispatchesAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the reminder log of an order.
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The reminders, or null when neither the server nor the cache can answer.</returns>
    public Task<ProcurementLoad<IReadOnlyList<ReminderView>>?> LoadRemindersAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the dropshipments.
    /// </summary>
    /// <param name="status">The wire status to filter by, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dropshipments, or null when neither the server nor the cache can answer.</returns>
    public Task<ProcurementLoad<IReadOnlyList<DropshipmentView>>?> LoadDropshipmentsAsync(string? status, CancellationToken cancellationToken);

    /// <summary>
    /// Loads one dropshipment.
    /// </summary>
    /// <param name="id">The dropshipment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dropshipment, or null when neither the server nor the cache can answer.</returns>
    public Task<ProcurementLoad<DropshipmentView>?> LoadDropshipmentAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Previews the pricing of a line: the server's answer, or the local estimate when the server cannot be reached.
    /// </summary>
    /// <param name="request">The inputs.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The amounts; <see cref="ProcurementLoad{T}.FromCache"/> tells it is the local estimate.</returns>
    public Task<ProcurementLoad<PricingPreview>> PreviewPricingAsync(PricingPreviewRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Transmits a purchase order.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<ProcurementActionResult> DispatchOrderAsync(DispatchOrderRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Records a supplier confirmation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<ProcurementActionResult> RecordConfirmationAsync(SupplierConfirmationRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Reminds a supplier of an overdue order.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<ProcurementActionResult> SendReminderAsync(OrderReminderRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Creates the supplier order of a dropship sales order.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<ProcurementActionResult> CreateDropshipmentAsync(CreateDropshipmentRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Issues the custom transport document of a dropshipment.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<ProcurementActionResult> IssueDdtAsync(IssueDdtRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Records the delivery notice of a dropshipment.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<ProcurementActionResult> RecordDeliveryAsync(RecordDeliveryRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Replays the queued writes in order.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<ProcurementFlushResult> FlushAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Loads the queued writes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The waiting and the refused writes.</returns>
    public Task<ProcurementQueue> LoadQueueAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Acknowledges a refused write so it leaves the list.
    /// </summary>
    /// <param name="entryId">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the update.</returns>
    public Task DismissConflictAsync(Guid entryId, CancellationToken cancellationToken);
}
