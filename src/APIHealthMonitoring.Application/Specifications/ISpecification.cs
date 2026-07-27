using System.Linq.Expressions;

namespace APIHealthMonitoring.Application.Specifications;

/// <summary>
/// Defines the contract for the Specification pattern.
/// A specification encapsulates a query rule — including filtering,
/// ordering, eager loading, and pagination — as a reusable, testable object.
/// </summary>
/// <typeparam name="T">The entity type this specification applies to.</typeparam>
public interface ISpecification<T>
{
    // -------------------------------------------------------------------------
    // Filtering
    // -------------------------------------------------------------------------

    /// <summary>
    /// The filter expression applied as a WHERE clause.
    /// Null means no filtering — all records are returned.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    // -------------------------------------------------------------------------
    // Eager Loading
    // -------------------------------------------------------------------------

    /// <summary>
    /// A list of navigation property expressions to eagerly load (JOIN).
    /// Each entry translates to an EF Core .Include() call.
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// A list of string-based include paths for multi-level eager loading.
    /// Each entry translates to an EF Core .Include(string) call.
    /// Example: "MonitoredApi.HealthCheckLogs.Details"
    /// </summary>
    List<string> IncludeStrings { get; }

    // -------------------------------------------------------------------------
    // Ordering
    // -------------------------------------------------------------------------

    /// <summary>
    /// The primary ordering expression (ORDER BY ... ASC).
    /// Null means no ordering is applied.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// The primary ordering expression in descending order (ORDER BY ... DESC).
    /// Null means no descending ordering is applied.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    // -------------------------------------------------------------------------
    // Pagination
    // -------------------------------------------------------------------------

    /// <summary>
    /// The number of records to skip (OFFSET).
    /// Used in combination with <see cref="Take"/> for pagination.
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// The number of records to take (FETCH NEXT).
    /// Used in combination with <see cref="Skip"/> for pagination.
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Indicates whether pagination (Skip/Take) should be applied.
    /// When false, Skip and Take are ignored even if set.
    /// </summary>
    bool IsPagingEnabled { get; }
}