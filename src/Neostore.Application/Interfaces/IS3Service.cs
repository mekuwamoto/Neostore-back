using Microsoft.AspNetCore.Http;

namespace Neostore.Application.Interfaces;

public interface IS3Service
{
    Task<ImagemUploadResultado> UploadAsync(
        IFormFile arquivo,
        string prefixo,
        CancellationToken cancellationToken = default);

    Task DeletarAsync(string chaveS3, CancellationToken cancellationToken = default);
}

public record ImagemUploadResultado(
    string ChaveS3,
    string NomeArquivo,
    string TipoConteudo,
    long TamanhoBytes);
