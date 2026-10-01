using ConferenceRooms.DTOs.Auth;
using ConferenceRooms.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.Controllers
{
    [ApiController]
    [Route("/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Метод Регесрації
        [HttpPost("register")]
        public async Task<ActionResult<long>> Register([FromBody] RegisterRequest request)
        {
            var userId = await _authService.RegisterAsync(request);

            return StatusCode(StatusCodes.Status201Created,
               new RegisterResponse
               {
                   UserId = userId
               });
        }

        // Метод Логіна
        [HttpPost("login")]
        public async Task<ActionResult<TokensResponse>> Login([FromBody] LoginRequest request)
        {
            var tokens = await _authService.LoginAsync(request);
            return Ok(tokens);
        }
        

        // Метод для рефреш токена
        [HttpPost("refresh")]
        public async Task<ActionResult<TokensResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            var tokens = await _authService.RefreshAsync(request.RefreshToken);

            return Ok(tokens);
        }
    }
}
