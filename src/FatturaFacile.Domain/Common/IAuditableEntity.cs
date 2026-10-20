namespace FatturaFacile.Domain.Common;

/// <summary>
/// Audit trail tracking for created/modified timestamps and actors.
/// </summary>
public interface IAuditableEntity
{
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}