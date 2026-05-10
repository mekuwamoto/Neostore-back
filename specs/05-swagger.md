# Plano de Implementação: Swagger / OpenAPI

## Estado atual

| Item | Estado |
| :--- | :----- |
| `Microsoft.AspNetCore.OpenApi` 10.0.7 | ✅ instalado |
| `AddOpenApi()` em `DependencyInjection.cs` | ✅ registrado |
| `MapOpenApi()` em `Startup.cs` | ❌ ausente (spec nunca exposta) |
| UI (Scalar / Swashbuckle) | ❌ ausente |
| `[ProducesResponseType]` nos controllers | ❌ ausente |
| Metadados (título, versão, descrição) | ❌ ausente |

---

## Decisão de UI

**Scalar** é a escolha para este projeto:

- Compatível nativamente com o novo pipeline `Microsoft.AspNetCore.OpenApi` do .NET 9/10
- Swashbuckle não suporta oficialmente .NET 10 (usa middleware legado de geração de spec)
- UI moderna, sem dependência do Swashbuckle.AspNetCore

```bash
# Em src/Neostore.Api
dotnet add package Scalar.AspNetCore
```

---

## Passo 1 — Instalar Scalar

```bash
cd src/Neostore.Api
dotnet add package Scalar.AspNetCore
```

---

## Passo 2 — Configurar metadados do OpenAPI

**`Neostore.Api/Services/DependencyInjection.cs`** — substituir `AddOpenApi()` por:

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

---

## Passo 3 — Expor spec e UI em `Startup.cs`

```csharp
using Scalar.AspNetCore;

// Após builder.Build(), antes de MapControllers():
app.MapOpenApi();              // /openapi/v1.json
app.MapScalarApiReference();   // /scalar/v1
```

Resultado:
- Spec JSON: `GET /openapi/v1.json`
- UI interativa: `GET /scalar/v1`

---

## Passo 4 — Anotar controllers com `[ProducesResponseType]`

### `CategoriaController`

| Action | Códigos |
| :----- | :------ |
| `POST Criar` | 201 `CategoriaDto`, 400, 422 |
| `GET ObterArvore` | 200 `List<CategoriaDto>` |
| `GET ObterPorId` | 200 `CategoriaDto`, 404 |
| `PUT Atualizar` | 200 `CategoriaDto`, 400, 404 |
| `DELETE Deletar` | 204, 400, 404 |

```csharp
[HttpPost]
[ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
public async Task<ActionResult<CategoriaDto>> Criar(...)

[HttpGet]
[ProducesResponseType(typeof(List<CategoriaDto>), StatusCodes.Status200OK)]
public async Task<ActionResult<List<CategoriaDto>>> ObterArvore()

[HttpGet("{id}")]
[ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<CategoriaDto>> ObterPorId(Guid id)

[HttpPut("{id}")]
[ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<CategoriaDto>> Atualizar(...)

[HttpDelete("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Deletar(Guid id)
```

### `ProdutoController`

| Action | Códigos |
| :----- | :------ |
| `POST Criar` | 201 `ProdutoDto`, 400, 422 |
| `GET ObterPaginado` | 200 `ProdutosPaginadoDto` |
| `GET ObterPorId` | 200 `ProdutoDto`, 404 |
| `PUT Atualizar` | 200 `ProdutoDto`, 400, 404 |
| `PATCH AjustarEstoque` | 200 `{ estoque: int }`, 400, 404 |
| `DELETE Deletar` | 204, 404 |

```csharp
[HttpPost]
[ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
public async Task<ActionResult<ProdutoDto>> Criar(...)

[HttpGet]
[ProducesResponseType(typeof(ProdutosPaginadoDto), StatusCodes.Status200OK)]
public async Task<ActionResult<ProdutosPaginadoDto>> ObterPaginado(...)

[HttpGet("{id}")]
[ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<ProdutoDto>> ObterPorId(Guid id)

[HttpPut("{id}")]
[ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<ProdutoDto>> Atualizar(...)

[HttpPatch("{id}/estoque")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<int>> AjustarEstoque(...)

[HttpDelete("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Deletar(Guid id)
```

---

## Sequência de execução

```
1. Passo 1 → dotnet add package Scalar.AspNetCore
2. Passo 2 → Configurar metadados (título, versão) em AddOpenApi()
3. Passo 3 → MapOpenApi() + MapScalarApiReference() em Startup.cs
4. Passo 4 → [ProducesResponseType] em CategoriaController
5. Passo 4 → [ProducesResponseType] em ProdutoController
6. dotnet build — verificar 0 erros
7. dotnet run — acessar /scalar/v1 e validar spec
```

---

## Resultado esperado

| URL | Conteúdo |
| :-- | :------- |
| `GET /openapi/v1.json` | Spec OpenAPI 3.x gerada automaticamente |
| `GET /scalar/v1` | UI interativa com todos os endpoints documentados |
