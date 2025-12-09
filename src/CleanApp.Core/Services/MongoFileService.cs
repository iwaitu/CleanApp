using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using CleanApp.Core.Interfaces;

namespace CleanApp.Core.Services
{
    public class MongoFileService : IMongoFileService
    {
        private readonly IMongoDatabase _database;
        private readonly GridFSBucket _gridFS;

        public MongoFileService(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("MongoDB")["ConnectionString"];
            var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);

            // 明确使用 GridFSBucketOptions，避免版本不兼容问题
            _gridFS = new GridFSBucket(_database, new GridFSBucketOptions
            {
                BucketName = "fs", // 默认是 "fs"，可以按需修改
                ChunkSizeBytes = 255 * 1024, // 可选：默认值
                WriteConcern = WriteConcern.WMajority,
                ReadPreference = ReadPreference.Primary
            });
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var objId = await _gridFS.UploadFromStreamAsync(fileName, fileStream);
            return objId.ToString();
        }

        public async Task<Stream> DownloadFileAsync(string id)
        {
            var objectId = ObjectId.Parse(id); // 更健壮的转换方式
            return await _gridFS.OpenDownloadStreamAsync(objectId);
        }

        public async Task DeleteFileAsync(string id)
        {
            var objectId = ObjectId.Parse(id); // 更健壮的转换方式
            await _gridFS.DeleteAsync(objectId);
        }
    }
}
