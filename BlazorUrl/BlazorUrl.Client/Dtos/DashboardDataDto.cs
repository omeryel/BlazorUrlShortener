namespace BlazorUrl.Client.Dtos
{

    public record DashboardDataDto(int TotalLinks, int TotalClicks, int TotalActiveLinks, int TotalInactiveLinks,
        int TotalLinkCreatedToday, int TotalClicksToday);
    
    
}
