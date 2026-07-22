
using ErrorOr;

public static class PaginationErrors
{
    public static Error InvalidPage(int page) => Error.Validation(
        code: "Pagination.InvalidPage",
        description: $"Page must be greater than 0, got {page}");

    public static Error InvalidPageSize(int size, int max) => Error.Validation(
        code: "Pagination.InvalidPageSize",
        description: $"Page size must be between 1 and {max}, got {size}");
}