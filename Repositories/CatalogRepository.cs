using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CatalogRepository : ICatalogRepository
{
    private readonly IMongoCollection<MotorCatalog> _catalogCollection;

    public CatalogRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("CatalogDB");
        _catalogCollection = database.GetCollection<MotorCatalog>("MotorCatalogs");
    }

    public async Task<List<MotorCatalog>> GetAllAsync() =>
        await _catalogCollection.Find(_ => true).ToListAsync();

    public async Task<MotorCatalog> GetByIdAsync(string id) =>
        await _catalogCollection.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task AddAsync(MotorCatalog catalog) =>
        await _catalogCollection.InsertOneAsync(catalog);
}
