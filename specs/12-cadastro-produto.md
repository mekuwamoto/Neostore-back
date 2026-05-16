# ADR-12: Cadastro de Produto com Upload de Imagens via S3

## Status
Proposed

## Date
2026-05-14

## Context

O endpoint `POST /api/admin/produtos` já existe e o `CriarProdutoCommand` já aceita `List<ImagemInputDto>` com campos `NomeArquivo`, `ChaveS3`, `TipoConteudo` e `TamanhoBytes`. No entanto, o fluxo atual supõe que o frontend fornece a `ChaveS3` já calculada — o que transfere responsabilidade de storage para o cliente e expõe detalhes de infraestrutura.

O requisito correto é:
- Frontend envia arquivos binários de imagem (obrigatório — mínimo 1)
- Backend faz upload das imagens para o bucket S3 `neostore-imagens` (ministack local)
- Backend persiste apenas metadados de localização no banco de dados — sem binários

Entidade `Imagem` já modela essa intenção: campos `ChaveS3`, `NomeArquivo`, `TipoConteudo`, `TamanhoBytes` e `DataCriacao` existem sem campo de dados binários.

## Decision

### Fluxo de Cadastro

```
Frontend
  │
  │  POST /api/admin/produtos
  │  Content-Type: multipart/form-data
  │  { nome, sku, preco, idCategoria, descricao, estoque, imagens: [File...] }
  │
  ▼
ProdutoController
  │  (valida MIME/tamanho — boundary HTTP)
  │
  └─► _mediator.Send(CriarProdutoCommand { ..., Imagens: List<IFormFile> })
        │
        ▼
      CriarProdutoCommandHandler
        │
        ├─► IS3Service.UploadAsync(arquivo) → ImagemUploadResultado
        │   (para cada IFormFile)
        │
        ├─► Constrói entidades Imagem com metadados retornados
        │
        └─► _produtoRepository.CriarAsync(produto) → ProdutoDto
```

Backend nunca devolve binários — apenas metadados (`ChaveS3`, `NomeArquivo`, `TipoConteudo`, `TamanhoBytes`) que o frontend usa para montar a URL da imagem se necessário.

### Interface IS3Service

Definida em `Neostore.Application/Interfaces/IS3Service.cs`:

```csharp
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
```

`IFormFile` vem de `Microsoft.AspNetCore.Http` — `Neostore.Application` já aceita referências ao ASP.NET Core abstractions pois não referencia nenhum concern de infraestrutura.

> **Alternativa descartada:** definir interface em `Neostore.Domain` — Domain não deve conhecer `IFormFile` (concern de HTTP).

### Command — CriarProdutoCommand

`Imagens` passa de `List<ImagemInputDto>` para `List<IFormFile>` — o command carrega os arquivos brutos:

```csharp
public record CriarProdutoCommand(
    string Nome,
    string SKU,
    decimal Preço,
    Guid IdCategoria,
    string Descrição,
    int Estoque,
    List<IFormFile> Imagens
) : IRequest<ProdutoDto>;
```

`ImagemInputDto` deixa de existir — era necessária apenas quando o controller fazia o upload.

### Handler — CriarProdutoCommandHandler

Handler injeta `IS3Service` e realiza o upload antes de persistir:

```csharp
public class CriarProdutoCommandHandler : IRequestHandler<CriarProdutoCommand, ProdutoDto>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IS3Service _s3Service;
    private readonly IMapper _mapper;

    public async Task<ProdutoDto> Handle(CriarProdutoCommand request, CancellationToken cancellationToken)
    {
        if (await _produtoRepository.ExistePorSkuAsync(request.SKU))
            throw new InvalidOperationException($"SKU '{request.SKU}' já existe.");

        Categoria? categoria = await _categoriaRepository.ObterPorIdAsync(request.IdCategoria);
        if (categoria == null)
            throw new InvalidOperationException("Categoria não encontrada.");

        Produto produto = new()
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            SKU = request.SKU,
            Preço = request.Preço,
            IdCategoria = request.IdCategoria,
            Descrição = request.Descrição,
            Estoque = request.Estoque
        };

        foreach (IFormFile arquivo in request.Imagens)
        {
            ImagemUploadResultado resultado = await _s3Service.UploadAsync(
                arquivo,
                prefixo: "produtos",
                cancellationToken);

            produto.AdicionarImagem(new Imagem
            {
                Id = Guid.NewGuid(),
                NomeArquivo = resultado.NomeArquivo,
                ChaveS3 = resultado.ChaveS3,
                TipoConteudo = resultado.TipoConteudo,
                TamanhoBytes = resultado.TamanhoBytes,
                DataCriacao = DateTime.UtcNow
            });
        }

        await _produtoRepository.CriarAsync(produto);

        return _mapper.Map<Produto, ProdutoDto>(produto);
    }
}
```

### Implementação S3Service

Em `Neostore.Infrastructure/Services/S3Service.cs`:

```csharp
public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _options;

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
```

### Opções de Configuração

Em `Neostore.Infrastructure/Options/S3Options.cs`:

```csharp
public class S3Options
{
    public const string SectionName = "S3";

    public string ServiceUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string BucketImagens { get; set; } = string.Empty;
}
```

Em `appsettings.Development.json`:

```json
"S3": {
  "ServiceUrl": "http://localhost:4566",
  "AccessKey": "test",
  "SecretKey": "test",
  "Region": "us-east-1",
  "BucketImagens": "neostore-imagens"
}
```

Em `appsettings.json` (produção — valores via variáveis de ambiente ou secrets):

```json
"S3": {
  "ServiceUrl": "",
  "AccessKey": "",
  "SecretKey": "",
  "Region": "us-east-1",
  "BucketImagens": "neostore-imagens"
}
```

### Registro em DI (Infrastructure)

Em `Neostore.Infrastructure/DependencyInjection.cs`:

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    S3Options s3Options = configuration.GetSection(S3Options.SectionName).Get<S3Options>()!;
    services.Configure<S3Options>(configuration.GetSection(S3Options.SectionName));

    AmazonS3Config s3Config = new()
    {
        ServiceURL = s3Options.ServiceUrl,
        ForcePathStyle = true   // obrigatório para ministack/LocalStack
    };

    BasicAWSCredentials credentials = new(s3Options.AccessKey, s3Options.SecretKey);
    AmazonS3Client s3Client = new(credentials, s3Config);

    services.AddSingleton<IAmazonS3>(s3Client);
    services.AddScoped<IS3Service, S3Service>();

    return services;
}
```

### Controller — Alteração do Endpoint de Criação

`POST /api/admin/produtos` passa a aceitar `multipart/form-data`. Controller não injeta `IS3Service` — responsabilidade exclusiva do handler. Controller valida apenas boundary HTTP (MIME e tamanho) antes de enviar o command:

```csharp
[HttpPost]
[Consumes("multipart/form-data")]
[ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<ProdutoDto>> Criar(
    [FromForm] CriarProdutoRequest request,
    CancellationToken cancellationToken)
{
    foreach (IFormFile arquivo in request.Imagens)
    {
        if (!TiposPermitidos.Contains(arquivo.ContentType))
            return BadRequest(new { erro = $"Tipo de arquivo não permitido: {arquivo.ContentType}" });

        if (arquivo.Length > TamanhoMaximoBytes)
            return BadRequest(new { erro = "Arquivo excede o tamanho máximo de 5 MB." });
    }

    CriarProdutoCommand command = new(
        request.Nome,
        request.SKU,
        request.Preço,
        request.IdCategoria,
        request.Descrição,
        request.Estoque,
        request.Imagens);

    ProdutoDto resultado = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
}
```

`CriarProdutoRequest` — campos de texto + arquivos:

```csharp
public record CriarProdutoRequest(
    string Nome,
    string SKU,
    decimal Preço,
    Guid IdCategoria,
    string Descrição,
    int Estoque,
    List<IFormFile> Imagens);
```

Constantes no controller:

```csharp
private static readonly string[] TiposPermitidos = ["image/jpeg", "image/png", "image/webp"];
private const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB
```

### Chave S3

Formato: `produtos/{guid}{extensao}`

Exemplos:
- `produtos/3f7a1b2c-...-4e9d.jpg`
- `produtos/a1b2c3d4-...-5f6e.png`

### Validator — CriarProdutoCommand

`CriarProdutoCommandValidator` valida que `Imagens` não está vazia:

```csharp
RuleFor(x => x.Imagens)
    .NotEmpty().WithMessage("Pelo menos uma imagem é obrigatória.");
```

Validação de MIME e tamanho não ocorre no validator — o command agora carrega `IFormFile`, mas essas regras são de boundary HTTP e ficam no controller (executadas antes de `_mediator.Send`).

### Pacotes NuGet

```bash
dotnet add src/Neostore.Infrastructure/Neostore.Infrastructure.csproj package AWSSDK.S3
```

Nenhum pacote novo em `Neostore.Application` — `IS3Service` usa apenas `IFormFile` do ASP.NET Core abstractions já disponível transitivamente.

### Migration

Não necessária — tabela `Imagens` já existe na migration `InitialCreate` com colunas `NomeArquivo`, `ChaveS3`, `TipoConteudo`, `TamanhoBytes`, `IdProduto`, `DataCriacao`.

### Bucket — Criação no Ministack

O bucket `neostore-imagens` deve ser criado no ministack antes do primeiro uso. Adicionar ao `docker-compose.yml` ou executar manualmente:

```bash
aws --endpoint-url=http://localhost:4566 s3 mb s3://neostore-imagens
```

## Consequences

### Positivo
- Frontend envia apenas arquivos — não conhece detalhes de S3.
- Binários nunca persistidos no banco de dados.
- `IS3Service` desacoplado via interface — testável com mock.
- `S3Service` isolado em Infrastructure — Application permanece sem dependência de AWS SDK.
- Chave S3 gerada no backend garante unicidade (GUID).

### Trade-offs
- Handler orquestra upload S3 + persistência em sequência sem transação distribuída: se `SaveChangesAsync` falhar após uploads concluídos, imagens ficam órfãs no S3. Aceitável para MVP — rollback pode ser adicionado via outbox pattern futuramente.
- `IFormFile` no command e na interface `IS3Service` acopla Application a ASP.NET Core abstractions. Alternativa (usar `Stream` + metadados separados) aumenta complexidade sem benefício real no contexto deste projeto.
- Controller ainda tem lógica de validação de tipo/tamanho — exceção justificada por ser validação de HTTP boundary, não de negócio.

## Decisões de Design

| Decisão | Escolha | Alternativa descartada |
| ------- | ------- | ---------------------- |
| Onde ocorre upload S3 | Handler (`CriarProdutoCommandHandler`) | Controller — misturaria infraestrutura com roteamento HTTP |
| Interface IS3Service | Em Application | Em Domain — Domain não deve conhecer IFormFile |
| Formato da chave S3 | `produtos/{guid}{ext}` | `produtos/{idProduto}/{guid}{ext}` — Id do produto não existe no momento do upload |
| Binários no banco | Nunca — apenas metadados | Salvar BLOB — inviável para performance e custo |
| Validação de tipo/tamanho | No controller | No validator do command — command já recebe metadados, não arquivos |
| ForcePathStyle S3 | `true` | `false` — ministack/LocalStack requer path-style |

## Sequência de Implementação

1. Instalar `AWSSDK.S3` em `Neostore.Infrastructure`
2. Criar `S3Options` em `Neostore.Infrastructure/Options/`
3. Criar interface `IS3Service` + record `ImagemUploadResultado` em `Neostore.Application/Interfaces/`
4. Implementar `S3Service` em `Neostore.Infrastructure/Services/`
5. Registrar `IAmazonS3` e `IS3Service` em `Neostore.Infrastructure/DependencyInjection.cs`
6. Adicionar config S3 em `appsettings.json` e `appsettings.Development.json`
7. Criar record `CriarProdutoRequest` em `Neostore.Api/`
8. Atualizar `CriarProdutoCommand`: substituir `List<ImagemInputDto>` por `List<IFormFile>`; remover `ImagemInputDto`
9. Atualizar `CriarProdutoCommandHandler`: injetar `IS3Service`, chamar `UploadAsync` por arquivo, construir entidades `Imagem` com metadados retornados
10. Atualizar `ProdutoController`: remover injeção de `IS3Service`, alterar `Criar` para `[FromForm]`, adicionar validação de MIME/tamanho, passar `IFormFile` list diretamente no command
11. Atualizar `CriarProdutoCommandValidator`: adicionar regra `Imagens NotEmpty`
12. Criar bucket no ministack: `aws --endpoint-url=http://localhost:4566 s3 mb s3://neostore-imagens`

