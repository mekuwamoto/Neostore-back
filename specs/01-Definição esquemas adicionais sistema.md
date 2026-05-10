# ADR-01: Arquitetura CQRS, MediatR e Entidades de Domínio

## Status
Implemented

## Date
2025-01-01

## Context
API admin precisa de uma camada de aplicação desacoplada da infraestrutura para facilitar testes unitários e separação de responsabilidades. Handlers de leitura e escrita têm requisitos distintos (queries são simples selects; commands têm validação e efeitos colaterais).

## Decision
Adotar **CQRS via MediatR** dentro de uma **Onion Architecture**:

- **Commands** (escrita): imutáveis (`record`), validados por FluentValidation antes do handler.
- **Queries** (leitura): retornam DTOs otimizados, sem efeitos colaterais.
- **Handlers**: orquestram a lógica, chamam repositórios, retornam DTOs.

### Estrutura de Camadas

| Camada | Responsabilidade |
| ------ | ---------------- |
| `Neostore.Domain` | Entidades + regras de negócio puras |
| `Neostore.Application` | Commands, Queries, Handlers, Validators, DTOs |
| `Neostore.Persistence` | EF Core DbContext + Repositórios |
| `Neostore.Infrastructure` | Serviços externos (S3, email, etc.) |
| `Neostore.Api` | Controllers + DI composition root |

### Entidades de Domínio

#### Produto
| Campo | Tipo | Regras |
| ----- | ---- | ------ |
| `Id` | Guid | PK, `ValueGeneratedNever` |
| `Nome` | string | Obrigatório |
| `SKU` | string | Único no sistema |
| `Preco` | decimal | > 0 |
| `IdCategoria` | Guid | FK para Categoria |
| `Descricao` | string | — |
| `Imagens` | `List<Imagem>` | Gerenciadas via S3 |
| `Estoque` | int | ≥ 0 |

**Métodos:** `AjustarEstoque(delta)`, `AdicionarImagem(imagem)`, `RemoverImagem(idImagem)`

#### Categoria
| Campo | Tipo | Regras |
| ----- | ---- | ------ |
| `Id` | Guid | PK |
| `Nome` | string | Único |
| `Slug` | string | Gerado a partir do Nome |
| `IdCategoriaPai` | Guid? | Auto-relacionamento |

**Métodos:** `GerarSlug(nome)`, `ValidarHierarquia(categoriaPai)` (impede circularidade)

#### Imagem
| Campo | Tipo | Regras |
| ----- | ---- | ------ |
| `Id` | Guid | PK |
| `NomeArquivo` | string | — |
| `ChaveS3` | string | Obrigatório |
| `TipoConteudo` | string | MIME type |
| `TamanhoBytes` | long | — |
| `IdProduto` | Guid | FK para Produto |
| `DataCriacao` | DateTime | UTC |

**Métodos:** `ObterUrlS3(bucketUrl)`

#### UsuarioAdmin
| Campo | Tipo | Regras |
| ----- | ---- | ------ |
| `Id` | Guid | PK |
| `Email` | string | Único |
| `SenhaHash` | string | Nunca texto plano |
| `Role` | string | Ex: Admin, Gerente |

**Métodos:** `AtualizarSenha(novoHash)`

### Commands e Queries Implementados

**Produto:**
- Commands: `CriarProdutoCommand`, `AtualizarProdutoCommand`, `DeletarProdutoCommand`, `AjustarEstoqueCommand`
- Queries: `ObterProdutoPorIdQuery`, `ObterTodosProdutosQuery`, `ObterProdutosPaginadoQuery`

**Categoria:**
- Commands: `CriarCategoriaCommand`, `AtualizarCategoriaCommand`, `DeletarCategoriaCommand`
- Queries: `ObterCategoriaPorIdQuery`, `ObterTodasCategoriasQuery`

**UsuarioAdmin:**
- Commands: `CriarUsuarioAdminCommand`, `AtualizarSenhaCommand`, `DeletarUsuarioAdminCommand` _(handlers pendentes)_
- Queries: `ObterUsuarioAdminPorIdQuery`, `ObterTodosUsuariosAdminQuery` _(handlers pendentes)_

### Validators (FluentValidation)
- `CriarProdutoCommandValidator`, `AtualizarProdutoCommandValidator`
- `CriarCategoriaCommandValidator`, `AtualizarCategoriaCommandValidator`
- `CriarUsuarioAdminCommandValidator`, `AtualizarSenhaCommandValidator`

### Registro DI
```csharp
// Neostore.Application/DependencyInjection.cs
services.AddMediatR(typeof(DependencyInjection));
services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
```

### Convenção de FK
Prefixo `Id` + nome da entidade (CamelCase): `IdCategoria`, `IdProduto`, `IdCategoriaPai`.

## Consequences
### Positivo
- Handlers testáveis em isolamento (mockar apenas repositório).
- Validação automática via pipeline antes de qualquer handler.
- Queries e commands evoluem independentemente.

### Trade-offs
- Boilerplate maior: cada operação requer record + handler + validator.
- Handlers de UsuarioAdmin ainda sem implementação.

## Testes
53 testes unitários (xUnit + AwesomeAssertions), 100% sucesso:
- `ProdutoTests`: 12 testes
- `ImagemTests`: 11 testes
- `CategoriaTests`: 13 testes
- `UsuarioAdminTests`: 17 testes
