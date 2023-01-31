using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Calicot.Shared.Models;
using Microsoft.AspNetCore.Http;


namespace Calicot.Shared.Services;

public interface IBlobStorageService
{
    Task<MemoryStream> GetBlobDataAsync(string fileName, string containerName);
    Task<string> GetContentTypeAsync(string fileName, string containerName);
    Task<string> UploadFileToBlobAsync(string strFileName, IFormFile file, string fileMimeType, string containerName);
    Task<string> UploadFileStreamToBlobAsync(string fileName, FileStream file, string fileMimeType, string containerName);
    Task<string> UploadMemoryStreamToBlobAsync(string fileName, MemoryStream file, string fileMimeType, string containerName);
    void DeleteBlobDataAsync(string fileName);
}
