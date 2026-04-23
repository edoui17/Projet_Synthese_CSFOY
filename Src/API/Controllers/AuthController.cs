using System;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository m_authRepository;
    private readonly IPlayerRepository m_playerRepository;

    public AuthController(IAuthRepository p_authRepository, IPlayerRepository p_playerRepository)
    {
        m_authRepository = p_authRepository;
        m_playerRepository = p_playerRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest p_request)
    {
        if (string.IsNullOrEmpty(p_request.Username) || string.IsNullOrEmpty(p_request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        bool isValid = await m_authRepository.VerifyPasswordAsync(p_request.Username, p_request.Password);
        if (!isValid)
        {
            return Unauthorized("Invalid username or password.");
        }

        Player? player = await m_playerRepository.GetByUsernameAsync(p_request.Username);
        if (player == null)
        {
            return NotFound("Player not found.");
        }

        string token = Guid.NewGuid().ToString();
        await m_authRepository.UpdateSessionTokenAsync(player.Id, token);

        return Ok(new { SessionToken = token });
    }
}
