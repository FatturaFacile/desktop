// <copyright file="LedgerGuardSql.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// The SQLite triggers guarding the ledger tables below the application: a posted entry and its lines never change and are never
/// deleted, an entry can only be posted when it balances, and a line is one sided and not negative. Money is stored as text by
/// EF Core, so the comparisons cast to REAL; the tolerance only absorbs binary rounding, the exact check is done in code.
/// </summary>
public static class LedgerGuardSql
{
    private const string PostedLine = "(SELECT is_locked FROM general_ledger_entries WHERE id = OLD.general_ledger_entry_id) = 1";

    private const string BadSign = "CAST(NEW.debit AS REAL) < 0 OR CAST(NEW.credit AS REAL) < 0 OR (CAST(NEW.debit AS REAL) > 0 AND CAST(NEW.credit AS REAL) > 0)";

    private const string LinesOfEntry = "FROM general_ledger_lines WHERE general_ledger_entry_id = OLD.id";

    private const string Unbalanced = "OLD.is_locked = 0 AND NEW.is_locked = 1 AND ((SELECT COUNT(*) " + LinesOfEntry + ") < 2"
        + " OR ABS(COALESCE((SELECT SUM(CAST(debit AS REAL) - CAST(credit AS REAL)) " + LinesOfEntry + "), 0)) > 0.00005"
        + " OR COALESCE((SELECT SUM(CAST(debit AS REAL)) " + LinesOfEntry + "), 0) <= 0)";

    /// <summary>
    /// Gets the statements creating the triggers.
    /// </summary>
    public static IReadOnlyList<string> Create { get; } =
    [
        Trigger("trg_gl_entry_posted_no_update", "BEFORE UPDATE ON general_ledger_entries", "OLD.is_locked = 1", "A posted journal entry is immutable: reverse it instead."),
        Trigger("trg_gl_entry_posted_no_delete", "BEFORE DELETE ON general_ledger_entries", "OLD.is_locked = 1", "A posted journal entry cannot be deleted: reverse it instead."),
        Trigger("trg_gl_entry_post_requires_balance", "BEFORE UPDATE OF is_locked ON general_ledger_entries", Unbalanced, "A journal entry must balance (debit equals credit, total above zero) to be posted."),
        Trigger("trg_gl_line_sign_insert", "BEFORE INSERT ON general_ledger_lines", BadSign, "A ledger line is a non-negative debit or a non-negative credit, never both."),
        Trigger("trg_gl_line_sign_update", "BEFORE UPDATE OF debit, credit ON general_ledger_lines", BadSign, "A ledger line is a non-negative debit or a non-negative credit, never both."),
        Trigger("trg_gl_line_posted_no_update", "BEFORE UPDATE ON general_ledger_lines", PostedLine, "The lines of a posted journal entry are immutable."),
        Trigger("trg_gl_line_posted_no_delete", "BEFORE DELETE ON general_ledger_lines", PostedLine, "The lines of a posted journal entry cannot be deleted."),
    ];

    /// <summary>
    /// Gets the statements dropping the triggers.
    /// </summary>
    public static IReadOnlyList<string> Drop { get; } =
    [
        "DROP TRIGGER IF EXISTS trg_gl_entry_posted_no_update;",
        "DROP TRIGGER IF EXISTS trg_gl_entry_posted_no_delete;",
        "DROP TRIGGER IF EXISTS trg_gl_entry_post_requires_balance;",
        "DROP TRIGGER IF EXISTS trg_gl_line_sign_insert;",
        "DROP TRIGGER IF EXISTS trg_gl_line_sign_update;",
        "DROP TRIGGER IF EXISTS trg_gl_line_posted_no_update;",
        "DROP TRIGGER IF EXISTS trg_gl_line_posted_no_delete;",
    ];

    private static string Trigger(string name, string timing, string condition, string message) =>
        "CREATE TRIGGER IF NOT EXISTS " + name + " " + timing + " WHEN " + condition + " BEGIN SELECT RAISE(ABORT, '" + message + "'); END;";
}
