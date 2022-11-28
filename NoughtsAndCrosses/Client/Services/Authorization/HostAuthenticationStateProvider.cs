using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TicTacToeOnline.Shared.Models;

namespace TicTacToeOnline.Client.Services.Authorization
{
    public class HostAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        public HostAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var httpResponseMessage = (await _httpClient.GetAsync("api/Account/GetCurrentUser"))!;
            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var user = await httpResponseMessage.Content.ReadFromJsonAsync<User>();
                if (user != null)
                {
                    claimsIdentity = new ClaimsIdentity(
                        new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Username)
                        },
                        "GameUser");
                }
            }

            return new AuthenticationState(new ClaimsPrincipal(claimsIdentity));
        }

        public void RefreshState()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
