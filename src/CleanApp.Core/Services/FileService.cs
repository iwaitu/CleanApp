using CleanApp.Contracts;
using CleanApp.Core.Interfaces;
using CleanApp.Domain;
using CleanApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CleanApp.Core.Services
{
    public class FileService : IFileService
    {
        private readonly IMongoFileService _mongoFileService;
        private readonly IUnitOfWork<AppDbContext> _unitOfWork;

        public FileService(IMongoFileService mongoFileService, IUnitOfWork<AppDbContext> unitOfWork)
        {
            _mongoFileService = mongoFileService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppFile> UploadAsync(Stream fileStream, string fileName)
        {
            // upload binary content to MongoDB GridFS and map to AppFile
            var id = await _mongoFileService.UploadFileAsync(fileStream, fileName);

            var file = new AppFile
            {
                Id = id,
                FileName = fileName,
                SizeInBytes = (int)(fileStream.Length >= 0 ? fileStream.Length : 0)
            };

            _unitOfWork.AddEntity(file);
            await _unitOfWork.CommitAsync();

            return file;
        }

        public Task<Stream> DownloadAsync(string id)
        {
            return _mongoFileService.DownloadFileAsync(id);
        }

        public async Task DeleteAsync(string id)
        {
            await _mongoFileService.DeleteFileAsync(id);

            var entity = await _unitOfWork.FindByIdAsync<AppFile>(id);
            if (entity != null)
            {
                _unitOfWork.RemoveEntity(entity);
                await _unitOfWork.CommitAsync();
            }
        }

        // list all files metadata from MongoDB
        public async Task<PageOf<AppFile>> ListAsync(string? name, int page = 1, int pageSize = 20)
        {
            var query = _unitOfWork.GetRepository<AppFile>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(p => p.FileName.Contains(name));
            }

            var total = await query.CountAsync();

            var files = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageOf<AppFile>
            {
                List = files,
                Page = page,
                PageSize = pageSize,
                Total = total
            };
        }
    }
}
