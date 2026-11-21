// <copyright file="AiCopilotModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.AiCopilot;

/// <summary>
/// EF Core mapping of the AI copilot cache tables.
/// </summary>
public static class AiCopilotModelConfiguration
{
    /// <summary>
    /// Maps the suggestion cache (with the offline decision queue) and the insight cache with tenant isolation.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="currentTenantService">The active tenant.</param>
    public static void ConfigureAiCopilot(this ModelBuilder modelBuilder, ICurrentTenantService currentTenantService)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(currentTenantService);

        _ = modelBuilder.Entity<AiSuggestion>(b =>
        {
            _ = b.ToTable("ai_suggestions");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36).ValueGeneratedNever();
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.Kind).HasColumnName("kind").IsRequired().HasMaxLength(50);
            _ = b.Property(x => x.Workflow).HasColumnName("workflow").IsRequired().HasMaxLength(100);
            _ = b.Property(x => x.SourceType).HasColumnName("source_type").IsRequired().HasMaxLength(100);
            _ = b.Property(x => x.SourceReference).HasColumnName("source_reference").IsRequired().HasMaxLength(200);
            _ = b.Property(x => x.SourceText).HasColumnName("source_text");
            _ = b.Property(x => x.ProposedPayloadJson).HasColumnName("proposed_payload").IsRequired();
            _ = b.Property(x => x.FieldConfidencesJson).HasColumnName("field_confidences").IsRequired();
            _ = b.Property(x => x.Confidence).HasColumnName("confidence");
            _ = b.Property(x => x.Amount).HasColumnName("amount");
            _ = b.Property(x => x.Reasoning).HasColumnName("reasoning");
            _ = b.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(30);
            _ = b.Property(x => x.Priority).HasColumnName("priority").IsRequired().HasMaxLength(20);
            _ = b.Property(x => x.ProviderUsed).HasColumnName("provider_used").HasMaxLength(100);
            _ = b.Property(x => x.ModelUsed).HasColumnName("model_used").HasMaxLength(100);
            _ = b.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            _ = b.Property(x => x.ServerCreatedAt).HasColumnName("server_created_at");
            _ = b.Property(x => x.QueuedDecision).HasColumnName("queued_decision").HasConversion<string>().HasMaxLength(20);
            _ = b.Property(x => x.QueuedFeedback).HasColumnName("queued_feedback");
            _ = b.Property(x => x.QueuedEditedPayloadJson).HasColumnName("queued_edited_payload");
            _ = b.Property(x => x.QueuedAt).HasColumnName("queued_at");
            _ = b.Property(x => x.QueuedAttempts).HasColumnName("queued_attempts");
            _ = b.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(1000);
            _ = b.Property(x => x.CachedAt).HasColumnName("cached_at");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.Status });
            _ = b.HasIndex(x => new { x.TenantId, x.QueuedDecision, x.QueuedAt });
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<AiInsightCache>(b =>
        {
            _ = b.ToTable("ai_insight_cache");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.PayloadJson).HasColumnName("payload").IsRequired();
            _ = b.Property(x => x.FetchedAt).HasColumnName("fetched_at");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => x.TenantId).IsUnique();
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
        });
    }
}
