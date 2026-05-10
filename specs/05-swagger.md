# ADR-05: Documentação OpenAPI com Scalar

## Status
Implemented

## Date
2025-01-01

## Context
API precisa de documentação interativa para desenvolvimento e testes. `.NET 10` introduziu novo pipeline `Microsoft.AspNetCore.OpenApi` que diverge do Swashbuckle legado. Swashbuckle não suporta oficialmente .NET 10.

## Decision
Usar **`Microsoft.AspNetCore.OpenApi`** (nativo .NET 10) para geração de spec + **Scalar** como UI interativa.

### Justificativa: Scalar vs Swashbuckle

| Critério | Scalar | Swashbuckle |
| -------- | ------ | ----------- |
| Compatibilidade .NET 10 | ✅ Nativo | ❌ Não suportado oficialmente |
| Pipeline de geração | Novo pipeline ASP.NET Core | Middleware legado |
| UI | Moderna | Clássica |

### Pacotes

```bash
# Neostore.Api
dotnet add package Scalar.AspNetCore
# Microsoft.AspNetCore.OpenApi 10.0.7 já instalado
```

### Configuração (`DependencyInjection.cs`)

```csharp
services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Title = "Neostore API",
            Version = "v1",
            Description = "API administrativa do Neostore — gestão de produtos, categorias e usuários."
        };
        return Task.CompletedTask;
    });
});
```

### Exposição (`Startup.cs`)

```csharp
using Scalar.AspNetCore;

app.MapOpenApi();             // GET /openapi/v1.json
app.MapScalarApiReference();  // GET /scalar/v1
```

### Anotações nos Controllers

**CategoriaController:**

| Action | Códigos HTTP |
| ------ | ------------ |
| `POST Criar` | 201 `CategoriaDto`, 400, 422 |
| `GET ObterArvore` | 200 `List<CategoriaDto>` |
| `GET ObterPorId` | 200 `CategoriaDto`, 404 |
| `PUT Atualizar` | 200 `CategoriaDto`, 400, 404 |
| `DELETE Deletar` | 204, 400, 404 |

**ProdutoController:**

| Action | Códigos HTTP |
| ------ | ------------ |
| `POST Criar` | 201 `ProdutoDto`, 400, 422 |
| `GET ObterPaginado` | 200 `ProdutosPaginadoDto` |
| `GET ObterPorId` | 200 `ProdutoDto`, 404 |
| `PUT Atualizar` | 200 `ProdutoDto`, 400, 404 |
| `PATCH AjustarEstoque` | 200 `int`, 400, 404 |
| `DELETE Deletar` | 204, 404 |

## Consequences
### Positivo
- Spec OpenAPI 3.x gerada automaticamente sem configuração extra.
- UI Scalar moderna com suporte nativo ao pipeline .NET 10.
- `[ProducesResponseType]` documenta contrato de cada endpoint na spec.

### Trade-offs
- Scalar menos conhecido que Swagger UI — curva de aprendizado mínima.

## URLs

| URL | Conteúdo |
| --- | -------- |
| `GET /openapi/v1.json` | Spec OpenAPI 3.x |
| `GET /scalar/v1` | UI interativa |
