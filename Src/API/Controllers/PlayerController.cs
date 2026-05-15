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
    public async Task<IActionResult> GetLeaderboard()
    {
        IEnumerable<Player> players = await m_playerRepository.GetAllAsync();

        IEnumerable<PlayerLeaderboardEntry> leaderboard = players
            .OrderByDescending(p => p.HighScore)
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
                    Level = bestSession?.LevelReached ?? 1,
                    Score = p.HighScore
                };
            });

        return Ok(leaderboard);
    }
}
