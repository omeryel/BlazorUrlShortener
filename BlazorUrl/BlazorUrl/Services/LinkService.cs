using BlazorUrl.Client.Dtos;
using BlazorUrl.Client.Interfaces;
using BlazorUrl.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlazorUrl.Services
{
    public class LinkService : ILinkService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IShortCodeGeneratorService _shortCodeGeneratorService;
        private readonly IConfiguration _configuration;

        public LinkService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IShortCodeGeneratorService shortCodeGeneratorService, IConfiguration configuration)
        {
            _dbContextFactory = dbContextFactory;
            _shortCodeGeneratorService = shortCodeGeneratorService;
            _configuration = configuration;
        }

        public async Task<LinkDto> CreateLinkAsync(LinkCreateDto linkCreateDto)
        {
            var domain = _configuration["Domain"] ?? throw new InvalidOperationException($"Domain is not defined in appsettings.json");
            var shortCode = await _shortCodeGeneratorService.GenerateShortCodeAsync();

            var link = new Link
            {
                LongUrl = linkCreateDto.LongUrl,
                ShortCode = shortCode,
                IsActive = true,
                UserId = linkCreateDto.UserId,
                ShortUrl = $"{domain.TrimEnd('/')}/{shortCode}",
                CreatedOn = DateTime.Now,
            };

            await using var context = _dbContextFactory.CreateDbContext();

            await context.Links.AddAsync(link);
            await context.SaveChangesAsync();

            return new LinkDto
            {
                Id = link.Id,
                LongUrl = linkCreateDto.LongUrl,
                IsActive = true,
                ShortUrl = link.ShortUrl
            };

        }

        public async Task DeleteLinkAsync(long id, string userId)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            var link = await context.Links.Include(l => l.LinkAnalytics).FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

            if (link is null)
            {
                return;
            }
            if (link.LinkAnalytics.Count > 0)
            {
                context.LinkAnalytics.RemoveRange(link.LinkAnalytics);
            }
            context.Links.Remove(link);

            await context.SaveChangesAsync();
        }

        public async Task<DashboardDataDto> GetDashboardDataAsync(string userId)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            //var totalLinks = await GetTotalLinks(userId);
            //var totalClicks = await GetTotalClicks(userId);
            //var totalActiveLinks = await GetTotaActivelLinks(userId);
            //var totalInactiveLinks = totalLinks - totalActiveLinks;
            //var totalLinksToday = await GetTotalLinksToday(userId);
            //var totalClicksToday = await GetTotalClicksToday(userId);

            var counts = await Task.WhenAll(
                GetTotalLinks(userId),
                GetTotalClicks(userId),
                GetTotaActivelLinks(userId),
                GetTotalLinksToday(userId),
                GetTotalClicksToday(userId));

            var totalLinks = counts[0];
            var totalClicks = counts[1];
            var totalActiveLinks = counts[2];
            var totalInactiveLinks = totalLinks - totalActiveLinks;
            var totalLinksToday = counts[3];
            var totalClicksToday = counts[4];



            return new DashboardDataDto(totalLinks, totalClicks, totalActiveLinks, totalInactiveLinks, totalLinksToday, totalClicksToday);
        }

        private async Task<int> GetTotalLinks(string userId)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.Links.CountAsync(a => a.UserId == userId);
        }

        private async Task<int> GetTotalClicks(string userId)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.LinkAnalytics.CountAsync(a => a.Link.UserId == userId);
        }

        private async Task<int> GetTotaActivelLinks(string userId)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.Links.CountAsync(x => x.UserId == userId && x.IsActive);
        }
        private async Task<int> GetTotalLinksToday(string userId)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.Links.CountAsync(x => x.CreatedOn.Date == DateTime.Today);
        }
        private async Task<int> GetTotalClicksToday(string userId)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.LinkAnalytics.CountAsync(x => x.ClickedAt.Date == DateTime.Today);
        }




        public async Task<LinkDetailsDto?> GetLinkAsync(long id, string userId)
        {
            await using var context = _dbContextFactory.CreateDbContext();

            var link = await context.Links.Include(x => x.LinkAnalytics).FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

            if (link is null)
            {
                return null;
            }

            LinkAnalyticDto[] linkAnalytics = link.LinkAnalytics.Select(x => new LinkAnalyticDto
            {
                Id = x.Id,
                ClickedAt = x.ClickedAt,
                LinkId = x.LinkId,
            }).ToArray() ?? [];

            var linkDto = new LinkDto
            {
                Id = id,
                IsActive = link.IsActive,
                LongUrl = link.LongUrl,
                ShortUrl = link.ShortUrl,
                TotalClicks = linkAnalytics.Length,
            };
            return new LinkDetailsDto(linkDto, linkAnalytics);
        }

        public async Task<PagedResult<LinkDto>> GetLinksByUserAsync(string userId, int startIndex, int pageSize, bool activeOnly)
        {
            await using var context = _dbContextFactory.CreateDbContext();

            var query = context.Links.Where(x => x.UserId == userId);
            if (activeOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            var totalCount = await query.CountAsync();
            var links = await query.OrderByDescending(x => x.Id).Skip(startIndex).Take(pageSize)
                .Select(x => new LinkDto
                {
                    Id = x.Id,
                    LongUrl = x.LongUrl,
                    ShortUrl = x.ShortUrl,
                    IsActive = x.IsActive,
                    TotalClicks = x.LinkAnalytics.Count
                }).ToArrayAsync();

            return new PagedResult<LinkDto>(links, totalCount);
        }

        public async Task<LinkDto?> UpdateLinkAsync(LinkEditDto linkEditDto)
        {
            await using var context = _dbContextFactory.CreateDbContext();

            var dbLink = await context.Links
                .Include(l => l.LinkAnalytics)
                .FirstOrDefaultAsync(l => l.Id == linkEditDto.Id && l.UserId == linkEditDto.UserId);

            if (dbLink is null)
            {
                return null;
            }

            dbLink.LongUrl = linkEditDto.LongUrl;
            dbLink.IsActive = linkEditDto.IsActive;
            context.Links.Update(dbLink);

            await context.SaveChangesAsync();

            return new LinkDto
            {
                Id = linkEditDto.Id,
                LongUrl = linkEditDto.LongUrl,
                ShortUrl = dbLink.ShortUrl,
                TotalClicks = dbLink.LinkAnalytics.Count
            };
        }
    }
}
