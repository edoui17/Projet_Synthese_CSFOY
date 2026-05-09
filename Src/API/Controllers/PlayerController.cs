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

        ProfileResponse response = new ProfileResponse
        {
            Username = player.Username,
            Stats = player.Stats,
            Config = player.Config,
            Inventory = inventory
        };

        return Ok(response);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] SyncRequest p_request)
    {
        try
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
        catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException || ex is InvalidOperationException)
        {
            // Log exception here in a real scenario
            return StatusCode(503, "Service Unavailable: Database connection lost.");
        }
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
        IEnumerable<Player> players = await m_playerRepository.GetAllAsync();

        IEnumerable<PlayerLeaderboardEntry> leaderboard = players.Select(p => new PlayerLeaderboardEntry
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
    }

    private int CalculateLevel(PlayerStats? p_stats)
    {
        if (p_stats == null) return 1;
        // Basic calculation based on total stats. Adjust as needed for specific game logic.
        float totalStats = p_stats.Health + p_stats.Attack + p_stats.Speed + p_stats.Luck;
        return Math.Max(1, (int)(totalStats / 10)); // Example: 1 level per 10 stat points
    }
}
