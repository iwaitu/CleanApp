namespace CleanApp.Core.Interfaces
{
    public interface IMongoFileService
    {
        Task DeleteFileAsync(string id);
        Task<Stream> DownloadFileAsync(string id);
        Task<string> UploadFileAsync(Stream fileStream, string fileName);
    }
}