using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using PKFAuditManagement.Interface;
using System;
using System.IO;
using System.Threading.Tasks;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly IConfiguration _configuration;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _bucketName = _configuration["AWS_BUCKET_NAME"];
    }

    // Method to upload a file to the S3 bucket
    public async Task UploadFileAsync(string key, Stream inputStream)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = inputStream,
            ContentType = "application/octet-stream"
        };

        await _s3Client.PutObjectAsync(putRequest);
    }

    // Method to download a file from the S3 bucket
    public async Task<Stream> DownloadFileAsync(string key)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _s3Client.GetObjectAsync(getRequest);
        return response.ResponseStream;
    }

    // Method to delete a file from the S3 bucket
    public async Task DeleteFileAsync(string key)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(deleteRequest);
    }

    // Method to generate a pre-signed URL for accessing a file
    public string GeneratePreSignedUrl(string key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(30) // URL expiration time
        };

        return _s3Client.GetPreSignedURL(request);
    }
}
