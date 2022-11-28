using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using TicTacToeOnline.Client.Services.Authorization;
using TicTacToeOnline.Client.Shared;

namespace TicTacToeOnline.Client.Pages
{
    public partial class Index
    {
        [Inject] private HostAuthenticationStateProvider HostAuthenticationStateProvider { get; set; } = default!;
        [CascadingParameter] public MainLayout Parent { get; set; } = default!;

        private string _username = default!;
        private async Task LoginAsync()
        {
            await HttpClient.PostAsJsonAsync($"api/Account/Login", _username);
            HostAuthenticationStateProvider.RefreshState();
            await Parent.RefreshStateAsync();
        }
    }
}
