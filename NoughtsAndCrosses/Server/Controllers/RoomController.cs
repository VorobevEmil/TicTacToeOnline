using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TicTacToeOnline.Server.Hubs;
using TicTacToeOnline.Server.Services;
using TicTacToeOnline.Shared.Models;

namespace TicTacToeOnline.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly RoomManager _manager;

        private User CurrentUser => new User()
        {
            Id = User.Claims.First(claim => ClaimTypes.NameIdentifier == claim.Type).Value,
            Username = User.Identity!.Name!
        };

        public RoomController(IHubContext<GameHub> hubContext, RoomManager manager)
        {
            _manager = manager;
        }

        [HttpPost("CreateNewRoom")]
        public async Task<ActionResult<string>> CreateNewRoom([FromBody] string roomName)
        {
            try
            {
                var roomId = await _manager.AddRoomAsync(CurrentUser, roomName);
                return Ok(roomId);
            }
            catch (Exception)
            {
                return BadRequest("Не удалось создать комнату");
            }
        }

        [HttpGet("GetAllRooms")]
        public ActionResult<List<Room>> GetAllRooms()
        {
            try
            {
                return Ok(_manager.GetAllRooms());
            }
            catch (Exception)
            {
                return BadRequest("Не удалось получить все комнаты");
            }
        }

        [HttpGet("GetRoomById/{id}")]
        public ActionResult<Room> GetRoomById(string id)
        {
            try
            {
                var room = _manager.GetRoomById(id);
                if (room == null)
                    return NotFound();
                return Ok(room);
            }
            catch (Exception)
            {
                return BadRequest("Не удалось получить комнату");
            }
        }
    }
}
