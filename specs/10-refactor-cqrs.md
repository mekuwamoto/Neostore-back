# ADR-10: Refatoração CQRS — Colocar Handler no mesmo arquivo do Command/Query

**Status:** Implemented  
**Data:** 2026-05-14

## Contexto

Atualmente `Neostore.Application` separa command/query records em `Commands/` e `Queries/`, e seus handlers em `Handlers/`. Cada feature requer navegar entre dois arquivos para entender o fluxo completo. A pasta `Handlers/` é redundante — MediatR não exige essa separação.

## Decisão

Eliminar a pasta `Handlers/`. Mover cada handler para o mesmo arquivo do seu command ou query correspondente. Command/query record e handler ficam no mesmo arquivo, mesmo namespace.

## Estrutura Alvo

```
Commands/
  Categoria/
    CriarCategoriaCommand.cs        ← record + handler
    AtualizarCategoriaCommand.cs    ← record + handler
    DeletarCategoriaCommand.cs      ← record + handler
  Produto/
    CriarProdutoCommand.cs          ← record + handler
    AtualizarProdutoCommand.cs      ← record + handler
    DeletarProdutoCommand.cs        ← record + handler
    AjustarEstoqueCommand.cs        ← record + handler
  UsuarioAdmin/
    CriarUsuarioAdminCommand.cs     ← record + handler
    AtualizarSenhaCommand.cs        ← record + handler
    AtualizarUsuarioAdminCommand.cs ← record + handler
    DeletarUsuarioAdminCommand.cs   ← record + handler
Queries/
  Categoria/
    ObterTodasCategoriasQuery.cs    ← record + handler
    ObterCategoriaPorIdQuery.cs     ← record + handler
  Produto/
    ObterTodosProdutosQuery.cs      ← record + handler
    ObterProdutoPorIdQuery.cs       ← record + handler
    ObterProdutosPaginadoQuery.cs   ← record + handler (ProdutosPaginadoDto permanece aqui)
  UsuarioAdmin/
    ObterTodosUsuariosAdminQuery.cs ← record + handler
    ObterUsuarioAdminPorIdQuery.cs  ← record + handler
```

## Padrão de arquivo resultante

```csharp
// Commands/Categoria/CriarCategoriaCommand.cs
namespace Neostore.Application.Commands.Categoria;

public record CriarCategoriaCommand(
    string Nome,
    Guid? IdCategoriaPai
) : IRequest<CategoriaDto>;

public class CriarCategoriaCommandHandler : IRequestHandler<CriarCategoriaCommand, CategoriaDto>
{
    // ... handler body (inalterado)
}
```

## Mudanças de Namespace

| De | Para |
|----|------|
| `Neostore.Application.Handlers.Categoria` | `Neostore.Application.Commands.Categoria` (command handlers) |
| `Neostore.Application.Handlers.Categoria` | `Neostore.Application.Queries.Categoria` (query handlers) |
| `Neostore.Application.Handlers.Produto` | `Neostore.Application.Commands.Produto` (command handlers) |
| `Neostore.Application.Handlers.Produto` | `Neostore.Application.Queries.Produto` (query handlers) |
| `Neostore.Application.Handlers.UsuarioAdmin` | `Neostore.Application.Commands.UsuarioAdmin` (command handlers) |
| `Neostore.Application.Handlers.UsuarioAdmin` | `Neostore.Application.Queries.UsuarioAdmin` (query handlers) |

## Plano de execução

### Passo 1 — Commands/Categoria (3 arquivos)

Para cada command em `Commands/Categoria/`:
1. Abrir `Commands/Categoria/<Command>.cs`
2. Copiar corpo do handler de `Handlers/Categoria/<Command>Handler.cs`
3. Remover `using Neostore.Application.Commands.Categoria;` do handler (namespace já é o mesmo)
4. Colar handler abaixo do record, mesmo namespace
5. Deletar `Handlers/Categoria/<Command>Handler.cs`

Arquivos afetados:
- `CriarCategoriaCommand.cs` + `CriarCategoriaCommandHandler.cs`
- `AtualizarCategoriaCommand.cs` + `AtualizarCategoriaCommandHandler.cs`
- `DeletarCategoriaCommand.cs` + `DeletarCategoriaCommandHandler.cs`

### Passo 2 — Queries/Categoria (2 arquivos)

Para cada query em `Queries/Categoria/`:
1. Mesmo processo do Passo 1
2. Remover `using Neostore.Application.Queries.Categoria;` do handler copiado
3. Deletar `Handlers/Categoria/<Query>Handler.cs`

Arquivos afetados:
- `ObterTodasCategoriasQuery.cs` + `ObterTodasCategoriasQueryHandler.cs`
- `ObterCategoriaPorIdQuery.cs` + `ObterCategoriaPorIdQueryHandler.cs`

Após Passos 1 e 2: `Handlers/Categoria/` estará vazio — deletar pasta.

### Passo 3 — Commands/Produto (4 arquivos)

Arquivos afetados:
- `CriarProdutoCommand.cs` + `CriarProdutoCommandHandler.cs`
- `AtualizarProdutoCommand.cs` + `AtualizarProdutoCommandHandler.cs`
- `DeletarProdutoCommand.cs` + `DeletarProdutoCommandHandler.cs`
- `AjustarEstoqueCommand.cs` + `AjustarEstoqueCommandHandler.cs`

### Passo 4 — Queries/Produto (3 arquivos)

Arquivos afetados:
- `ObterTodosProdutosQuery.cs` + `ObterTodosProdutosQueryHandler.cs`
- `ObterProdutoPorIdQuery.cs` + `ObterProdutoPorIdQueryHandler.cs`
- `ObterProdutosPaginadoQuery.cs` + `ObterProdutosPaginadoQueryHandler.cs`

Após Passos 3 e 4: `Handlers/Produto/` estará vazio — deletar pasta.

### Passo 5 — Commands/UsuarioAdmin (4 arquivos)

Arquivos afetados:
- `CriarUsuarioAdminCommand.cs` + `CriarUsuarioAdminCommandHandler.cs`
- `AtualizarSenhaCommand.cs` + `AtualizarSenhaCommandHandler.cs`
- `AtualizarUsuarioAdminCommand.cs` + `AtualizarUsuarioAdminCommandHandler.cs`
- `DeletarUsuarioAdminCommand.cs` + `DeletarUsuarioAdminCommandHandler.cs`

### Passo 6 — Queries/UsuarioAdmin (2 arquivos)

Arquivos afetados:
- `ObterTodosUsuariosAdminQuery.cs` + `ObterTodosUsuariosAdminQueryHandler.cs`
- `ObterUsuarioAdminPorIdQuery.cs` + `ObterUsuarioAdminPorIdQueryHandler.cs`

Após Passos 5 e 6: `Handlers/UsuarioAdmin/` estará vazio — deletar pasta. `Handlers/` estará vazio — deletar pasta raiz.

### Passo 7 — Atualizar usings nos testes

`Neostore.Tests` referencia handlers por namespace. Atualizar `using` em:

| Arquivo de teste | Using antigo | Using novo |
|-----------------|--------------|------------|
| `Handlers/Categoria/CriarCategoriaCommandHandlerTests.cs` | `Neostore.Application.Handlers.Categoria` | `Neostore.Application.Commands.Categoria` |
| `Handlers/Categoria/AtualizarCategoriaCommandHandlerTests.cs` | `Neostore.Application.Handlers.Categoria` | `Neostore.Application.Commands.Categoria` |
| `Handlers/Categoria/DeletarCategoriaCommandHandlerTests.cs` | `Neostore.Application.Handlers.Categoria` | `Neostore.Application.Commands.Categoria` |
| `Handlers/Categoria/ObterTodasCategoriasQueryHandlerTests.cs` | `Neostore.Application.Handlers.Categoria` | `Neostore.Application.Queries.Categoria` |
| `Handlers/Categoria/ObterCategoriaPorIdQueryHandlerTests.cs` | `Neostore.Application.Handlers.Categoria` | `Neostore.Application.Queries.Categoria` |
| `Handlers/Produto/CriarProdutoCommandHandlerTests.cs` | `Neostore.Application.Handlers.Produto` | `Neostore.Application.Commands.Produto` |
| `Handlers/Produto/AtualizarProdutoCommandHandlerTests.cs` | `Neostore.Application.Handlers.Produto` | `Neostore.Application.Commands.Produto` |
| `Handlers/Produto/DeletarProdutoCommandHandlerTests.cs` | `Neostore.Application.Handlers.Produto` | `Neostore.Application.Commands.Produto` |
| `Handlers/Produto/AjustarEstoqueCommandHandlerTests.cs` | `Neostore.Application.Handlers.Produto` | `Neostore.Application.Commands.Produto` |
| `Handlers/Produto/ObterTodosProdutosQueryHandlerTests.cs` | `Neostore.Application.Handlers.Produto` | `Neostore.Application.Queries.Produto` |
| `Handlers/Produto/ObterProdutoPorIdQueryHandlerTests.cs` | `Neostore.Application.Handlers.Produto` | `Neostore.Application.Queries.Produto` |
| `Handlers/Produto/ObterProdutosPaginadoQueryHandlerTests.cs` | `Neostore.Application.Handlers.Produto` | `Neostore.Application.Queries.Produto` |

### Passo 8 — Atualizar CLAUDE.md

Atualizar `src/Neostore.Application/CLAUDE.md`:
- Remover `Handlers/` da seção Structure
- Atualizar "Adding a New Command/Query" — passo 2 agora aponta para o próprio arquivo do command/query

Atualizar `CLAUDE.md` raiz do submodulo (`Neostore-back/CLAUDE.md`):
- Seção "Adding a feature" passo 2: remover menção a `Handlers/`

## O que NÃO muda

- Corpo dos handlers — nenhuma lógica alterada
- `DependencyInjection.cs` — MediatR escaneia assembly por convenção, não por namespace
- Validators — permanecem em `Validators/`
- Behaviors — permanecem em `Behaviors/`
- DTOs — permanecem em `DTOs/`
- `ProdutosPaginadoDto` — permanece em `Queries/Produto/ObterProdutosPaginadoQuery.cs`

## Verificação

```bash
cd src
dotnet build
dotnet test
```

Build e todos os testes devem passar sem modificação de lógica.
