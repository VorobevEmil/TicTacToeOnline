using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using TicTacToeOnline.Server.Services;
using TicTacToeOnline.Shared.Models;

namespace TicTacToeOnline.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly RoomManager _manager;
        public GameHub(RoomManager manager)
        {
            _manager = manager;
        }

        private string UserId => Context.User!.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
        public async Task ReplaceCurrentRoom(Room room)
        {
            _manager.ReplaceRoom(room);
            await Clients.Group(room.Id).SendAsync("ReplaceOldRoom", room);
        }

        public async Task SetFigure(Room room)
        {
            var winP1 = room.CheckPlayerWin(FigureType.Cross);
            var winP2 = room.CheckPlayerWin(FigureType.Nought);
            if (winP1 || winP2)
            {
                if (winP1)
                {
                    room.ScorePlayer1++;
                    room.MovingUser = room.Users[0].Id;
                    await Clients.Groups(room.Id).SendAsync("NotifyScopePlus", room.Users[0]);
                }
                else
                {
                    room.ScorePlayer2++;
                    room.MovingUser = room.Users[1].Id;
                    await Clients.Groups(room.Id).SendAsync("NotifyScopePlus", room.Users[1]);
                }

                room.ResetFigures();
                room.RoundCount--;
            }

            if (room.CheckIfAllCellsFilled() && !winP1 && !winP2)
            {
                room.ResetFigures();
            }
            else  if (room.RoundCount == 0 && room.ScorePlayer1 == 5 && room.ScorePlayer2 == 5)
            {
                room.RoundCount++;
                room.ResetFigures();
            }
            else if (room.RoundCount == 0)
            {
                if (room.ScorePlayer1 > room.ScorePlayer2)
                {
                    await Clients.Groups(room.Id).SendAsync("NotifyWin", room.Users[0]);

                }
                else
                {
                    await Clients.Groups(room.Id).SendAsync("NotifyWin", room.Users[1]);
                }

                room.ScorePlayer1 = default!;
                room.ScorePlayer2 = default!;
                room.RoundCount = 10;
            }

            await ReplaceCurrentRoom(room);
        }

        public async Task EnterToGame(Room room)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);
            var user = room.Users.FirstOrDefault(t => t.Id == UserId);
            if (user == null)
            {
                user = new User()
                {
                    Id = UserId,
                    ConnectionsId = new List<string>() { Context.ConnectionId },
                    Username = Context.User!.Identity!.Name!
                };
            }
            else
            {
                user.ConnectionsId.Add(Context.ConnectionId);
            }

            await Clients.Groups(room.Id).SendAsync("Notify", user);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _manager.OnDisconnectedUserFromRoomAsync(UserId, Context.ConnectionId);
        }
    }
}
