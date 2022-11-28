using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TicTacToeOnline.Shared.Models;
using MudBlazor;
using TicTacToeOnline.Client.Components;

namespace TicTacToeOnline.Client.Pages
{
    public partial class RoomList
    {
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        private HubConnection connection = default!;
        private List<Room> Rooms { get; set; } = default!;
        private string UserId { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Rooms = (await HttpClient.GetFromJsonAsync<List<Room>>("api/Room/GetAllRooms"))!;
            await ConfigureHubConnection();

            UserId = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User.Claims
                .First(t => t.Type == ClaimTypes.NameIdentifier).Value;
        }

        private async Task ConfigureHubConnection()
        {
            connection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/gameHub"))
                .Build();

            connection.On<Room>("ReceiveNewRoom", (newRoom) =>
            {
                Rooms.Add(newRoom);
                StateHasChanged();
            });

            connection.On<Room>("RemoveRoom", (oldRoom) =>
            {
                Rooms.RemoveAll(room => room.Id == oldRoom.Id);
                StateHasChanged();
            });

            await connection.StartAsync();
        }

        private async Task CreateNewRoom()
        {
            DialogOptions closeOnEscapeKey = new DialogOptions()
            {
                FullWidth = true
            };

            var result = await DialogService.Show<CreateRoomDialog>("Создать комнату", closeOnEscapeKey).Result;
            if (!result.Cancelled)
            {
                var roomIdResponseMessage = await HttpClient.PostAsJsonAsync("api/Room/CreateNewRoom", (string)result.Data);
                if (roomIdResponseMessage.IsSuccessStatusCode)
                {
                    NavigationManager.NavigateTo($"tic-tac-toe/{await roomIdResponseMessage.Content.ReadAsStringAsync()}");
                }
            }
        }
    }
}
