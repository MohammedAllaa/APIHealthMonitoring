using APIHealthMonitoring.Application.Interfaces;
using APIHealthMonitoring.Application.Interfaces.Repositories;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Persistence.Data;
using APIHealthMonitoring.Persistence.Repositories;

namespace APIHealthMonitoring.Persistence.UnitOfWork;

/// <summary>
/// Concrete implementation of <see cref="IUnitOfWork"/>.
/// Coordinates a shared <see cref="AppDbContext"/> across multiple repositories
/// and provides a single atomic save operation.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    /// <summary>
    /// The shared EF Core database context used by all repositories
    /// created through this Unit of Work instance.
    /// </summary>
    private readonly AppDbContext _context;

    /// <summary>
    /// A cache of repository instances, keyed by entity type.
    /// Ensures each repository is created only once per Unit of Work lifetime
    /// and that all repositories share the same database context.
    /// </summary>
    private readonly Dictionary<Type, object> _repositories;

    /// <summary>
    /// Tracks whether Dispose has already been called to prevent
    /// double-disposal of the database context.
    /// </summary>
    private bool _disposed;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of <see cref="UnitOfWork"/>.
    /// </summary>
    /// <param name="context">
    /// The application database context injected by the DI container.
    /// </param>
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    // -------------------------------------------------------------------------
    // Repository Factory
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);

        if (!_repositories.ContainsKey(type))
        {
            var repositoryInstance = new GenericRepository<T>(_context);
            _repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<T>)_repositories[type];
    }

    // -------------------------------------------------------------------------
    // Commit
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Disposal
    // -------------------------------------------------------------------------

    /// <summary>
    /// Releases the database context and suppresses the finalizer.
    /// Called automatically by the DI container at the end of each request scope.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Core disposal logic, guarded against double-disposal.
    /// </summary>
    /// <param name="disposing">
    /// True when called from <see cref="Dispose()"/>;
    /// false when called from the finalizer.
    /// </param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }
    }
}