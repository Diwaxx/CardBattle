using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CardGame.Server.Data;
using CardGame.Common.Models;
using BCrypt.Net;

namespace CardGame.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly GameDbContext _context;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(GameDbContext context, ILogger<PlayersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Проверка существующего пользователя
                if (await _context.Players.AnyAsync(p => p.Username == request.Username))
                    return BadRequest(new { error = "Username already exists" });

                if (await _context.Players.AnyAsync(p => p.Email == request.Email))
                    return BadRequest(new { error = "Email already registered" });

                // Создание игрока
                var player = new Player
                {
                    Id = Guid.NewGuid(),
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Rating = 1000,
                    Level = 1,
                    Silver = 1000,
                    Gold = 50,
                    DefenseFormationJson = "{}",
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                };

                await _context.Players.AddAsync(player);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New player registered: {player.Username}");

                return Ok(new
                {
                    success = true,
                    playerId = player.Id,
                    message = "Registration successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer(Guid id)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
                return NotFound(new { error = "Player not found" });

            return Ok(new
            {
                id = player.Id,
                username = player.Username,
                rating = player.Rating,
                level = player.Level,
                gold = player.Gold,
                silver = player.Silver
            });
        }

        [HttpGet("top")]
        public async Task<IActionResult> GetTopPlayers([FromQuery] int limit = 10)
        {
            var topPlayers = await _context.Players
                .OrderByDescending(p => p.Rating)
                .Take(limit)
                .Select(p => new
                {
                    p.Id,
                    p.Username,
                    p.Rating,
                    p.Level,
                })
                .ToListAsync();

            return Ok(topPlayers);
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}