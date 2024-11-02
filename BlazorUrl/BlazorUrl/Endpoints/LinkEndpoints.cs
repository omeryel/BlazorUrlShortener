using BlazorUrl.Client.Dtos;
using BlazorUrl.Client.Extensions;
using BlazorUrl.Client.Interfaces;
using BlazorUrl.Services;
using Humanizer;
using System.Security.Claims;

namespace BlazorUrl.Endpoints
{
    public static class LinkEndpoints
    {

        public static IEndpointRouteBuilder MapLinkEndpoints(this IEndpointRouteBuilder app)
        {
            var linksGroup = app.MapGroup("/api/links").RequireAuthorization();

            linksGroup.MapPost("", async (LinkCreateDto dto, ILinkService linkService, ClaimsPrincipal principal) =>
            {
                var userId = principal.GetUserId();
                if (userId != dto.UserId)
                {
                    return Results.Unauthorized();
                }

                var link = await linkService.CreateLinkAsync(dto);
                return Results.Ok(link);
            });

            linksGroup.MapGet("", async (int startIndex, int pageSize, bool activeOnly, ILinkService linkService, ClaimsPrincipal principal) =>
            {
                var userId = principal.GetUserId();
                var pagedResult = await linkService.GetLinksByUserAsync(userId, startIndex, pageSize, activeOnly);
                return Results.Ok(pagedResult);
            });

            linksGroup.MapPatch("/{linkId:long}", async (long linkId, LinkEditDto dto, ILinkService linkService, ClaimsPrincipal principal) =>
                {
                    var userId = principal.GetUserId();
                    if (userId != dto.UserId)
                    {
                        return Results.Unauthorized();
                    }
                    if (linkId != dto.Id)
                    {
                        Results.NotFound();
                    }

                    var link = await linkService.UpdateLinkAsync(dto);
                    if (link is null)
                    {
                        Results.NotFound(linkId);
                    }
                    return Results.Ok(link);
                });


            linksGroup.MapDelete("/{linkId:long}", async (long linkId, ILinkService linkService, ClaimsPrincipal principal) =>
            {
                var userId = principal.GetUserId();

                await linkService.DeleteLinkAsync(linkId, userId);

                return Results.NoContent();
            });


            linksGroup.MapGet("/{linkId:long}", async (long linkId, ILinkService linkService, ClaimsPrincipal principal) =>
            {
                var userId = principal.GetUserId();
                var linkDetailsDto = await linkService.GetLinkAsync(linkId, userId!);
                return Results.Ok(linkDetailsDto);

            });

            return app;
        }


    }
}
