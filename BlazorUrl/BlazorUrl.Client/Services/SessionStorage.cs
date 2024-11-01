using Microsoft.JSInterop;
using System.Security.Principal;

namespace BlazorUrl.Client.Services
{
    public class SessionStorage
    {
        public const string LongUrlKey = "long-url";
         private readonly IJSRuntime _runtime;

        public SessionStorage(IJSRuntime jsRuntime)
        {
            _runtime = jsRuntime;
        }

        public async Task SaveAsync(string key, string value)
        {
            await _runtime.InvokeVoidAsync("window.sessionStorage.setItem", key, value);
        }


        public async Task<string?> GetAsync(string key)
        {
            return await _runtime.InvokeAsync<string?>("window.sessionStorage.getItem", key);
        }

        public async Task RemoveAsync(string key)
        {
            await _runtime.InvokeVoidAsync("window.sessionStorage.removeItem", key);
        }
    }
}
