using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicTacToeOnline.Shared.Models;

namespace TicTacToeOnline.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        [HttpGet("GetCurrentUser")]
        public ActionResult<User> GetCurrentUser()
         {
            if (User.Identity!.IsAuthenticated)
            {
                return Ok(new User()
                {
                    Id = User.Claims.First(t => ClaimTypes.NameIdentifier == t.Type).Value,
                    Username = User.Identity.Name!
                });
            }

            return Unauthorized("Пользователь не авторизован");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] string username)
        {

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, username)
            };
            var claimIdentity = new ClaimsIdentity(claims, "TicTacToe");
            var claimPrincipal = new ClaimsPrincipal(claimIdentity);
            await HttpContext.SignInAsync("TicTacToe", claimPrincipal);

            return Ok("Пользователь вошел в систему");
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(User.Identity!.AuthenticationType);
            return Ok("Пользователь вышел из системы");
        }

    }
}
