using APIHealthMonitoring.Application.Interfaces.Repositories;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace APIHealthMonitoring.Persistence.Repositories;

/// <summary>
/// Provides a generic, reusable implementation of <see cref="IGenericRepository{T}"/>
/// using Entity Framework Core. Handles standard CRUD and query operations
/// for any entity that inherits from <see cref="BaseEntity"/>.
/// </summary>
/// <typeparam name="T">
/// The entity type. Must inherit from <see cref="BaseEntity"/>.
/// </typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    /// <summary>
    /// The EF Core database context. All queries and commands flow through this.
    /// </summary>
    protected readonly AppDbContext _context;

    /// <summary>
    /// The EF Core DbSet for entity type T.
    /// Caching it here avoids repeated calls to _context.Set&lt;T&gt;().
    /// </summary>
    protected readonly DbSet<T> _dbSet;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of <see cref="GenericRepository{T}"/>.
    /// </summary>
    /// <param name="context">
    /// The application database context injected by the DI container.
    /// </param>
    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // -------------------------------------------------------------------------
    // Query Operations
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Command Operations
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    /// <inheritdoc/>
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <inheritdoc/>
    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    // -------------------------------------------------------------------------
    // Existence Check
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(e => e.Id == id, cancellationToken);
    }
}