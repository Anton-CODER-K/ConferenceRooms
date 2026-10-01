using ConferenceRooms.Database;
using ConferenceRooms.DTOs.Auth;
using ConferenceRooms.Entities;
using ConferenceRooms.Enums;
using ConferenceRooms.Exceptions;
using ConferenceRooms.Repositories;
using ConferenceRooms.Repositories.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace ConferenceRooms.Services
{
    public class AuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly TransactionExecutor _tx;
        private readonly IAuthRepository _authRepo;
        private readonly JwtService _jwtService;

        public AuthService(ILogger<AuthService> logger, TransactionExecutor tx, IAuthRepository authRepo, JwtService jwtService)
        {
            _logger = logger;
            _tx = tx;
            _authRepo = authRepo;
            _jwtService = jwtService;
        }

        // Регестрація користувача
        public async Task<long> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new BusinessException("Email is required.", StatusCodes.Status400BadRequest);
            }
            if (request.Password.Length < 8)
            {
                throw new BusinessException("Password must contain at least 8 characters.", StatusCodes.Status400BadRequest);
            }

            var passwordHash = HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                RoleId = Role.User,
            };

            // Виклик метода для запису користувача в бд
            var userId = await _tx.ExecuteAsync(async (conn) =>
            {
                return await _authRepo.UserRegisterAsync(user, conn);
            });
            // в майбутньому можна прикрутити надсилання на пошту і одельно для провірки кода

            _logger.LogInformation("User registered successfully. UserId: {UserId}", userId);

            return userId;
            
        }

        // Логін Користувача
        public async Task<TokensResponse> LoginAsync(LoginRequest request)
        {
            var user = await _tx.ExecuteAsync(async (conn) =>
            {
                return await _authRepo.GetUserByEmail(request.Email, conn);
            });

            if (user == null)
                throw new BusinessException("Invalid phone number or password", StatusCodes.Status401Unauthorized);

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new BusinessException("Invalid phone number or password", StatusCodes.Status401Unauthorized);

            
            var refreshToken = GenerateToken();
            var refreshTokenHash = HashToken(refreshToken);
            await _tx.ExecuteAsync(async (conn) =>
            {
                await _authRepo.InsertRefreshToken(user.UserId, refreshTokenHash, conn);
            });

            var accessToken = _jwtService.GenerateAccessToken(user.UserId, user.RoleId.ToString());

            _logger.LogInformation("User {UserId} is login", user.UserId);

            return new TokensResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

        }

        // Метод для оновлення токена
        public async Task<TokensResponse> RefreshAsync(string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);

            long userId = 0;
            string accessToken = string.Empty;
            string newRefreshToken = string.Empty;

            await _tx.ExecuteAsync(async (conn, tx) =>
            {
                var token = await _authRepo.GetActiveRefreshToken(tokenHash, conn,tx);

                if (token == null)
                {
                    throw new BusinessException("Invalid refresh token.", StatusCodes.Status401Unauthorized);
                }

                userId = token.UserId;

                var affectedRows = await _authRepo.RevokeRefreshToken(token.RefreshTokenId, conn, tx);

                if (affectedRows == 0)
                {
                    throw new BusinessException("Invalid refresh token.", StatusCodes.Status401Unauthorized);
                }

                var role = await _authRepo.GetRolesUserByUserId(token.UserId, conn, tx);

                accessToken = _jwtService.GenerateAccessToken(token.UserId, role);

                newRefreshToken = GenerateToken();

                var newRefreshTokenHash = HashToken(newRefreshToken);

                await _authRepo.InsertRefreshToken(userId, newRefreshTokenHash, conn, tx);
            });

            _logger.LogInformation("Refresh token rotated for user {UserId}", userId);

            return new TokensResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }


        private static string HashPassword(string password)
        {
            string hashPassword = BCrypt.Net.BCrypt.HashPassword(password);
            return hashPassword;
        }
        private static string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }
        private static string GenerateToken(int bytes = 32)
        {
            var buffer = new byte[bytes];
            RandomNumberGenerator.Fill(buffer);
            return Convert.ToBase64String(buffer)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }
    }
}
