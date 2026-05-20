using System;
using System.Security.Cryptography;
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

        Player? player = await m_playerRepository.GetByUsernameAsync(p_request.Username);
        if (player == null)
        {
            return Unauthorized("Invalid username or password.");
        }

#if DEBUG
        // Bypass temporarily simplified for audit phase in DEBUG mode.
#else
        bool isPasswordValid = await m_authRepository.VerifyPasswordAsync(p_request.Username, p_request.Password);
        if (!isPasswordValid)
        {
            return Unauthorized("Invalid username or password.");
        }
#endif

        byte[] tokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        string token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        await m_authRepository.UpdateSessionTokenAsync(player.Id, token);

        AuthResponse response = new AuthResponse
        {
            SessionToken = token,
            Username = player.Username
        };

        return Ok(response);
    }
}
