# ADR-06: AutoMapper para Mapeamento de DTOs

## Status
Implemented

## Date
2025-01-01

## Context
Handlers de produto e categoria tinham mapeamentos manuais (`new ProdutoDto { ... }`) e métodos estáticos `MapToProdutoDto()` duplicados em cada handler. Qualquer novo campo na entidade exige atualização em múltiplos handlers.

## Decision
Usar **AutoMapper** para centralizar mapeamentos em `MappingProfile`, injetando `IMapper` nos handlers.

**Regra obrigatória:** Sempre usar a sobrecarga com dois type params:
```csharp
mapper.Map<TSource, TDestination>(source)
```
Nunca usar `mapper.Map<TDestination>(source)` (omite TSource — reduz clareza e dificulta debugging).

### Pacote

```bash
dotnet add src/Neostore.Application/Neostore.Application.csproj package AutoMapper
```

> Não instalar `AutoMapper.Extensions.Microsoft.DependencyInjection` — AutoMapper 13+ registra diretamente via `AddAutoMapper()`.

### MappingProfile (`Neostore.Application/Mappings/MappingProfile.cs`)

```csharp
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

Todos os campos têm nomes idênticos entre entidade e DTO — zero `.ForMember()` necessário.

### Registro DI (`Neostore.Application/DependencyInjection.cs`)

```csharp
services.AddAutoMapper(typeof(DependencyInjection).Assembly);
```

Escaneia o assembly e registra todos os `Profile` automaticamente.

### Handlers Refatorados

**Categoria** (4 handlers):
- `CriarCategoriaCommandHandler`, `AtualizarCategoriaCommandHandler`, `ObterCategoriaPorIdQueryHandler`:
  ```csharp
  return mapper.Map<Categoria, CategoriaDto>(categoria);
  ```
- `ObterTodasCategoriasQueryHandler`:
  ```csharp
  return mapper.Map<List<Categoria>, List<CategoriaDto>>(categorias);
  ```

**Produto** (5 handlers) — métodos estáticos `MapToProdutoDto()` removidos:
- `CriarProdutoCommandHandler`, `AtualizarProdutoCommandHandler`, `ObterProdutoPorIdQueryHandler`:
  ```csharp
  return mapper.Map<Produto, ProdutoDto>(produto);
  ```
- `ObterTodosProdutosQueryHandler`:
  ```csharp
  return mapper.Map<List<Produto>, List<ProdutoDto>>(produtos);
  ```
- `ObterProdutosPaginadoQueryHandler`:
  ```csharp
  Itens = mapper.Map<List<Produto>, List<ProdutoDto>>(produtos)
  ```

O mapeamento `Imagem → ImagemDto` é feito automaticamente pelo profile quando `Produto → ProdutoDto` é mapeado.

### Testes

Handlers que recebem `IMapper` usam instância real (não Moq):
```csharp
IMapper mapper = new MapperConfiguration(cfg =>
    cfg.AddProfile<MappingProfile>()).CreateMapper();
```

Passar `mapper` direto testa o profile real junto ao handler.

## Consequences
### Positivo
- Mapeamentos centralizados — adicionar campo na entidade requer mudança apenas no `MappingProfile`.
- Elimina métodos estáticos `MapToProdutoDto()` duplicados.
- Testes cobrem o profile real, não um mock.

### Trade-offs
- AutoMapper adiciona dependência externa.
- Erros de mapeamento surgem em runtime se o profile estiver incompleto (mitigado por `AssertConfigurationIsValid()` em testes).

## Checklist de Implementação

- [x] AutoMapper instalado em `Neostore.Application`
- [x] `MappingProfile` criado com 3 maps (Categoria, Produto, Imagem)
- [x] `AddAutoMapper()` registrado em `DependencyInjection.cs`
- [x] 4 handlers de Categoria refatorados
- [x] 5 handlers de Produto refatorados
- [x] Métodos estáticos `MapToProdutoDto()` removidos
- [x] Testes atualizados com `MapperConfiguration` real
- [x] `dotnet build` sem warnings
- [x] `dotnet test` verde
