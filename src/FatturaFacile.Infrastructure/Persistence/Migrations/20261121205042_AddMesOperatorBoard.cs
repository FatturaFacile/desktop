using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FatturaFacile.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261121205042_AddMesOperatorBoard : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "mes_boards",
            columns: table => new
            {
                id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                tenant_id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                work_center_id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                board_json = table.Column<string>(type: "TEXT", nullable: false),
                synced_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                created_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_mes_boards", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "mes_outbox",
            columns: table => new
            {
                id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                tenant_id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                action = table.Column<int>(type: "INTEGER", nullable: false),
                operation_id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                phase_id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: true),
                payload_json = table.Column<string>(type: "TEXT", nullable: false),
                idempotency_key = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                occurred_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                status = table.Column<int>(type: "INTEGER", nullable: false),
                attempts = table.Column<int>(type: "INTEGER", nullable: false),
                next_attempt_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                last_error = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                created_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_mes_outbox", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_mes_boards_tenant_id_work_center_id",
            table: "mes_boards",
            columns: new[] { "tenant_id", "work_center_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_mes_outbox_tenant_id_idempotency_key",
            table: "mes_outbox",
            columns: new[] { "tenant_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_mes_outbox_tenant_id_status_occurred_at",
            table: "mes_outbox",
            columns: new[] { "tenant_id", "status", "occurred_at" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "mes_boards");

        migrationBuilder.DropTable(
            name: "mes_outbox");
    }
}
