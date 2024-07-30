namespace API.Helpers;

public class PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPage)
{
    public int CurrentPage { get; } = currentPage;
    public int ItemsPerPage { get; } = itemsPerPage;
    public int TotalItems { get; } = totalItems;
    public int TotalPage { get; } = totalPage;
}
