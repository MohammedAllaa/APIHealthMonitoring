namespace APIHealthMonitoring.Application.Specifications;

/// <summary>
/// A standardized envelope for paginated query results.
/// Carries the current page of data alongside all metadata
/// needed by the client to render pagination controls
/// and make subsequent page requests.
/// </summary>
/// <typeparam name="T">The type of items in the paginated result.</typeparam>
public class PaginatedResult<T>
{
    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of <see cref="PaginatedResult{T}"/>.
    /// </summary>
    /// <param name="data">The items for the current page.</param>
    /// <param name="totalCount">
    /// The total number of records matching the filter,
    /// regardless of pagination. Used to calculate total pages.
    /// </param>
    /// <param name="pageIndex">
    /// The current page number, one-based (first page = 1).
    /// </param>
    /// <param name="pageSize">
    /// The maximum number of items per page.
    /// </param>
    public PaginatedResult(
        IReadOnlyList<T> data,
        int totalCount,
        int pageIndex,
        int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    // -------------------------------------------------------------------------
    // Data
    // -------------------------------------------------------------------------

    /// <summary>
    /// The items returned for the current page.
    /// </summary>
    public IReadOnlyList<T> Data { get; }

    // -------------------------------------------------------------------------
    // Pagination Metadata
    // -------------------------------------------------------------------------

    /// <summary>
    /// The total number of records matching the applied filter,
    /// across all pages combined.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// The current page number (one-based).
    /// Page 1 is the first page.
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// The maximum number of records per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// The total number of pages available, calculated from
    /// <see cref="TotalCount"/> and <see cref="PageSize"/>.
    /// Uses ceiling division to ensure a partial last page is counted.
    /// </summary>
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;

    // -------------------------------------------------------------------------
    // Navigation Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Indicates whether a previous page exists.
    /// Clients use this to enable or disable a "Previous" button.
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// Indicates whether a next page exists.
    /// Clients use this to enable or disable a "Next" button.
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;
}