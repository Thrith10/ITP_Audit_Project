namespace PKFAuditManagement.Interface
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IS3Service
    {
        Task UploadFileAsync(string key, Stream inputStream);
        Task<Stream> DownloadFileAsync(string key);
        Task DeleteFileAsync(string key);
        string GeneratePreSignedUrl(string key);
    }
}
