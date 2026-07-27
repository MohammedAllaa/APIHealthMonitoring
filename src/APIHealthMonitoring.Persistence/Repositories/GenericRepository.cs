using APIHealthMonitoring.Application.Interfaces.Repositories;
using APIHealthMonitoring.Application.Specifications;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Persistence.Data;
using APIHealthMonitoring.Persistence.Specifications;
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

    // -------------------------------------------------------------------------
    // Specification-Based Query Operations
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<T?> GetEntityWithSpecAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        // For counting, we apply ONLY the filter criteria.
        // Ordering, includes, and pagination are irrelevant for a COUNT query
        // and would generate unnecessary SQL overhead.
        return await ApplySpecificationForCount(specification)
            .CountAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies the full specification (filter, includes, ordering, pagination)
    /// to the base DbSet query using the SpecificationEvaluator.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    private IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        return SpecificationEvaluator<T>.GetQuery(
            _dbSet.AsQueryable(),
            specification);
    }

    /// <summary>
    /// Applies ONLY the filter criteria of the specification to the base query.
    /// Used exclusively for COUNT queries where ordering, includes,
    /// and pagination must not be applied.
    /// </summary>
    /// <param name="specification">The specification whose criteria to apply.</param>
    private IQueryable<T> ApplySpecificationForCount(ISpecification<T> specification)
    {
        var query = _dbSet.AsQueryable();

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        return query;
    }
}