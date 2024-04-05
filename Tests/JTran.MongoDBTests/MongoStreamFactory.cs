using MondoCore.Data;

using JTran.Common;
using JTran.Streams;

namespace JTran.MongoDBTests
{
    internal class MongoStreamFactory<T> : StreamFactory, IAsyncDisposable where T : IIdentifiable<Guid>, new()
    {
        private readonly IWriteRepository<Guid, T> _writer;

        public MongoStreamFactory(string dbName, string collectionName, string connectionStr) 
        {
            var db = new MondoCore.MongoDB.MongoDB(dbName, connectionStr); 
            
            _writer = db.GetRepositoryWriter<Guid, T>(collectionName);
        }

        protected override Task HandleStream(Stream stream, int index)
        {
            T obj = stream.ToObject<T>();

            return _writer.Insert(obj);
        }
    }
}
