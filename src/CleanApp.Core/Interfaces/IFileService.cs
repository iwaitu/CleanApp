using CleanApp.Contracts;
using CleanApp.Domain;

namespace CleanApp.Core.Interfaces
{
    public interface IFileService
    {
        Task DeleteAsync(string id);
        Task<Stream> DownloadAsync(string id);
        Task<PageOf<AppFile>> ListAsync(string? name, int page = 1, int pageSize = 20);
        Task<AppFile> UploadAsync(Stream fileStream, string fileName);
    }
}