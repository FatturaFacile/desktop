// <copyright file="ProjectModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Projects;

/// <summary>
/// Maps the offline project portfolio and WBS tables (Mod 078).
/// </summary>
public static class ProjectModelConfiguration
{
    /// <summary>
    /// Maps the project portfolio entities.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureProjects(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<Project>(b =>
        {
            _ = b.ToTable("projects");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").HasMaxLength(36);
            _ = b.Property(x => x.ProjectCode).HasColumnName("project_code").HasMaxLength(64).IsRequired();
            _ = b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            _ = b.Property(x => x.CustomerId).HasColumnName("customer_id").HasMaxLength(36);
            _ = b.Property(x => x.ProjectManagerId).HasColumnName("project_manager_id").HasMaxLength(36);
            _ = b.Property(x => x.ProjectType).HasColumnName("project_type").HasConversion<int>();
            _ = b.Property(x => x.BillingModel).HasColumnName("billing_model").HasConversion<int>();
            _ = b.Property(x => x.CostCenterId).HasColumnName("cost_center_id").HasMaxLength(36);
            _ = b.Property(x => x.StartDate).HasColumnName("start_date");
            _ = b.Property(x => x.TargetEndDate).HasColumnName("target_end_date");
            _ = b.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
            _ = b.Property(x => x.CupCode).HasColumnName("cup_code").HasMaxLength(15);
            _ = b.Property(x => x.CigCode).HasColumnName("cig_code").HasMaxLength(10);
            _ = b.Property(x => x.PlannedHours).HasColumnName("planned_hours");
            _ = b.Property(x => x.ActualHours).HasColumnName("actual_hours");
            _ = b.Property(x => x.PlannedCost).HasColumnName("planned_cost");
            _ = b.Property(x => x.ActualCost).HasColumnName("actual_cost");
            _ = b.Property(x => x.PlannedRevenue).HasColumnName("planned_revenue");
            _ = b.Property(x => x.PercentComplete).HasColumnName("percent_complete");
            _ = b.Property(x => x.IsTemplate).HasColumnName("is_template");
            _ = b.Property(x => x.TemplateSourceId).HasColumnName("template_source_id").HasMaxLength(36);
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.ProjectCode }).IsUnique();
            _ = b.HasIndex(x => new { x.TenantId, x.Status });
        });

        _ = modelBuilder.Entity<WbsNode>(b =>
        {
            _ = b.ToTable("wbs_nodes");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.ProjectId).HasColumnName("project_id").HasMaxLength(36);
            _ = b.Property(x => x.ParentNodeId).HasColumnName("parent_node_id").HasMaxLength(36);
            _ = b.Property(x => x.WbsCode).HasColumnName("wbs_code").HasMaxLength(64).IsRequired();
            _ = b.Property(x => x.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
            _ = b.Property(x => x.NodeType).HasColumnName("node_type").HasConversion<int>();
            _ = b.Property(x => x.PlannedHours).HasColumnName("planned_hours");
            _ = b.Property(x => x.ActualHours).HasColumnName("actual_hours");
            _ = b.Property(x => x.PlannedCost).HasColumnName("planned_cost");
            _ = b.Property(x => x.ActualCost).HasColumnName("actual_cost");
            _ = b.Property(x => x.PlannedRevenue).HasColumnName("planned_revenue");
            _ = b.Property(x => x.StartDate).HasColumnName("start_date");
            _ = b.Property(x => x.EndDate).HasColumnName("end_date");
            _ = b.Property(x => x.PercentComplete).HasColumnName("percent_complete");
            _ = b.Property(x => x.SortOrder).HasColumnName("sort_order");
            _ = b.Property(x => x.EarlyStart).HasColumnName("early_start");
            _ = b.Property(x => x.EarlyFinish).HasColumnName("early_finish");
            _ = b.Property(x => x.LateStart).HasColumnName("late_start");
            _ = b.Property(x => x.LateFinish).HasColumnName("late_finish");
            _ = b.Property(x => x.TotalFloatDays).HasColumnName("total_float_days");
            _ = b.Property(x => x.FreeFloatDays).HasColumnName("free_float_days");
            _ = b.Property(x => x.IsCritical).HasColumnName("is_critical");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.ProjectId, x.WbsCode }).IsUnique();
            _ = b.HasIndex(x => new { x.TenantId, x.ProjectId, x.ParentNodeId });
        });

        _ = modelBuilder.Entity<TaskDependency>(b =>
        {
            _ = b.ToTable("task_dependencies");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.ProjectId).HasColumnName("project_id").HasMaxLength(36);
            _ = b.Property(x => x.PredecessorNodeId).HasColumnName("predecessor_node_id").HasMaxLength(36);
            _ = b.Property(x => x.SuccessorNodeId).HasColumnName("successor_node_id").HasMaxLength(36);
            _ = b.Property(x => x.DependencyType).HasColumnName("dependency_type").HasConversion<int>();
            _ = b.Property(x => x.LagDays).HasColumnName("lag_days");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.ProjectId, x.PredecessorNodeId, x.SuccessorNodeId }).IsUnique();
        });

        _ = modelBuilder.Entity<ProjectBaseline>(b =>
        {
            _ = b.ToTable("project_baselines");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.ProjectId).HasColumnName("project_id").HasMaxLength(36);
            _ = b.Property(x => x.BaselineNumber).HasColumnName("baseline_number");
            _ = b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            _ = b.Property(x => x.CapturedAt).HasColumnName("captured_at");
            _ = b.Property(x => x.SnapshotJson).HasColumnName("snapshot_json").IsRequired();
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.ProjectId, x.BaselineNumber }).IsUnique();
        });

        _ = modelBuilder.Entity<ProjectOutboxEntry>(b =>
        {
            _ = b.ToTable("project_outbox");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.Action).HasColumnName("action").HasConversion<int>();
            _ = b.Property(x => x.ProjectId).HasColumnName("project_id").HasMaxLength(36);
            _ = b.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(36);
            _ = b.Property(x => x.ProjectCode).HasColumnName("project_code").HasMaxLength(64).IsRequired();
            _ = b.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
            _ = b.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(36);
            _ = b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at");
            _ = b.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
            _ = b.Property(x => x.Attempts).HasColumnName("attempts");
            _ = b.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
            _ = b.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
            _ = b.Property(x => x.ResultJson).HasColumnName("result_json");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            _ = b.HasIndex(x => new { x.TenantId, x.Status, x.OccurredAtUtc });
            _ = b.HasIndex(x => new { x.TenantId, x.ProjectId });
        });
    }
}
