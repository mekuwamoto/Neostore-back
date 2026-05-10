# ADR-02: Implementação dos CRUDs de Categorias e Produtos

## Status
Partially Implemented

## Date
2025-01-01

## Context
Com entidades, CQRS e repositórios definidos (ADR-01), é necessário estabelecer a ordem de implementação e rastrear o estado de cada operação para garantir consistência entre camadas.

## Decision
Cada entidade segue esta ordem de implementação:

1. **Domínio** — entidade + métodos de negócio
2. **Persistência** — Fluent API Configuration + repositório
3. **Application (Commands)** — intenções de escrita + validadores
4. **Application (Queries)** — intenções de leitura + DTOs
5. **Application (Handlers)** — orquestração via MediatR
6. **API** — controllers expondo endpoints

### CRUD de Categorias

#### Operações de Escrita

| Operação | Dados de Entrada | Regras de Negócio | Status |
| -------- | ---------------- | ----------------- | ------ |
| Criar | Nome, IdCategoriaPai? | Gerar Slug. Nome único. Validar existência do pai. | ✅ Implementado |
| Atualizar | Id, Nome, IdCategoriaPai? | Atualizar Slug se nome mudar. Impedir auto-referência. | ✅ Implementado |
| Excluir | Id | Bloquear se houver produtos ou subcategorias vinculadas. | ✅ Implementado |

#### Operações de Leitura

| Operação | Descrição | Status |
| -------- | --------- | ------ |
| Obter por Id | Retorna categoria + nome da categoria pai | ✅ Implementado |
| Listar Árvore | Todas as categorias hierarquicamente | ✅ Implementado |
| Listar Raiz | Apenas categorias sem `IdCategoriaPai` | ⏳ Repositório implementado (`ObterRaizAsync`), sem Query/Handler/endpoint |

#### Endpoints

| Método | Endpoint | Handler | Status |
| ------ | -------- | ------- | ------ |
| `POST` | `/api/admin/categorias` | `CriarCategoriaCommandHandler` | ✅ |
| `GET` | `/api/admin/categorias` | `ObterTodasCategoriasQueryHandler` | ✅ |
| `GET` | `/api/admin/categorias/{id}` | `ObterCategoriaPorIdQueryHandler` | ✅ |
| `PUT` | `/api/admin/categorias/{id}` | `AtualizarCategoriaCommandHandler` | ✅ |
| `DELETE` | `/api/admin/categorias/{id}` | `DeletarCategoriaCommandHandler` | ✅ |
| `GET` | `/api/admin/categorias/raiz` | — | ⏳ Pendente |

---

### CRUD de Produtos

#### Operações de Escrita

| Operação | Dados de Entrada | Regras de Negócio | Status |
| -------- | ---------------- | ----------------- | ------ |
| Criar | Nome, SKU, Preco, IdCategoria, Descricao, Imagens | SKU único. IdCategoria deve existir. Preco > 0. | ✅ Implementado |
| Atualizar | Id + todos os campos | SKU único exceto para o próprio registro. | ✅ Implementado |
| Ajustar Estoque | Id, Quantidade (delta) | Saldo final nunca < 0. | ✅ Implementado |
| Excluir | Id | Soft-delete para preservar histórico. | ⚠️ Hard-delete atual — soft-delete pendente (ver ADR-03) |

#### Operações de Leitura

| Operação | Descrição | Status |
| -------- | --------- | ------ |
| Obter Detalhes | Produto completo + Categoria simplificada | ✅ Implementado |
| Listagem Paginada | Filtros por `IdCategoria`, `Nome` (parcial), `SKU` | ✅ Implementado |
| Verificar Disponibilidade | Saldo de estoque por Id | ❌ Não implementado |

#### Endpoints

| Método | Endpoint | Handler | Status |
| ------ | -------- | ------- | ------ |
| `POST` | `/api/admin/produtos` | `CriarProdutoCommandHandler` | ✅ |
| `GET` | `/api/admin/produtos` | `ObterProdutosPaginadoQueryHandler` | ✅ |
| `GET` | `/api/admin/produtos/{id}` | `ObterProdutoPorIdQueryHandler` | ✅ |
| `PUT` | `/api/admin/produtos/{id}` | `AtualizarProdutoCommandHandler` | ✅ |
| `PATCH` | `/api/admin/produtos/{id}/estoque` | `AjustarEstoqueCommandHandler` | ✅ |
| `DELETE` | `/api/admin/produtos/{id}` | `DeletarProdutoCommandHandler` | ⚠️ Hard-delete |
| `GET` | `/api/admin/produtos/{id}/disponibilidade` | — | ❌ Não implementado |

---

### Requisitos Transversais

| Requisito | Status |
| --------- | ------ |
| Validação via FluentValidation em todos os Commands | ✅ Implementado |
| Tratamento de erros: `InvalidOperationException` → 400, exceções → 500 | ✅ Implementado nos controllers |
| Logs de auditoria (usuário, operação, timestamp, id do registro) | ❌ Não implementado — ver ADR-04 |

## Consequences
### Positivo
- CRUDs principais de categorias e produtos operacionais.
- Paginação com filtros elimina necessidade de listagem completa.
- Validação centralizada no pipeline MediatR.

### Trade-offs
- Soft-delete ausente: exclusão de produto é irreversível no estado atual.
- Endpoint de disponibilidade de estoque ausente.
- Logs de auditoria ausentes.

## Pendências

- [ ] Soft-delete em Produto e UsuarioAdmin (ADR-03)
- [ ] `GET /api/admin/categorias/raiz` — criar `ObterRaizQuery` + handler usando `ObterRaizAsync` existente
- [ ] `GET /api/admin/produtos/{id}/disponibilidade` — retorna `{ id, estoque }`
- [ ] Logs de auditoria via MediatR Pipeline Behavior (ADR-04)
- [ ] Migration inicial: `dotnet ef migrations add InitialCreate`
- [ ] Docker Compose com MariaDB
