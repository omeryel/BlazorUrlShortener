
namespace BlazorUrl.Services
{
    public interface IShortCodeGeneratorService
    {
        Task<string> GenerateShortCodeAsync();
    }
}