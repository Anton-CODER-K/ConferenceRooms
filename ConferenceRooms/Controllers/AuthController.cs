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
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
        [ProducesResponseType(typeof(TokensResponse), 200)]
        public async Task<ActionResult<TokensResponse>> Login([FromBody] LoginRequest request)
        {
            var tokens = await _authService.LoginAsync(request);
            return Ok(tokens);
        }
        

        // Метод для рефреш токена
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokensResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokensResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            var tokens = await _authService.RefreshAsync(request.RefreshToken);

            return Ok(tokens);
        }
    }
}
