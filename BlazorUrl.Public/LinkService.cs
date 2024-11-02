using BlazorUrl.Data;

namespace BlazorUrl.Public
{
    public interface ILinkService
    {
        Task<string?> GetLongUrlByShortCodeAsync(string shortCode);
    }

    public class LinkService : ILinkService
    {
        private readonly ApplicationDbContext _context;

        public LinkService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetLongUrlByShortCodeAsync(string shortCode)
        {
            var link = _context.Links.FirstOrDefault(x => x.ShortCode == shortCode);

            if (link is null)
            {
                return null;
            }

            var linkAnalytic = new LinkAnalytic
            {
                LinkId = link.Id,
                ClickedAt = DateTime.Now
            };

            _context.LinkAnalytics.Add(linkAnalytic);
            await _context.SaveChangesAsync();
            return link.LongUrl;
        }
    }
}
