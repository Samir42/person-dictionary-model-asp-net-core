using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace PersonDictionaryModel.Core.Application.Interfaces
{
    public interface IFileService
    {
        Task<MemoryStream> GetStreamAsync(IFormFile formFile);
    }
}
