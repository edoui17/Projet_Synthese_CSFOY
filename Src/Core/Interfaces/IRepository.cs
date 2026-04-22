using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object p_id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T p_entity);
    void Update(T p_entity);
    void Delete(T p_entity);
    Task SaveChangesAsync();
}
