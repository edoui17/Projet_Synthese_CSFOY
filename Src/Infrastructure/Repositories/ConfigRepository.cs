using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ConfigRepository : IConfigRepository
{
    private readonly AppDbContext m_context;

    public ConfigRepository(AppDbContext p_context)
    {
        m_context = p_context;
    }

    public async Task UpdateConfigAsync(PlayerConfig p_config)
    {
        var entity = await m_context.PlayerConfigs.FirstOrDefaultAsync(c => c.PlayerId == p_config.PlayerId);

        if (entity == null)
        {
            entity = new PlayerConfigEntity
            {
                PlayerId = p_config.PlayerId,
                MasterVolume = p_config.MasterVolume,
                MusicVolume = p_config.MusicVolume,
                SfxVolume = p_config.SfxVolume
            };
            await m_context.PlayerConfigs.AddAsync(entity);
        }
        else
        {
            entity.MasterVolume = p_config.MasterVolume;
            entity.MusicVolume = p_config.MusicVolume;
            entity.SfxVolume = p_config.SfxVolume;
        }

        await m_context.SaveChangesAsync();
    }
}
