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
}
