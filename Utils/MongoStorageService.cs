using MongoDB.Driver;
using MongoDB.Bson;

namespace DADN.Utils
{
    public class MongoStorageService : IStorageService
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public MongoStorageService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("ConveyorDB");
            _collection = database.GetCollection<BsonDocument>("ConveyorData");
        }

        public void Save(string key, string jsonData)
        {
            var doc = new BsonDocument { { "key", key }, { "data", jsonData } };
            _collection.InsertOne(doc);
        }

        public string? Get(string key)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("key", key);
            var doc = _collection.Find(filter).FirstOrDefault();
            return doc?["data"].AsString;
        }

        public void Delete(string key)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("key", key);
            _collection.DeleteOne(filter);
        }
    }
}
