using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Application.Interfaces.Repositories;

namespace APIHealthMonitoring.Application.Interfaces;

/// <summary>
/// Defines the contract for the Unit of Work pattern.
/// Coordinates the work of multiple repositories by sharing a single
/// database context and providing a single save operation to commit
/// all changes as one atomic transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // -------------------------------------------------------------------------
    // Repository Access
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the repository for the specified entity type.
    /// All repositories returned share the same underlying database context,
    /// ensuring that all changes participate in a single transaction.
    /// </summary>
    /// <typeparam name="T">
    /// The entity type. Must inherit from <see cref="BaseEntity"/>.
    /// </typeparam>
    IGenericRepository<T> Repository<T>() where T : BaseEntity;

    // -------------------------------------------------------------------------
    // Commit
    // -------------------------------------------------------------------------

    /// <summary>
    /// Persists all tracked changes across all repositories to the database
    /// as a single atomic operation.
    /// Returns the number of state entries written to the database.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}