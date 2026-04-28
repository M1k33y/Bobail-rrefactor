namespace Bobail.Application.DTOs;

public class PagedGameHistoryResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public List<GameHistoryResponse> Items { get; set; } = new();
}
