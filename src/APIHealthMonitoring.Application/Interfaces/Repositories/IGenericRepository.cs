using APIHealthMonitoring.Application.Specifications;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Interfaces.Repositories;

/// <summary>
/// Defines the contract for a generic repository that provides standard
/// CRUD and query operations for any domain entity.
/// </summary>
/// <typeparam name="T">
/// The entity type. Must inherit from <see cref="BaseEntity"/>
/// to guarantee it has an Id and audit fields.
/// </typeparam>
public interface IGenericRepository<T> where T : BaseEntity
{
    // -------------------------------------------------------------------------
    // Query Operations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retrieves an entity by its primary key.
    /// Returns null if no entity with the given Id exists.
    /// </summary>
    /// <param name="id">The primary key value to search for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities of type T from the database.
    /// Use with caution on large tables — prefer specification-based queries.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Command Operations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Marks the entity for insertion into the database.
    /// The record is not written until SaveChangesAsync is called on the Unit of Work.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    void Add(T entity);

    /// <summary>
    /// Marks the entity for update in the database.
    /// The record is not written until SaveChangesAsync is called on the Unit of Work.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(T entity);

    /// <summary>
    /// Marks the entity for deletion from the database.
    /// The record is not removed until SaveChangesAsync is called on the Unit of Work.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(T entity);

    // -------------------------------------------------------------------------
    // Existence Check
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines whether an entity with the given primary key exists.
    /// More efficient than GetByIdAsync when you only need to check existence.
    /// </summary>
    /// <param name="id">The primary key value to check.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Specification-Based Query Operations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a single entity that satisfies the given specification,
    /// or null if no match is found.
    /// Use this when you expect at most one result (e.g., get by unique field).
    /// </summary>
    /// <param name="specification">The specification defining the query rules.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task<T?> GetEntityWithSpecAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities that satisfy the given specification.
    /// Supports filtering, ordering, eager loading, and pagination.
    /// </summary>
    /// <param name="specification">The specification defining the query rules.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task<IReadOnlyList<T>> GetAllWithSpecAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total count of entities that satisfy the given specification.
    /// Used to calculate total pages for pagination — runs a COUNT query,
    /// not a full data fetch.
    /// </summary>
    /// <param name="specification">The specification defining the filter rules.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);
}