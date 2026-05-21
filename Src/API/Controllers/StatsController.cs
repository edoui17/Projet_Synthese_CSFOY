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
    private readonly IProgressionService m_progressionService;

    public StatsController(IStatsRepository p_statsRepository, IProgressionService p_progressionService)
    {
        m_statsRepository = p_statsRepository;
        m_progressionService = p_progressionService;
    }

    [HttpPost("session")]
    public async Task<IActionResult> AddSession([FromBody] StatsUpsertRequest p_request)
    {
        Player? player = HttpContext.Items["Player"] as Player;
        if (player == null) return Unauthorized();

        if (p_request.Stats == null) return BadRequest("Stats data is required.");

        p_request.Stats.PlayerId = player.Id;
        p_request.Stats.LevelReached = m_progressionService.CalculateLevel(p_request.Stats.Score);

        m_progressionService.CheckAndUpdateHighScore(player, p_request.Stats.Score);

        await m_statsRepository.AddGameStatsAsync(p_request.Stats);

        return Ok();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        Player? player = HttpContext.Items["Player"] as Player;
        if (player == null) return Unauthorized();

        var history = await m_statsRepository.GetTopStatsByPlayerIdAsync(player.Id);
        return Ok(history);
    }
}
