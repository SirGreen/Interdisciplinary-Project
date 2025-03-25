using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICatalogRepository
{
    Task<List<MotorCatalog>> GetAllAsync();
    Task<MotorCatalog> GetByIdAsync(string id);
    Task AddAsync(MotorCatalog catalog);
}
