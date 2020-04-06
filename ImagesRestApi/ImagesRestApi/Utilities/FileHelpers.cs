using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using ImagesRestApi.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using File = ImagesRestApi.Models.File;

namespace ImagesRestApi.Utilities
{
    public static class FileHelpers
    {
        private const int MegabyteSize = 1048576;
        // For more file signatures, see the ProcessedStreamedFile Signatures Database (https://www.filesignatures.net/)
        // and the official specifications for the file types you wish to add.
        private static readonly Dictionary<string, List<byte[]>> FileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".zip", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
        };

        // **WARNING!**
        // In the following file processing methods, the file's content isn't scanned.
        // In most production scenarios, an anti-virus/anti-malware scanner API is
        // used on the file before making the file available to users or other
        // systems. For more information, see the topic that accompanies this sample
        // app.
        private static void ValidateFileSize(long size, long sizeLimit)
        {
            if (size == 0) throw new InvalidDataException("The file is empty.");
            if (size > sizeLimit)
            {
                var megabyteSizeLimit = sizeLimit / MegabyteSize;
                throw new InvalidDataException($"The file exceeds {megabyteSizeLimit:N1} MB.");
            }
        }

        public static async Task<ProcessedStreamedFile> ProcessStreamedFile(MultipartSection section, ContentDispositionHeaderValue contentDisposition,
            List<string> permittedExtensions, long sizeLimit)
        {
            await using var memoryStream = new MemoryStream();
            await section.Body.CopyToAsync(memoryStream);
            // Check if the file is empty or exceeds the size limit.
            ValidateFileSize(memoryStream.Length, sizeLimit);
            var fileExtension = GetFileExtension(contentDisposition.FileName.Value);
            if (!IsValidFileExtensionAndSignature(fileExtension, memoryStream, permittedExtensions))
                throw new InvalidDataException("The file type isn't permitted or the file's signature doesn't match the file's extension.");

            return new ProcessedStreamedFile()
            {
                Content = memoryStream.ToArray(),
                Extension = fileExtension
            };
        }

        public static async Task<ProcessedStreamedFile> ProcessStreamedFile(PipeReader reader, string contentType,
            List<string> permittedExtensions, long sizeLimit, IContentTypeProvider contentTypeProvider)
        {
            await using var memoryStream = new MemoryStream();
            await reader.CopyToAsync(memoryStream);
            // Check if the file is empty or exceeds the size limit.
            ValidateFileSize(memoryStream.Length, sizeLimit);
            var extensionBySignature = GetFileExtensionBySignature(memoryStream);
            if (!permittedExtensions.Contains(extensionBySignature) 
                || !contentTypeProvider.TryGetContentType(extensionBySignature, out var contentTypeBySignature) 
                || contentTypeBySignature != contentType)
                throw new InvalidDataException("The file type isn't permitted or the file's signature doesn't match the file's extension.");
            return new ProcessedStreamedFile()
            {
                Content = memoryStream.ToArray(),
                Extension = extensionBySignature
            };
        }

        public static ProcessedStreamedFile ProcessFile(File file, List<string> permittedExtensions, long sizeLimit)
        {
            // Check if the file is empty or exceeds the size limit.
            ValidateFileSize(file.Content.Length, sizeLimit);
            var extensionBySignature = GetFileExtensionBySignature(file);
            if (!permittedExtensions.Contains(extensionBySignature))
                throw new InvalidDataException("The file type isn't permitted or the file's signature doesn't match the file's extension.");
            return new ProcessedStreamedFile()
            {
                Content = file.Content,
                Extension = extensionBySignature
            };
        }
        private static bool IsValidFileExtensionAndSignature(string fileExtension, Stream data, List<string> permittedExtensions)
        {
            if (data == null || data.Length == 0) return false;

            if (string.IsNullOrEmpty(fileExtension) || !permittedExtensions.Contains(fileExtension)) return false;

            data.Position = 0;

            using var reader = new BinaryReader(data);
            /* ProcessedStreamedFile signature check
             --------------------
             With the file signatures provided in the FileSignature
             dictionary, the following code tests the input content's
             file signature.
            */
            var signatures = FileSignature[fileExtension];
            var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

            return signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
        }
        private static string GetFileExtension(string fileName) => string.IsNullOrEmpty(fileName) ? null : Path.GetExtension(fileName).ToLowerInvariant();
        private static string GetFileExtensionBySignature(Stream data)
        {
            data.Position = 0;
            using var reader = new BinaryReader(data);
            var maxSignatureLength = FileSignature.Max(signatures => signatures.Value.Max(signature => signature.Length));
            var headerBytes = reader.ReadBytes(maxSignatureLength);
            var fileSignature = FileSignature.FirstOrDefault(signatures =>
                signatures.Value.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature)));
            return fileSignature.Key;
        }
        private static string GetFileExtensionBySignature(File file)
        {
            var maxSignatureLength = FileSignature.Max(signatures => signatures.Value.Max(signature => signature.Length));
            var signatureData = file.Content.Take(maxSignatureLength);
            var fileSignature = FileSignature.FirstOrDefault(signatures =>
                signatures.Value.Any(signature => signatureData.Take(signature.Length).SequenceEqual(signature)));
            return fileSignature.Key;
        }
    }
}
