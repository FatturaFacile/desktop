namespace FatturaFacile.Domain.Entities.Purchasing;

/// <summary>
/// Defines the lifecycle states of an ISO 20022 payment instruction.
/// </summary>
public enum PaymentInstructionStatus
{
    /// <summary>
    /// Instruction has been created but not yet exported.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Instruction has been exported to a pain.001 file.
    /// </summary>
    GeneratedPain001 = 1,

    /// <summary>
    /// Instruction has been executed by the bank.
    /// </summary>
    Executed = 2,

    /// <summary>
    /// Instruction has been rejected by the bank.
    /// </summary>
    Rejected = 3,
}
