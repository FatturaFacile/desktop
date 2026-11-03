// <copyright file="ToolApprovalAIFunctions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace FatturaFacile.Wpf.AI.Agents.ToolApprovals;

/// <summary>
/// Provides AI function tools that require explicit human approval before execution.
/// </summary>
public static class ToolApprovalAIFunctions
{

    /// <summary>
    /// Gets the approval-wrapped tool function for updating CRM account tiers.
    /// </summary>
    public static AIFunction UpdateAccountTierTool => new ApprovalRequiredAIFunction(
        innerFunction: AIFunctionFactory.Create(
                                                method: (Func<string, bool, string, int, string>)UpdateAccountTier,
                                                description: "Updates the account tier for CRM accounts matching the specified state and active status. " +
                                                             "Use this when users ask to change, update, or set the tier level of accounts."));

    /// <summary>
    /// Gets the approval-wrapped tool function for sending bulk email notifications.
    /// </summary>
    public static AIFunction SendBulkEmailTool => new ApprovalRequiredAIFunction(
        innerFunction: AIFunctionFactory.Create(
                                                method: (Func<string, string, string, int, string, string>)SendBulkEmail,
                                                description: "Sends a bulk email to recipients based on a filter criteria. " +
                                                             "Use this when users ask to send emails, reminders, or notifications to a group of people."));

    /// <summary>
    /// Gets the approval-wrapped tool function for archiving historical orders.
    /// </summary>
    public static AIFunction ArchiveOrdersTool => new ApprovalRequiredAIFunction(
        innerFunction: AIFunctionFactory.Create(
                                                method: (Func<int, int, int, string, string>)ArchiveOrders,
                                                description: "Archives or deletes orders older than a specified date threshold. " +
                                                             "Use this when users ask to clean up, archive, or remove old orders."));

    /// <summary>
    /// Updates the account tier for all CRM accounts matching the given state and active status.
    /// </summary>
    /// <param name="state">The full state name, e.g. 'Texas', 'California', 'New York'.</param>
    /// <param name="isActive">Whether to target active (true) or inactive (false) accounts.</param>
    /// <param name="newTier">The new tier level to assign, e.g. 'Tier 1', 'Tier 2', 'Tier 3'.</param>
    /// <param name="accountCount">Number of accounts that will be updated (e.g. 8).</param>
    /// <returns>A status message summarizing the account updates.</returns>
    [Description(description: "Updates the account tier for all CRM accounts matching the given state and active status.")]
    private static string UpdateAccountTier(
        [Description(description: "The full state name, e.g. 'Texas', 'California', 'New York'")] string state,
        [Description(description: "Whether to target active (true) or inactive (false) accounts")]
        bool isActive,
        [Description(description: "The new tier level to assign, e.g. 'Tier 1', 'Tier 2', 'Tier 3'")]
        string newTier,
        [Description(description: "Number of accounts that will be updated (e.g. 8)")]
        int accountCount) =>
        $"All {accountCount} {(isActive ? "active" : "inactive")} accounts in {state} have been moved to the {newTier} service level.";

    /// <summary>
    /// Sends a bulk email notification to recipients matching the specified filter.
    /// </summary>
    /// <param name="recipientFilter">The filter for selecting recipients, e.g. 'overdue invoices', 'unpaid accounts'.</param>
    /// <param name="template">The email template to use, e.g. 'Friendly Nudge', 'Overdue Notice', 'Payment Reminder'.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="clientCount">Number of clients who will receive the email (e.g. 4).</param>
    /// <param name="totalBalance">Total outstanding balance across selected clients (e.g. '$321.00').</param>
    /// <returns>A status message summarizing the bulk email dispatch.</returns>
    [Description(description: "Sends a bulk email notification to recipients matching the specified filter.")]
    private static string SendBulkEmail(
        [Description(description: "The filter for selecting recipients, e.g. 'overdue invoices', 'unpaid accounts'")]
        string recipientFilter,
        [Description(description: "The email template to use, e.g. 'Friendly Nudge', 'Overdue Notice', 'Payment Reminder'")]
        string template,
        [Description(description: "The email subject line")]
        string subject,
        [Description(description: "Number of clients who will receive the email (e.g. 4)")]
        int clientCount,
        [Description(description: "Total outstanding balance across selected clients (e.g. '$321.00')")]
        string totalBalance) =>
        $"All {clientCount} clients have been successfully nudged regarding their unpaid balances. Total balance: {totalBalance}.";

    /// <summary>
    /// Archives orders older than the specified date threshold.
    /// </summary>
    /// <param name="year">The cutoff year — orders before this year will be archived, e.g. 2020.</param>
    /// <param name="startYear">The earliest year to include in the archive range (e.g. 1999).</param>
    /// <param name="recordCount">Number of order records to be archived (e.g. 1234).</param>
    /// <param name="destination">Archive destination name (e.g. 'Secure archive vault').</param>
    /// <returns>A status message summarizing the archived records.</returns>
    [Description(description: "Archives orders older than the specified date threshold.")]
    private static string ArchiveOrders(
        [Description(description: "The cutoff year — orders before this year will be archived, e.g. 2020")] int year,
        [Description(description: "The earliest year to include in the archive range (e.g. 1999)")]
        int startYear,
        [Description(description: "Number of order records to be archived (e.g. 1234)")]
        int recordCount,
        [Description(description: "Archive destination name (e.g. 'Secure archive vault')")]
        string destination) =>
        $"{recordCount:N0} records from {startYear}–{year} have been moved to the {destination}.";

}
