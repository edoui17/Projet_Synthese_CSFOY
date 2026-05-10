using System;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerConfigController : ControllerBase
{
    private readonly IConfigRepository m_configRepository;
    private readonly IAuthRepository m_authRepository;

    public PlayerConfigController(IConfigRepository p_configRepository, IAuthRepository p_authRepository)
    {
        m_configRepository = p_configRepository;
        m_authRepository = p_authRepository;
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] ConfigUpsertRequest p_request)
    {
        try
        {
            Player? player = HttpContext.Items["Player"] as Player;
            if (player == null) return Unauthorized();

            if (p_request.Config == null) return BadRequest("Config data is required.");

            p_request.Config.PlayerId = player.Id;
            await m_configRepository.UpdateConfigAsync(p_request.Config);

            return Ok();
        }
        catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException || ex is InvalidOperationException)
        {
            return StatusCode(503, "Service Unavailable: Database connection lost.");
        }
    }
}
