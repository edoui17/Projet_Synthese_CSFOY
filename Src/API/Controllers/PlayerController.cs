using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IPlayerRepository m_playerRepository;
    private readonly IInventoryRepository m_inventoryRepository;
    private readonly IStatsRepository m_statsRepository;
    private readonly IProgressionService m_progressionService;

    public PlayerController(
        IPlayerRepository p_playerRepository,
        IInventoryRepository p_inventoryRepository,
        IStatsRepository p_statsRepository,
        IProgressionService p_progressionService)
    {
        m_playerRepository = p_playerRepository;
        m_inventoryRepository = p_inventoryRepository;
        m_statsRepository = p_statsRepository;
        m_progressionService = p_progressionService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        Player? player = HttpContext.Items["Player"] as Player;
        if (player == null) return Unauthorized();

        IEnumerable<InventoryEntry> inventory = await m_inventoryRepository.GetByPlayerIdAsync(player.Id);
        IEnumerable<GameStats> gameStats = await m_statsRepository.GetTopStatsByPlayerIdAsync(player.Id);

        ProfileResponse response = new ProfileResponse
        {
            Username = player.Username,
            HighScore = player.HighScore,
            UpdatedAt = player.UpdatedAt,
            LastSessions = gameStats,
            Config = player.Config,
            Inventory = inventory
        };

        return Ok(response);
    }

    [HttpGet("profile/{p_pseudonyme}")]
    public async Task<IActionResult> GetProfileByUsername(string p_pseudonyme)
    {
        Player? player = await m_playerRepository.GetByUsernameAsync(p_pseudonyme);
        if (player == null) return NotFound();

        IEnumerable<InventoryEntry> inventory = await m_inventoryRepository.GetByPlayerIdAsync(player.Id);
        IEnumerable<GameStats> gameStats = await m_statsRepository.GetTopStatsByPlayerIdAsync(player.Id);

        ProfileResponse response = new ProfileResponse
        {
            Username = player.Username,
            HighScore = player.HighScore,
            UpdatedAt = player.UpdatedAt,
            LastSessions = gameStats,
            Config = player.Config,
            Inventory = inventory
        };

        return Ok(response);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] SyncRequest p_request)
    {
        Player? player = HttpContext.Items["Player"] as Player;
        if (player == null) return Unauthorized();

        if (p_request.Stats != null)
        {
            p_request.Stats.PlayerId = player.Id;
            p_request.Stats.LevelReached = m_progressionService.CalculateLevel(p_request.Stats.Score);

            m_progressionService.CheckAndUpdateHighScore(player, p_request.Stats.Score);

            await m_statsRepository.AddGameStatsAsync(p_request.Stats);
        }

        if (p_request.Inventory != null)
        {
            await m_inventoryRepository.UpdateInventoryAsync(player.Id, p_request.Inventory);
        }

        return Ok();
    }

    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] string? p_sortBy = "score",
        [FromQuery] string? p_order = "desc",
        [FromQuery] string? p_search = null)
    {
        IEnumerable<Player> players = await m_playerRepository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(p_search))
        {
            players = players.Where(p => p.Username.Contains(p_search, StringComparison.OrdinalIgnoreCase));
        }

        bool isDescending = p_order?.ToLower() != "asc";

        switch (p_sortBy?.ToLower())
        {
            case "duration":
                players = isDescending
                    ? players.OrderByDescending(p => p.GameStats.Any() ? p.GameStats.Max(s => s.Duration) : TimeSpan.Zero)
                    : players.OrderBy(p => p.GameStats.Any() ? p.GameStats.Max(s => s.Duration) : TimeSpan.Zero);
                break;
            case "level":
                players = isDescending
                    ? players.OrderByDescending(p => p.GameStats.Any() ? p.GameStats.Max(s => s.LevelReached) : 0)
                    : players.OrderBy(p => p.GameStats.Any() ? p.GameStats.Max(s => s.LevelReached) : 0);
                break;
            case "health":
                players = isDescending
                    ? players.OrderByDescending(p => p.GameStats.Any() ? p.GameStats.Max(s => s.BonusHealth) : 0)
                    : players.OrderBy(p => p.GameStats.Any() ? p.GameStats.Max(s => s.BonusHealth) : 0);
                break;
            case "attack":
                players = isDescending
                    ? players.OrderByDescending(p => p.GameStats.Any() ? p.GameStats.Max(s => s.BonusAttack) : 0)
                    : players.OrderBy(p => p.GameStats.Any() ? p.GameStats.Max(s => s.BonusAttack) : 0);
                break;
            case "speed":
                players = isDescending
                    ? players.OrderByDescending(p => p.GameStats.Any() ? p.GameStats.Max(s => s.BonusSpeed) : 0)
                    : players.OrderBy(p => p.GameStats.Any() ? p.GameStats.Max(s => s.BonusSpeed) : 0);
                break;
            case "luck":
                players = isDescending
                    ? players.OrderByDescending(p => p.GameStats.Any() ? p.GameStats.Max(s => s.BonusLuck) : 0)
                    : players.OrderBy(p => p.GameStats.Any() ? p.GameStats.Max(s => s.BonusLuck) : 0);
                break;
            case "score":
            default:
                players = isDescending
                    ? players.OrderByDescending(p => p.HighScore)
                    : players.OrderBy(p => p.HighScore);
                break;
        }

        IEnumerable<PlayerLeaderboardEntry> leaderboard = players
            .Take(50)
            .Select(p =>
            {
                var bestSession = p.GameStats.OrderByDescending(s => s.Score).FirstOrDefault();
                return new PlayerLeaderboardEntry
                {
                    Id = p.Id,
                    Username = p.Username,
                    Health = bestSession?.Health ?? 0,
                    Attack = bestSession?.Attack ?? 0,
                    Speed = bestSession?.Speed ?? 0,
                    Luck = bestSession?.Luck ?? 0,
                    BonusHealth = bestSession?.BonusHealth ?? 0,
                    BonusAttack = bestSession?.BonusAttack ?? 0,
                    BonusSpeed = bestSession?.BonusSpeed ?? 0,
                    BonusLuck = bestSession?.BonusLuck ?? 0,
                    Level = bestSession?.LevelReached ?? 1,
                    Score = p.HighScore,
                    Duration = bestSession?.Duration ?? TimeSpan.Zero
                };
            });

        return Ok(leaderboard);
    }
}
