using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PersonDictionaryModel.Core.Application.Interfaces
{
    public interface IFileService
    {
        Task SaveFileAsync(byte[] cotnent, string rootPath, string fileName, string fileType);
        Task<string> GetFileAsync(string rootPath, string fileName, string fileType);
        Task<byte[]> GetBytesAsync(IFormFile formFile);
    }
}
