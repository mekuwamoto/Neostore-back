using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Neostore.Application.Interfaces;
using Neostore.Infrastructure.Options;

namespace Neostore.Infrastructure.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _options;

    public S3Service(IAmazonS3 s3Client, IOptions<S3Options> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<ImagemUploadResultado> UploadAsync(
        IFormFile arquivo,
        string prefixo,
        CancellationToken cancellationToken = default)
    {
        string extensao = Path.GetExtension(arquivo.FileName);
        string chaveS3 = $"{prefixo}/{Guid.NewGuid()}{extensao}";

        using Stream stream = arquivo.OpenReadStream();

        PutObjectRequest request = new()
        {
            BucketName = _options.BucketImagens,
            Key = chaveS3,
            InputStream = stream,
            ContentType = arquivo.ContentType,
            AutoCloseStream = false
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        return new ImagemUploadResultado(
            chaveS3,
            arquivo.FileName,
            arquivo.ContentType,
            arquivo.Length);
    }

    public async Task DeletarAsync(string chaveS3, CancellationToken cancellationToken = default)
    {
        DeleteObjectRequest request = new()
        {
            BucketName = _options.BucketImagens,
            Key = chaveS3
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);
    }
}
