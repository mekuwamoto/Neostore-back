# 06 — AutoMapper

## Objetivo

Substituir todos os mapeamentos manuais (new DTO { ... }) por AutoMapper, centralizando as regras de conversão em `MappingProfile` e injetando `IMapper` nos handlers.

**Regra obrigatória:** Sempre usar a sobrecarga com dois type params:
```csharp
mapper.Map<TSource, TDestination>(source)
```
Nunca usar `mapper.Map<TDestination>(source)` (omite TSource).

---

## Passo 1 — Instalar pacote

Projeto: `Neostore.Application`

```bash
dotnet add src/Neostore.Application/Neostore.Application.csproj package AutoMapper
```

> Não instalar `AutoMapper.Extensions.Microsoft.DependencyInjection` — o AutoMapper 13+ registra direto via `AddAutoMapper()`.

---

## Passo 2 — Criar MappingProfile

Arquivo: `src/Neostore.Application/Mappings/MappingProfile.cs`

```csharp
using AutoMapper;
using Neostore.Application.DTOs;
using Neostore.Domain.Entities;

namespace Neostore.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Categoria, CategoriaDto>();
        CreateMap<Produto, ProdutoDto>();
        CreateMap<Imagem, ImagemDto>();
    }
}
```

Todos os campos têm nomes idênticos entre entidade e DTO → zero `.ForMember()` necessário.

---

## Passo 3 — Registrar no DI

Arquivo: `src/Neostore.Application/DependencyInjection.cs`

Adicionar dentro de `AddApplication()`:

```csharp
services.AddAutoMapper(typeof(DependencyInjection).Assembly);
```

Isso escaneia o assembly e registra todos os `Profile` automaticamente.

---

## Passo 4 — Refatorar handlers de Categoria

### 4.1 `CriarCategoriaCommandHandler`

- Injetar `IMapper _mapper` via construtor
- Substituir `new CategoriaDto { ... }` por:
  ```csharp
  return mapper.Map<Categoria, CategoriaDto>(categoria);
  ```

### 4.2 `AtualizarCategoriaCommandHandler`

- Injetar `IMapper _mapper` via construtor
- Substituir `new CategoriaDto { ... }` por:
  ```csharp
  return mapper.Map<Categoria, CategoriaDto>(categoria);
  ```

### 4.3 `ObterCategoriaPorIdQueryHandler`

- Injetar `IMapper _mapper` via construtor
- Substituir `new CategoriaDto { ... }` por:
  ```csharp
  return mapper.Map<Categoria, CategoriaDto>(categoria);
  ```

### 4.4 `ObterTodasCategoriasQueryHandler`

- Injetar `IMapper _mapper` via construtor
- Substituir projeção manual por:
  ```csharp
  return mapper.Map<List<Categoria>, List<CategoriaDto>>(categorias);
  ```

---

## Passo 5 — Refatorar handlers de Produto

Remover os métodos estáticos `MapToProdutoDto()` presentes em cada handler.

### 5.1 `CriarProdutoCommandHandler`

- Injetar `IMapper _mapper` via construtor
- Substituir chamada a `MapToProdutoDto(produto)` e o mapeamento manual de `Imagens` por:
  ```csharp
  return mapper.Map<Produto, ProdutoDto>(produto);
  ```
- O mapeamento de `Imagem → ImagemDto` é feito automaticamente pelo profile.

### 5.2 `AtualizarProdutoCommandHandler`

- Injetar `IMapper _mapper` via construtor
- Substituir chamada a `MapToProdutoDto(produto)` por:
  ```csharp
  return mapper.Map<Produto, ProdutoDto>(produto);
  ```

### 5.3 `ObterProdutoPorIdQueryHandler`

- Injetar `IMapper _mapper` via construtor
- Substituir `MapToProdutoDto(produto)` por:
  ```csharp
  return mapper.Map<Produto, ProdutoDto>(produto);
  ```

### 5.4 `ObterTodosProdutosQueryHandler`

- Injetar `IMapper _mapper` via construtor
- Substituir projeção manual por:
  ```csharp
  return mapper.Map<List<Produto>, List<ProdutoDto>>(produtos);
  ```

### 5.5 `ObterProdutosPaginadoQueryHandler`

- Injetar `IMapper _mapper` via construtor
- Substituir projeção da lista de itens por:
  ```csharp
  Itens = mapper.Map<List<Produto>, List<ProdutoDto>>(produtos)
  ```

---

## Passo 6 — Atualizar testes

Arquivo: `src/Neostore.Tests/`

Handlers que recebem `IMapper` precisam de mock nos testes unitários:

```csharp
IMapper mapper = new MapperConfiguration(cfg =>
    cfg.AddProfile<MappingProfile>()).CreateMapper();
```

Passar `mapper` direto (não mockar com Moq) — testa o profile real junto ao handler.

---

## Checklist

- [ ] Pacote AutoMapper instalado em `Neostore.Application`
- [ ] `MappingProfile` criado com 3 maps (Categoria, Produto, Imagem)
- [ ] `AddAutoMapper()` registrado em `DependencyInjection.cs`
- [ ] 4 handlers de Categoria refatorados
- [ ] 5 handlers de Produto refatorados
- [ ] Métodos estáticos `MapToProdutoDto()` removidos
- [ ] Testes atualizados com `MapperConfiguration` real
- [ ] `dotnet build` sem warnings
- [ ] `dotnet test` verde
