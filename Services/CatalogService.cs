using System.Collections.Generic;
using System.Threading.Tasks;

public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _repository;

    public CatalogService(ICatalogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MotorCatalog>> GetAllAsync() => await _repository.GetAllAsync();

    public async Task AddCatalogAsync(MotorCatalog catalog) =>
        await _repository.AddAsync(catalog);

    public async Task DeleteCatalogAsync(string id) => // Thêm hàm xóa
        await _repository.DeleteAsync(id);
}
