using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Defines the contract for user repository.
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{
    /// <summary>
    /// Get By Username Async.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

/// <summary>
    /// Get By Email Async.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
