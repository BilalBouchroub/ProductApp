namespace ProductApp.Application.Common;

public sealed record PageRequest(int Page = 1, int PageSize = 20, string? Search = null,
    string? SortBy = null, bool Descending = false)
{
    public int SafePage => Math.Max(1, Page);
    public int SafePageSize => Math.Clamp(PageSize, 1, 100);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
