namespace BlazorUrl.Client.Dtos
{
    public record PagedResult<TResult>(TResult[] Records, int TotalCount);
    
}
