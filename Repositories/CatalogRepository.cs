using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CatalogRepository : ICatalogRepository
{
    private readonly IMongoCollection<MotorCatalog> _catalogCollection;

    public CatalogRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("teco_motors");
        _catalogCollection = database.GetCollection<MotorCatalog>("products");
    }

    public async Task<List<MotorCatalog>> GetAllAsync() =>
        await _catalogCollection.Find(_ => true).ToListAsync();

    public async Task<MotorCatalog> GetByIdAsync(string id) =>
        await _catalogCollection.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task AddAsync(MotorCatalog catalog) =>
        await _catalogCollection.InsertOneAsync(catalog);

    public async Task DeleteAsync(string id) =>
        await _catalogCollection.DeleteOneAsync(c => c.Id == id);
}
