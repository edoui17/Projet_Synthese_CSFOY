using System;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces;

public interface IPlayerRepository : IRepository<Player>
{
    Task<Player?> GetByUsernameAsync(string p_username);
}
