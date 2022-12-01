using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using TicTacToeOnline.Shared.Models;
using MudBlazor;
using Microsoft.AspNetCore.SignalR.Client;

namespace TicTacToeOnline.Client.Pages
{
    public partial class RoomGame
    {
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Parameter] public string RoomId { get; set; } = default!;
        private Room Room { get; set; } = default!;
        private User User { get; set; } = default!;
        private HubConnection _connection = default!;

        protected override async Task OnInitializedAsync()
        {
            await ConfigureUser();
            await InitializedRoomAsync();
            await ConfigureHubConnectionAsync();
        }

        private async Task ConfigureUser()
        {
            var user = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
            User = new User()
            {
                Id = user.Claims.First(t => t.Type == ClaimTypes.NameIdentifier).Value,
                Username = user.Identity!.Name!
            };
        }

        private async Task InitializedRoomAsync()
        {
            var httpResponseMessage = await HttpClient.GetAsync($"api/Room/GetRoomById/{RoomId}");
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                Room = (await httpResponseMessage.Content.ReadFromJsonAsync<Room>())!;
            }
            else
            {
                var notFound = httpResponseMessage.StatusCode == HttpStatusCode.NotFound;
                await DialogService.ShowMessageBox("Ошибка", $"Не удалось {(notFound ? "найти данную комнату" : "подключиться к комнате")}!", yesText: "ОК");
                NavigationManager.NavigateTo("/");
            }
        }

        private async Task ConfigureHubConnectionAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/gameHub"))
                .Build();


            _connection.On<User>("Notify", async (user) =>
            {
                if (!Room.Users.Any(t => t.Id == user.Id)!)
                {
                    Snackbar.Add($"Пользователь {user.Username} вошел в комнату", Severity.Info);
                    Room.Users.Add(user);
                }
                else
                {
                    Room.Users[Room.Users.FindIndex(t => t.Id == user.Id)] = user;
                }

                if (Room.Users.Count == 1)
                {
                    Room.MovingUser = user.Id;
                }

                await _connection.SendAsync("ReplaceCurrentRoom", Room);
            });

            _connection.On<User>("NotifyLeave", (user) =>
            {
                Snackbar.Add($"Пользователь {user.Username} вышел из комнаты", Severity.Info);
            });

            _connection.On<User>("NotifyWin",  (user) =>
            {
                Snackbar.Add($"Пользователь {user.Username} победил в игре", Severity.Info);
            });

            _connection.On<User>("NotifyScopePlus", (user) =>
            {
                Snackbar.Add($"Плюс 1 очко игроку {user.Username}", Severity.Info);
            });
            _connection.On<Room>("ReplaceOldRoom", (room) =>
            {
                Room = room;
                StateHasChanged();
            });

            await _connection.StartAsync();

            await _connection.SendAsync("EnterToGame", Room);
        }

        private async Task SetFigureAsync(Figure figure)
        {
            figure.FigureType = Room.Users[0].Id == User.Id ? FigureType.Cross : FigureType.Nought;
            figure.Move = true;
            Room.MovingUser = Room.MovingUser == Room.Users[0].Id ? Room.Users[1].Id : Room.Users[0].Id;

            await _connection.SendAsync("SetFigure", Room);
        }

        private string? SetIcon(FigureType figureType)
        {
            return figureType switch
            {
                FigureType.Cross => Icons.Filled.Close,
                FigureType.Nought => Icons.Filled.RadioButtonUnchecked,
                _ => null
            };
        }

        private string? SetColor(FigureType figureType)
        {
            return figureType switch
            {
                FigureType.Cross => "color:#418dff !important",
                FigureType.Nought => "color:orange !important",
                _ => null
            };
        }

        private string? SetBackground(FigureType figureType)
        {
            return figureType switch
            {
                FigureType.Cross => "background: #ddf1ffe3",
                FigureType.Nought => "background: #fff9e2e3",
                _ => null
            };
        }
    }
}
