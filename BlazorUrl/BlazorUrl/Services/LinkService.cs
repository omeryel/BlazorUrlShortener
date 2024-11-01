using BlazorUrl.Client.Dtos;
using BlazorUrl.Client.Interfaces;
using BlazorUrl.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

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

        public async Task<PagedResult<LinkDto>> GetLinksByUserAsync(string userId, int startIndex, int pageSize, bool activeOnly)
        {
            await using var context = _dbContextFactory.CreateDbContext();

            var query = context.Links.Where(x => x.UserId == userId);
            if(activeOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            var totalCount = await query.CountAsync();
            var links = await query.Skip(startIndex).Take(pageSize).
                Select(x => new LinkDto
                {
                    Id=x.Id,
                    LongUrl=x.LongUrl,
                    ShortUrl=x.ShortUrl,
                    IsActive=x.IsActive,
                    TotalClicks=x.LinkAnalytics.Count
                }).ToArrayAsync();

            return new PagedResult<LinkDto>(links, totalCount);
        }

        public async Task<LinkDto?> UpdateLinkAsync(LinkEditDto linkEditDto)
        {
            await using var context = _dbContextFactory.CreateDbContext();

            var dbLink = await context.Links
                .Include(l => l.LinkAnalytics)
                .FirstOrDefaultAsync(l => l.Id == linkEditDto.Id && l.UserId == linkEditDto.UserId);

            if(dbLink is null)
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
