using Microsoft.AspNetCore.SignalR;
using TicTacToeOnline.Server.Hubs;
using TicTacToeOnline.Shared.Models;

namespace TicTacToeOnline.Server.Services
{
    public class RoomManager
    {
        private readonly IHubContext<GameHub> _hubContext;
        public RoomManager(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        private static List<Room> Rooms { get; } = new();
        public async Task<string> AddRoomAsync(User hostUser, string roomName)
        {
            var room = new Room()
            {
                Id = Guid.NewGuid().ToString(),
                RoomName = roomName,
            };
            room.RoundCount = 10;
            room.ResetFigures();
            Rooms.Add(room);

            await _hubContext.Clients.All.SendAsync("ReceiveNewRoom", room);

            return room.Id;
        }

        public List<Room> GetAllRooms()
        {
            return Rooms.ToList();
        }

        public Room? GetRoomById(string id)
        {
            return Rooms.FirstOrDefault(room => room.Id == id);
        }

        public async Task OnDisconnectedUserFromRoomAsync(string userId, string connectionId)
        {
            var room = Rooms.FirstOrDefault(room => room.Users.Any(t => t.Id == userId));
            if (room != null)
            {
                var userIndex = room.Users.FindIndex(t => t.Id == userId);
                if (room.Users[userIndex].ConnectionsId.Contains(connectionId))
                {
                    room.Users[userIndex].ConnectionsId.Remove(connectionId);
                }

                if (room.Users[userIndex].ConnectionsId.Count == 0)
                {

                    var isGamePlayers = room.Users.Take(2).Any(t => t.Id == room.Users[userIndex].Id);

                    await _hubContext.Clients.Group(room.Id).SendAsync("NotifyLeave", room.Users[userIndex]);
                    room.Users.Remove(room.Users[userIndex]);

                    if (isGamePlayers && room.Users.Count != 0)
                    {
                        room.RoundCount = 10;
                        room.MovingUser = room.Users[0].Id;
                        room.ResetFigures();
                    }
                }
                await _hubContext.Clients.Group(room.Id).SendAsync("ReplaceOldRoom", room);

                if (room.Users.Count == default)
                {
                    Rooms.Remove(room);
                    await _hubContext.Clients.All.SendAsync("RemoveRoom", room);
                }
            }
        }

        public void ReplaceRoom(Room room)
        {
            Rooms[Rooms.FindIndex(oldRoom => oldRoom.Id == room.Id)] = room;
        }
    }
}
