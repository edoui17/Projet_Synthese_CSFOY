using System;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IStatsRepository m_statsRepository;
    private readonly IAuthRepository m_authRepository;

    public StatsController(IStatsRepository p_statsRepository, IAuthRepository p_authRepository)
    {
        m_statsRepository = p_statsRepository;
        m_authRepository = p_authRepository;
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] StatsUpsertRequest p_request)
    {
        if (string.IsNullOrEmpty(p_request.SessionToken)) return Unauthorized();

        Player? player = await m_authRepository.GetBySessionTokenAsync(p_request.SessionToken);
        if (player == null) return Unauthorized();

        if (p_request.Stats == null) return BadRequest("Stats data is required.");

        p_request.Stats.PlayerId = player.Id;
        await m_statsRepository.UpdateStatsAsync(p_request.Stats);

        return Ok();
    }
}

public class StatsUpsertRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public PlayerStats Stats { get; set; } = null!;
}
