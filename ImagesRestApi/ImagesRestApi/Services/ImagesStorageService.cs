using System;
using System.Threading.Tasks;
using ImagesRestApi.DTO;
using ImagesRestApi.Models;
using ImagesRestApi.Services.Interfaces;
using ImagesRestApi.Wrappers;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;

namespace ImagesRestApi.Services
{
    public class ImagesStorageService : IImagesStorageService
    {
        //wrappers
        private readonly IDirectoryWrapper _directory;
        private readonly IFileWrapper _file;
        private readonly IPathWrapper _path;

        private readonly string _targetFilePath;


        public ImagesStorageService(IConfiguration config, IDirectoryWrapper directory, IFileWrapper file, IPathWrapper path,
            IContentDispositionHeaderValueWrapper contentDispositionHeaderValue, IContentTypeProvider contentTypeProvider)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _file = file ?? throw new ArgumentNullException(nameof(file));
            _path = path ?? throw new ArgumentNullException(nameof(path));

            _targetFilePath = config.GetValue<string>("StoredFilesPath");
        }

        public async Task<ImageDTO> SaveImage(ProcessedStreamedFile file, Guid fileId)
        {
            var folderName = Guid.NewGuid().ToString();// Don't trust the file name sent by the client. To display
            var trustedFileNameForFileStorage = $"original{file.Extension}";//var fileFolder = $"{_targetFilePath}\\{fileId}";
            var filePath = _path.Combine(_targetFilePath, folderName, trustedFileNameForFileStorage);//$"{fileFolder}\\{trustedFileNameForFileStorage}";

            _directory.CreateDirectory(filePath);
            await using (var targetStream = _file.Create(filePath))
                await targetStream.WriteAsync(file.Content);
            return new ImageDTO()
            {
                Id = fileId,
                Path = filePath
            };
        }

        public Task<byte[]> GetImage(ImageDTO image) => image == null || !_directory.Exists(image.Path) ? null : _file.ReadAllBytesAsync(image.Path);
        public void DeleteImageDirectory(ImageDTO image) => _directory.Delete(_path.GetDirectoryName(image.Path), true);
    }
}
