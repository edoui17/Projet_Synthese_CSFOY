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
        try
        {
            if (string.IsNullOrEmpty(p_request.Username) || string.IsNullOrEmpty(p_request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // Logic temporarily simplified for audit phase.
            // Verification will be implemented later with JWT.
            Player? player = await m_playerRepository.GetByUsernameAsync(p_request.Username);
            if (player == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            string token = Guid.NewGuid().ToString();
            await m_authRepository.UpdateSessionTokenAsync(player.Id, token);

            AuthResponse response = new AuthResponse
            {
                SessionToken = token,
                Username = player.Username
            };

            return Ok(response);
        }
        catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException || ex is InvalidOperationException)
        {
            return StatusCode(503, "Service Unavailable: Database connection lost.");
        }
    }
}
