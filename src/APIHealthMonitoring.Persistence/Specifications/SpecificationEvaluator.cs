using APIHealthMonitoring.Application.Specifications;
using APIHealthMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIHealthMonitoring.Persistence.Specifications;

/// <summary>
/// Translates an <see cref="ISpecification{T}"/> into an EF Core
/// <see cref="IQueryable{T}"/> pipeline by applying criteria, includes,
/// ordering, and pagination in the correct sequence.
/// This class is the bridge between the Application layer's query definitions
/// and the Persistence layer's EF Core execution engine.
/// </summary>
/// <typeparam name="T">
/// The entity type. Must inherit from <see cref="BaseEntity"/>.
/// </typeparam>
public static class SpecificationEvaluator<T> where T : BaseEntity
{
    /// <summary>
    /// Builds a complete <see cref="IQueryable{T}"/> from a base query and a specification.
    /// Each feature of the specification is applied in the correct order:
    /// filtering → includes → ordering → pagination.
    /// </summary>
    /// <param name="inputQuery">
    /// The initial queryable, typically <c>DbSet&lt;T&gt;.AsQueryable()</c>.
    /// No SQL is executed until the query is materialized (e.g., via ToListAsync).
    /// </param>
    /// <param name="specification">
    /// The specification containing the query rules to apply.
    /// </param>
    /// <returns>
    /// A fully composed <see cref="IQueryable{T}"/> ready to be executed
    /// against the database.
    /// </returns>
    public static IQueryable<T> GetQuery(
        IQueryable<T> inputQuery,
        ISpecification<T> specification)
    {
        var query = inputQuery;

        // Step 1 — Apply filter (WHERE clause)
        // Must be applied before ordering and paging to avoid
        // filtering on an already-sliced dataset.
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Step 2 — Apply strongly-typed includes (JOINs / eager loading)
        // Aggregates all Include expressions into the query pipeline.
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Step 3 — Apply string-based includes for multi-level navigation
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Step 4 — Apply ordering
        // OrderBy and OrderByDescending are mutually exclusive.
        // Only one will be applied; OrderBy takes precedence.
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Step 5 — Apply pagination (OFFSET / FETCH NEXT)
        // Must be applied last — after filtering and ordering —
        // to ensure the correct page of the correct sorted dataset is returned.
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip)
                         .Take(specification.Take);
        }

        return query;
    }
}