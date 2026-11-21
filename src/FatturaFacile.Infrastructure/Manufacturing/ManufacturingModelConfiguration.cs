// <copyright file="ManufacturingModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <summary>
/// EF Core mapping of the Mod 061 manufacturing cache (tables `mfg_*`). The tenant filters are added by the database context itself
/// (they must read the tenant service of each context instance).
/// </summary>
public static class ManufacturingModelConfiguration
{
    private const int IdLength = 36;

    /// <summary>
    /// Maps the manufacturing tables.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureManufacturing(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<MfgWorkCenter>(b =>
        {
            Base(b, "mfg_work_centers");
            _ = b.Property(x => x.WorkCenterCode).HasColumnName("work_center_code").IsRequired().HasMaxLength(50);
            _ = b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            _ = b.Property(x => x.Description).HasColumnName("description");
            _ = b.Property(x => x.DepartmentCode).HasColumnName("department_code").HasMaxLength(50);
            _ = b.Property(x => x.CurrencyCode).HasColumnName("currency_code").IsRequired().HasMaxLength(3);
            _ = b.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            _ = b.Property(x => x.AlternateWorkCenterIdsJson).HasColumnName("alternate_work_center_ids").IsRequired();
            _ = b.Property(x => x.WeeklyHoursJson).HasColumnName("weekly_hours").IsRequired();
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.HasIndex(x => new { x.TenantId, x.WorkCenterCode });
        });

        _ = modelBuilder.Entity<MfgMachineCenter>(b =>
        {
            Base(b, "mfg_machine_centers");
            _ = b.Property(x => x.MachineCode).HasColumnName("machine_code").IsRequired().HasMaxLength(50);
            _ = b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            _ = b.Property(x => x.TelemetryStatus).HasColumnName("telemetry_status").IsRequired().HasMaxLength(20);
            _ = b.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.HasIndex(x => new { x.TenantId, x.WorkCenterId });
        });

        _ = modelBuilder.Entity<MfgWorkingCalendar>(b =>
        {
            Base(b, "mfg_working_calendars");
            _ = b.Property(x => x.Code).HasColumnName("code").IsRequired().HasMaxLength(50);
            _ = b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            _ = b.Property(x => x.Timezone).HasColumnName("timezone").IsRequired().HasMaxLength(64);
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.HasMany(x => x.Shifts).WithOne().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Cascade);
            _ = b.HasMany(x => x.Closures).WithOne().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Cascade);
        });

        _ = modelBuilder.Entity<MfgCalendarShift>(b =>
        {
            Base(b, "mfg_calendar_shifts");
            _ = b.Property(x => x.ShiftCode).HasColumnName("shift_code").IsRequired().HasMaxLength(50);
            _ = b.Property(x => x.StartTime).HasColumnName("start_time").IsRequired().HasMaxLength(8);
            _ = b.Property(x => x.EndTime).HasColumnName("end_time").IsRequired().HasMaxLength(8);
        });

        _ = modelBuilder.Entity<MfgCalendarClosure>(b =>
        {
            Base(b, "mfg_calendar_closures");
            _ = b.Property(x => x.Reason).HasColumnName("reason");
        });

        _ = modelBuilder.Entity<MfgRouting>(b =>
        {
            Base(b, "mfg_routings");
            _ = b.Property(x => x.RoutingNumber).HasColumnName("routing_number").IsRequired().HasMaxLength(50);
            _ = b.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
            _ = b.Property(x => x.ItemName).HasColumnName("item_name").HasMaxLength(200);
            _ = b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            _ = b.Property(x => x.Revision).HasColumnName("revision").IsRequired().HasMaxLength(20);
            _ = b.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            _ = b.Property(x => x.Notes).HasColumnName("notes");
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.HasIndex(x => new { x.TenantId, x.RoutingNumber, x.Revision });
            _ = b.HasMany(x => x.Operations).WithOne().HasForeignKey(x => x.RoutingId).OnDelete(DeleteBehavior.Cascade);
        });

        _ = modelBuilder.Entity<MfgRoutingOperation>(b =>
        {
            Base(b, "mfg_routing_operations");
            _ = b.Property(x => x.OperationCode).HasColumnName("operation_code").IsRequired().HasMaxLength(50);
            _ = b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            _ = b.Property(x => x.OperationType).HasColumnName("operation_type").IsRequired().HasMaxLength(30);
            _ = b.Property(x => x.AlternateWorkCenterIdsJson).HasColumnName("alternate_work_center_ids").IsRequired();
            _ = b.HasMany(x => x.BomComponents).WithOne().HasForeignKey(x => x.OperationId).OnDelete(DeleteBehavior.Cascade);
        });

        _ = modelBuilder.Entity<MfgRoutingOperationBomLink>(b =>
        {
            Base(b, "mfg_routing_operation_boms");
            _ = b.Property(x => x.Notes).HasColumnName("notes");
        });

        _ = modelBuilder.Entity<MfgOeeSnapshot>(b =>
        {
            Base(b, "mfg_oee_snapshots");
            _ = b.Property(x => x.SixBigLossesJson).HasColumnName("six_big_losses").IsRequired();
            _ = b.Property(x => x.MachinesJson).HasColumnName("machines").IsRequired();
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.HasIndex(x => new { x.TenantId, x.WorkCenterId });
        });
    }

    private static void Base<T>(EntityTypeBuilder<T> b, string table)
        where T : class, ITenantScopedEntity
    {
        _ = b.ToTable(table);
        _ = b.HasKey(x => x.Id);
        _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(IdLength).ValueGeneratedNever();
        _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(IdLength);
        _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
        _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
