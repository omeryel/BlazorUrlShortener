using BlazorUrl.Data;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace BlazorUrl.Services
{
    public class ShortCodeGeneratorService : IShortCodeGeneratorService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public ShortCodeGeneratorService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _dbContextFactory = contextFactory;
        }
        public async Task<string> GenerateShortCodeAsync()
        {
            var shortCode = GenerateShortCode(6);

            await using var context = _dbContextFactory.CreateDbContext();

            while (await context.Links.AsNoTracking().AnyAsync(l => l.ShortCode == shortCode))
            {
                shortCode = GenerateShortCode(6);
            }

            return shortCode;
        }

        private static string GenerateShortCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int availableLength = chars.Length;
            var shortCodeChars = Enumerable.Repeat(chars, length).Select(c =>
            {
                var rndNumber = Random.Shared.Next(availableLength);
                return c[rndNumber];
            }).ToArray();
            return new string(shortCodeChars);
        }

    }
}
