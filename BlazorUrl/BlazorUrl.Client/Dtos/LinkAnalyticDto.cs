namespace BlazorUrl.Client.Dtos
{
    public class LinkAnalyticDto
    {
        public long Id { get; set; }
        public long LinkId { get; set; }
        public DateTime ClickedAt { get; set; }
    }
}
