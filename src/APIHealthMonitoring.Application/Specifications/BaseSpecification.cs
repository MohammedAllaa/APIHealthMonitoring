using System.Linq.Expressions;

namespace APIHealthMonitoring.Application.Specifications;

/// <summary>
/// Abstract base class for all specifications.
/// Implements <see cref="ISpecification{T}"/> with sensible defaults
/// and provides protected builder methods for constructing query rules.
/// Concrete specifications inherit from this class and call the
/// builder methods in their constructor.
/// </summary>
/// <typeparam name="T">The entity type this specification applies to.</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a specification with no filter criteria.
    /// Use this when you want all records (e.g., for a paginated list).
    /// </summary>
    protected BaseSpecification()
    {
    }

    /// <summary>
    /// Initializes a specification with a filter expression.
    /// </summary>
    /// <param name="criteria">The WHERE clause expression.</param>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    // -------------------------------------------------------------------------
    // ISpecification<T> Implementation — Properties
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    /// <inheritdoc/>
    public List<Expression<Func<T, object>>> Includes { get; }
        = new List<Expression<Func<T, object>>>();

    /// <inheritdoc/>
    public List<string> IncludeStrings { get; }
        = new List<string>();

    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <inheritdoc/>
    public int Skip { get; private set; }

    /// <inheritdoc/>
    public int Take { get; private set; }

    /// <inheritdoc/>
    public bool IsPagingEnabled { get; private set; }

    // -------------------------------------------------------------------------
    // Builder Methods — called by concrete specifications in their constructors
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets or overrides the filter expression (WHERE clause).
    /// </summary>
    /// <param name="criteria">The filter expression to apply.</param>
    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Adds a strongly-typed navigation property to eagerly load.
    /// Translates to an EF Core .Include() call.
    /// </summary>
    /// <param name="includeExpression">
    /// The navigation property expression, e.g. x => x.HealthCheckLogs.
    /// </param>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds a string-based include path for multi-level eager loading.
    /// Translates to an EF Core .Include(string) call.
    /// </summary>
    /// <param name="includeString">
    /// The dot-separated include path, e.g. "Category.HealthCheckLogs".
    /// </param>
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Applies ascending ordering to the query (ORDER BY ... ASC).
    /// </summary>
    /// <param name="orderByExpression">The property to order by.</param>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Applies descending ordering to the query (ORDER BY ... DESC).
    /// </summary>
    /// <param name="orderByDescendingExpression">The property to order by descending.</param>
    protected void ApplyOrderByDescending(
        Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    /// <summary>
    /// Enables pagination by setting Skip and Take values.
    /// Sets <see cref="IsPagingEnabled"/> to true automatically.
    /// </summary>
    /// <param name="skip">The number of records to skip (OFFSET).</param>
    /// <param name="take">The number of records to return (FETCH NEXT).</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}