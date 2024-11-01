using System.Security.Claims;

namespace BlazorUrl.Client.Extensions
{
    public static class ClaimPrincipalExtensions
    {

        public static string? GetUserId(this ClaimsPrincipal principal) 
            => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    }
}
