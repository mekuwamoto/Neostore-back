# ADR-11: AutoMapper — MappingProfile Explícito

**Status:** Implemented  
**Data:** 2026-05-14

## Contexto

`MappingProfile` atual usa `CreateMap<T1, T2>()` sem nenhum `.ForMember()` — AutoMapper resolve por convenção de nome. Isso oculta quais campos são mapeados, quais são ignorados e por quê. Verificar dependências entre entidade e DTO exige abrir os dois arquivos.

## Decisão

Reescrever `MappingProfile` com todos os campos explícitos: `.ForMember()` para cada campo mapeado, `.ForMember(..., opt => opt.Ignore())` para cada campo da entidade que não entra no DTO.

## Mapeamentos

### `CreateMap<Categoria, CategoriaDto>`

| Campo entidade | Tipo | Campo DTO | Tipo | Ação |
|---|---|---|---|---|
| `Id` | `Guid` | `Id` | `Guid` | MapFrom |
| `Nome` | `string` | `Nome` | `string` | MapFrom |
| `Slug` | `string` | `Slug` | `string` | MapFrom |
| `IdCategoriaPai` | `Guid?` | `IdCategoriaPai` | `Guid?` | MapFrom |

Nenhum campo ignorado — entidade `Categoria` não tem campos de soft-delete.

```csharp
CreateMap<Categoria, CategoriaDto>()
    .ForMember(dest => dest.Id,            opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.Nome,          opt => opt.MapFrom(src => src.Nome))
    .ForMember(dest => dest.Slug,          opt => opt.MapFrom(src => src.Slug))
    .ForMember(dest => dest.IdCategoriaPai, opt => opt.MapFrom(src => src.IdCategoriaPai));
```

---

### `CreateMap<Produto, ProdutoDto>`

| Campo entidade | Tipo | Campo DTO | Tipo | Ação |
|---|---|---|---|---|
| `Id` | `Guid` | `Id` | `Guid` | MapFrom |
| `Nome` | `string` | `Nome` | `string` | MapFrom |
| `SKU` | `string` | `SKU` | `string` | MapFrom |
| `Preço` | `decimal` | `Preço` | `decimal` | MapFrom |
| `IdCategoria` | `Guid` | `IdCategoria` | `Guid` | MapFrom |
| `Descrição` | `string?` | `Descrição` | `string` | MapFrom |
| `Imagens` | `List<Imagem>` | `Imagens` | `List<ImagemDto>` | MapFrom (requer `Imagem → ImagemDto`) |
| `Estoque` | `int` | `Estoque` | `int` | MapFrom |
| `Ativo` | `bool` | — | — | Ignore |
| `DeletadoEm` | `DateTime?` | — | — | Ignore |

```csharp
CreateMap<Produto, ProdutoDto>()
    .ForMember(dest => dest.Id,         opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.Nome,       opt => opt.MapFrom(src => src.Nome))
    .ForMember(dest => dest.SKU,        opt => opt.MapFrom(src => src.SKU))
    .ForMember(dest => dest.Preço,      opt => opt.MapFrom(src => src.Preço))
    .ForMember(dest => dest.IdCategoria, opt => opt.MapFrom(src => src.IdCategoria))
    .ForMember(dest => dest.Descrição,  opt => opt.MapFrom(src => src.Descrição))
    .ForMember(dest => dest.Imagens,    opt => opt.MapFrom(src => src.Imagens))
    .ForMember(dest => dest.Estoque,    opt => opt.MapFrom(src => src.Estoque));
```

> `Ativo` e `DeletadoEm` não precisam de `.Ignore()` explícito porque não existem no DTO — AutoMapper não exige ignorar campos da source que não têm destino correspondente. Documentado aqui para clareza de dependência.

---

### `CreateMap<Imagem, ImagemDto>`

| Campo entidade | Tipo | Campo DTO | Tipo | Ação |
|---|---|---|---|---|
| `Id` | `Guid` | `Id` | `Guid` | MapFrom |
| `NomeArquivo` | `string` | `NomeArquivo` | `string` | MapFrom |
| `ChaveS3` | `string` | `ChaveS3` | `string` | MapFrom |
| `TipoConteudo` | `string?` | `TipoConteudo` | `string` | MapFrom |
| `TamanhoBytes` | `long` | `TamanhoBytes` | `long` | MapFrom |
| `DataCriacao` | `DateTime` | `DataCriacao` | `DateTime` | MapFrom |
| `IdProduto` | `Guid` | — | — | Ignore |

`IdProduto` omitido do DTO intencionalmente — cliente já conhece o produto pelo contexto da request.

```csharp
CreateMap<Imagem, ImagemDto>()
    .ForMember(dest => dest.Id,           opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.NomeArquivo,  opt => opt.MapFrom(src => src.NomeArquivo))
    .ForMember(dest => dest.ChaveS3,      opt => opt.MapFrom(src => src.ChaveS3))
    .ForMember(dest => dest.TipoConteudo, opt => opt.MapFrom(src => src.TipoConteudo))
    .ForMember(dest => dest.TamanhoBytes, opt => opt.MapFrom(src => src.TamanhoBytes))
    .ForMember(dest => dest.DataCriacao,  opt => opt.MapFrom(src => src.DataCriacao));
```

---

### `CreateMap<UsuarioAdmin, UsuarioAdminDto>`

| Campo entidade | Tipo | Campo DTO | Tipo | Ação |
|---|---|---|---|---|
| `Id` | `Guid` | `Id` | `Guid` | MapFrom |
| `Email` | `string` | `Email` | `string` | MapFrom |
| `Role` | `string` | `Role` | `string` | MapFrom |
| `SenhaHash` | `string` | — | — | Ignore (segurança) |
| `Ativo` | `bool` | — | — | Ignore |
| `DeletadoEm` | `DateTime?` | — | — | Ignore |

`SenhaHash` nunca exposto — omissão deliberada, não acidental.

```csharp
CreateMap<UsuarioAdmin, UsuarioAdminDto>()
    .ForMember(dest => dest.Id,    opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
    .ForMember(dest => dest.Role,  opt => opt.MapFrom(src => src.Role));
```

---

## MappingProfile.cs resultante

```csharp
using AutoMapper;
using Neostore.Application.DTOs;
using Neostore.Domain.Entities;

namespace Neostore.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Categoria, CategoriaDto>()
            .ForMember(dest => dest.Id,             opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nome,            opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.Slug,            opt => opt.MapFrom(src => src.Slug))
            .ForMember(dest => dest.IdCategoriaPai,  opt => opt.MapFrom(src => src.IdCategoriaPai));

        CreateMap<Produto, ProdutoDto>()
            .ForMember(dest => dest.Id,          opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nome,         opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.SKU,          opt => opt.MapFrom(src => src.SKU))
            .ForMember(dest => dest.Preço,        opt => opt.MapFrom(src => src.Preço))
            .ForMember(dest => dest.IdCategoria,  opt => opt.MapFrom(src => src.IdCategoria))
            .ForMember(dest => dest.Descrição,    opt => opt.MapFrom(src => src.Descrição))
            .ForMember(dest => dest.Imagens,      opt => opt.MapFrom(src => src.Imagens))
            .ForMember(dest => dest.Estoque,      opt => opt.MapFrom(src => src.Estoque));

        CreateMap<Imagem, ImagemDto>()
            .ForMember(dest => dest.Id,            opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NomeArquivo,   opt => opt.MapFrom(src => src.NomeArquivo))
            .ForMember(dest => dest.ChaveS3,       opt => opt.MapFrom(src => src.ChaveS3))
            .ForMember(dest => dest.TipoConteudo,  opt => opt.MapFrom(src => src.TipoConteudo))
            .ForMember(dest => dest.TamanhoBytes,  opt => opt.MapFrom(src => src.TamanhoBytes))
            .ForMember(dest => dest.DataCriacao,   opt => opt.MapFrom(src => src.DataCriacao));

        CreateMap<UsuarioAdmin, UsuarioAdminDto>()
            .ForMember(dest => dest.Id,    opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Role,  opt => opt.MapFrom(src => src.Role));
    }
}
```

## Dependências

```
MappingProfile
├── Neostore.Domain.Entities.Categoria   → CategoriaDto
├── Neostore.Domain.Entities.Produto     → ProdutoDto
│     └── Neostore.Domain.Entities.Imagem → ImagemDto  (mapeamento encadeado)
└── Neostore.Domain.Entities.UsuarioAdmin → UsuarioAdminDto
```

Campos que exigem atenção ao renomear entidade/DTO:

| Se renomear em entidade | Quebra em DTO |
|---|---|
| `Produto.Preço` | `ProdutoDto.Preço` |
| `Produto.Descrição` | `ProdutoDto.Descrição` |
| `Imagem.TipoConteudo` | `ImagemDto.TipoConteudo` (entidade é `string?`, DTO é `string`) |

## Verificação

```bash
cd src
dotnet test --filter ClassName=MappingProfileTests
```

Testes de mapping devem validar `cfg.AssertConfigurationIsValid()` para cada perfil.
