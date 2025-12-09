using CleanApp.Core.Interfaces;
using CleanApp.Core.Services;
using CleanApp.Domain;
using CleanApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CleanAppCoreTests
{
    public class FileServiceTests
    {
        [Fact]
        public async Task UploadAsync_ShouldStoreFileInMongoAndPersistMetadata()
        {
            // arrange
            var mongoFileServiceMock = new Mock<IMongoFileService>();
            mongoFileServiceMock
                .Setup(m => m.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("mongo-file-id");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "FileServiceTests_Upload")
                .Options;

            await using var context = new AppDbContext(options);
            var unitOfWork = new UnitOfWork<AppDbContext>(context);
            var service = new FileService(mongoFileServiceMock.Object, unitOfWork);

            await using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            var fileName = "test.txt";

            // act
            var result = await service.UploadAsync(stream, fileName);

            // assert
            Assert.Equal("mongo-file-id", result.Id);
            Assert.Equal(fileName, result.FileName);
            Assert.Equal(stream.Length, result.SizeInBytes);

            var inDb = await context.AppFiles.FirstOrDefaultAsync(f => f.Id == result.Id);
            Assert.NotNull(inDb);
            Assert.Equal(fileName, inDb!.FileName);

            mongoFileServiceMock.Verify(m => m.UploadFileAsync(It.IsAny<Stream>(), fileName), Times.Once);
        }

        [Fact]
        public async Task DownloadAsync_ShouldCallMongoServiceAndReturnStream()
        {
            // arrange
            var mongoFileServiceMock = new Mock<IMongoFileService>();
            var expectedStream = new MemoryStream(new byte[] { 1, 2, 3 });

            mongoFileServiceMock
                .Setup(m => m.DownloadFileAsync("file-id"))
                .ReturnsAsync(expectedStream);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "FileServiceTests_Download")
                .Options;

            await using var context = new AppDbContext(options);
            var unitOfWork = new UnitOfWork<AppDbContext>(context);
            var service = new FileService(mongoFileServiceMock.Object, unitOfWork);

            // act
            var resultStream = await service.DownloadAsync("file-id");

            // assert
            Assert.Same(expectedStream, resultStream);
            mongoFileServiceMock.Verify(m => m.DownloadFileAsync("file-id"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteFromMongoAndMarkMetadataDeleted()
        {
            // arrange
            var mongoFileServiceMock = new Mock<IMongoFileService>();
            mongoFileServiceMock
                .Setup(m => m.DeleteFileAsync("file-id"))
                .Returns(Task.CompletedTask);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "FileServiceTests_Delete")
                .Options;

            await using var context = new AppDbContext(options);
            // 预先插入一条元数据
            var appFile = new AppFile
            {
                Id = "file-id",
                FileName = "to-delete.txt",
                SizeInBytes = 10
            };
            context.AppFiles.Add(appFile);
            await context.SaveChangesAsync();

            var unitOfWork = new UnitOfWork<AppDbContext>(context);
            var service = new FileService(mongoFileServiceMock.Object, unitOfWork);

            // act
            await service.DeleteAsync("file-id");

            // assert
            mongoFileServiceMock.Verify(m => m.DeleteFileAsync("file-id"), Times.Once);

            var inDb = await context.AppFiles.FirstOrDefaultAsync(f => f.Id == "file-id");
            // 由于 UnitOfWork 的 RemoveEntity 对 BaseEntity 做软删，
            // 这里如果 AppFile 继承 BaseEntity，应该是 IsDeleted = true
            // 如果你期望物理删除，可以改为 Assert.Null(inDb);

            Assert.NotNull(inDb);
            Assert.True(inDb!.IsDeleted);
        }

        [Fact]
        public async Task ListAsync_ShouldReturnPagedResultWithOptionalFilter()
        {
            // arrange
            var mongoFileServiceMock = new Mock<IMongoFileService>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "FileServiceTests_List")
                .Options;

            await using var context = new AppDbContext(options);
            // 插入几条测试数据
            context.AppFiles.AddRange(
                new AppFile { Id = "1", FileName = "hello.txt", SizeInBytes = 1 },
                new AppFile { Id = "2", FileName = "world.txt", SizeInBytes = 2 },
                new AppFile { Id = "3", FileName = "hello_world.txt", SizeInBytes = 3 }
            );
            await context.SaveChangesAsync();

            var unitOfWork = new UnitOfWork<AppDbContext>(context);
            var service = new FileService(mongoFileServiceMock.Object, unitOfWork);

            // act: 过滤包含 "hello" 的文件，分页大小 2
            var page = await service.ListAsync("hello", page: 1, pageSize: 2);

            // assert
            Assert.Equal(1, page.Page);
            Assert.Equal(2, page.PageSize);
            Assert.Equal(2, page.Total); // "hello.txt" 和 "hello_world.txt"
            Assert.Equal(2, page.List.Count);
            Assert.All(page.List, f => Assert.Contains("hello", f.FileName));
        }
    }
}