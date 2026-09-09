using ProductApp.Application.Common;

namespace ProductApp.Application.Tests.Common;

public sealed class PaginationTests
{
    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-5, 500, 1, 100)]
    [InlineData(3, 25, 3, 25)]
    public void Page_request_bounds_client_values(int page, int size, int expectedPage, int expectedSize)
    {
        var request = new PageRequest(page, size);
        Assert.Equal(expectedPage, request.SafePage);
        Assert.Equal(expectedSize, request.SafePageSize);
    }

    [Fact]
    public void Paged_result_calculates_total_pages()
    {
        var result = new PagedResult<int>([1, 2], 2, 10, 21);
        Assert.Equal(3, result.TotalPages);
    }
}
