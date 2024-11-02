using BlazorUrl.Client.Dtos;
using BlazorUrl.Client.Interfaces;

namespace BlazorUrl.Client.Services
{
    public class LinkApiProxy : ILinkService
    {
        private readonly ILinkApi _linkApi;

        public LinkApiProxy(ILinkApi linkApi)
        {
            _linkApi = linkApi;
        }

        public Task<LinkDto> CreateLinkAsync(LinkCreateDto linkCreateDto)
        {
            return _linkApi.CreateLinkAsync(linkCreateDto);
        }

        public Task DeleteLinkAsync(long id, string userId)
        {
            return _linkApi.DeleteLinkAsync(id);
        }

        public Task<LinkDetailsDto?> GetLinkAsync(long id, string userId)
        {
            return _linkApi.GetLinkAsync(id);
        }

        public Task<PagedResult<LinkDto>> GetLinksByUserAsync(string userId, int startIndex, int pageSize, bool activeOnly)
        {
            return _linkApi.GetLinksByUserAsync(startIndex, pageSize, activeOnly);
        }

        public Task<LinkDto?> UpdateLinkAsync(LinkEditDto linkEditDto)
        {
            return _linkApi.UpdateLinkAsync(linkEditDto.Id, linkEditDto);
        }
    }
}
