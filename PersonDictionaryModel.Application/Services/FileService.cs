using Microsoft.AspNetCore.Http;
using PersonDictionaryModel.Core.Application.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PersonDictionaryModel.Core.Application.Services
{
    public sealed class FileService : IFileService
    {
        public async Task<string> GetFileAsync(string rootPath, string fileName, string fileType)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileType))
                return string.Empty;

            var bytes = await File.ReadAllBytesAsync($"{rootPath}/{fileName}.{fileType}");

            return Convert.ToBase64String(bytes);
        }

        public async Task SaveFileAsync(byte[] cotnent, string rootPath, string fileName, string fileType)
        {
            await File.WriteAllBytesAsync($"{rootPath}/{fileName}.{fileType}", cotnent);
        }

        public async Task<byte[]> GetBytesAsync(IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
