using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces;

public interface IConfigRepository
{
    Task UpdateConfigAsync(PlayerConfig p_config);
}
