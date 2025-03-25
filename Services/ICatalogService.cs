using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICatalogService
{
    Task<List<MotorCatalog>> GetAllAsync();
    Task AddCatalogAsync(MotorCatalog catalog);
    Task DeleteCatalogAsync(string id);
}
