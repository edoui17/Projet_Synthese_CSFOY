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

    public PlayerController(
        IPlayerRepository p_playerRepository,
        IInventoryRepository p_inventoryRepository,
        IStatsRepository p_statsRepository)
    {
        m_playerRepository = p_playerRepository;
        m_inventoryRepository = p_inventoryRepository;
        m_statsRepository = p_statsRepository;
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
            GameStats = gameStats,
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
            .Select(p => new PlayerLeaderboardEntry
            {
                Id = p.Id,
                Username = p.Username,
                // These stats are now historic in GameStats, we take the best session's stats for leaderboard context
                Health = p.GameStats.OrderByDescending(s => s.Score).FirstOrDefault()?.Health ?? 0,
                Attack = p.GameStats.OrderByDescending(s => s.Score).FirstOrDefault()?.Attack ?? 0,
                Speed = p.GameStats.OrderByDescending(s => s.Score).FirstOrDefault()?.Speed ?? 0,
                Luck = p.GameStats.OrderByDescending(s => s.Score).FirstOrDefault()?.Luck ?? 0,
                Level = p.GameStats.OrderByDescending(s => s.Score).FirstOrDefault()?.LevelReached ?? 1,
                Score = p.HighScore
            });

        return Ok(leaderboard);
    }
}
