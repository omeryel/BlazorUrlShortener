﻿using BlazorUrl.Client.Dtos;

namespace BlazorUrl.Client.Interfaces
{
    public interface ILinkService
    {
        Task<LinkDto> CreateLinkAsync(LinkCreateDto linkCreateDto);
        Task<PagedResult<LinkDto>> GetLinksByUserAsync(string userId, int startIndex, int pageSize, bool activeOnly);
        Task<LinkDto?> UpdateLinkAsync(LinkEditDto linkEditDto);

        Task DeleteLinkAsync(long id, string userId);

        Task<LinkDetailsDto?> GetLinkAsync(long id, string userId);
        Task<DashboardDataDto> GetDashboardDataAsync(string userId);
    }
}