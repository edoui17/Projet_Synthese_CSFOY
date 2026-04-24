using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryRepository m_inventoryRepository;
    private readonly IAuthRepository m_authRepository;

    public InventoryController(IInventoryRepository p_inventoryRepository, IAuthRepository p_authRepository)
    {
        m_inventoryRepository = p_inventoryRepository;
        m_authRepository = p_authRepository;
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] InventoryUpsertRequest p_request)
    {
        if (string.IsNullOrEmpty(p_request.SessionToken)) return Unauthorized();

        Player? player = await m_authRepository.GetBySessionTokenAsync(p_request.SessionToken);
        if (player == null) return Unauthorized();

        if (p_request.Inventory == null) return BadRequest("Inventory data is required.");

        await m_inventoryRepository.UpdateInventoryAsync(player.Id, p_request.Inventory);

        return Ok();
    }
}

public class InventoryUpsertRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public IEnumerable<InventoryEntry> Inventory { get; set; } = null!;
}
