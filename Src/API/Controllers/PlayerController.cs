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
    private readonly IAuthRepository m_authRepository;

    public PlayerController(
        IPlayerRepository p_playerRepository,
        IInventoryRepository p_inventoryRepository,
        IStatsRepository p_statsRepository,
        IAuthRepository p_authRepository)
    {
        m_playerRepository = p_playerRepository;
        m_inventoryRepository = p_inventoryRepository;
        m_statsRepository = p_statsRepository;
        m_authRepository = p_authRepository;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile([FromHeader(Name = "X-Session-Token")] string p_token)
    {
        if (string.IsNullOrEmpty(p_token)) return Unauthorized();

        Player? player = await m_authRepository.GetBySessionTokenAsync(p_token);
        if (player == null) return Unauthorized();

        IEnumerable<InventoryEntry> inventory = await m_inventoryRepository.GetByPlayerIdAsync(player.Id);

        PlayerProfile profile = new PlayerProfile
        {
            Player = player,
            Inventory = inventory,
            Stats = player.Stats,
            Config = player.Config
        };

        return Ok(profile);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] SyncRequest p_request)
    {
        if (string.IsNullOrEmpty(p_request.SessionToken)) return Unauthorized();

        Player? player = await m_authRepository.GetBySessionTokenAsync(p_request.SessionToken);
        if (player == null) return Unauthorized();

        if (p_request.Stats != null)
        {
            p_request.Stats.PlayerId = player.Id;
            await m_statsRepository.UpdateStatsAsync(p_request.Stats);
        }

        if (p_request.Inventory != null)
        {
            await m_inventoryRepository.UpdateInventoryAsync(player.Id, p_request.Inventory);
        }

        return Ok();
    }
/*
    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        var players = await m_playerRepository.GetAllAsync();

        var leaderboard = players.Select(p => new PlayerLeaderboardEntry
        {
            Id = p.Id,
            Username = p.Username,
            Health = p.Stats?.Health ?? 0,
            Attack = p.Stats?.Attack ?? 0,
            Speed = p.Stats?.Speed ?? 0,
            Luck = p.Stats?.Luck ?? 0,
            Level = CalculateLevel(p.Stats)
        });

        return Ok(leaderboard);
    }*/

    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        // Ligne temporaire pour tester
        var leaderboard = new List<PlayerLeaderboardEntry>
        {
            new PlayerLeaderboardEntry { Id = Guid.NewGuid(), Username = "SuperGamer", Level = 50, Health = 500, Speed = 20 },
            new PlayerLeaderboardEntry { Id = Guid.NewGuid(), Username = "Marcorse", Level = -2, Health = 20, Speed = 1 },
            new PlayerLeaderboardEntry { Id = Guid.NewGuid(), Username = "SpeedRunner", Level = 45, Health = 100, Speed = 99 },
            new PlayerLeaderboardEntry { Id = Guid.NewGuid(), Username = "JoueurTest", Level = 4, Health = 3, Speed = 12 }
        };

        return Ok(leaderboard);
    }

    private int CalculateLevel(PlayerStats? stats)
    {
        if (stats == null) return 1;
        // Basic calculation based on total stats. Adjust as needed for specific game logic.
        float totalStats = stats.Health + stats.Attack + stats.Speed + stats.Luck;
        return Math.Max(1, (int)(totalStats / 10)); // Example: 1 level per 10 stat points
    }
}
