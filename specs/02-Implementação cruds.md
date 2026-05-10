# Plano de Implementação: CRUD de Categorias e Produtos

Este documento especifica o roteiro técnico para a implementação das operações de CRUD, seguindo os padrões de CQRS e Clean Architecture definidos nas especificações anteriores.

## 1. Fluxo de Implementação (Padrão)
Para cada entidade, a implementação deve seguir esta ordem lógica para garantir a integridade da arquitetura:
1.  **Domínio:** Definição da entidade e métodos de negócio.
2.  **Persistência:** Mapeamento (Fluent API Configuration em arquivo próprio por entidade) para MariaDB e implementação do Repositório.
3.  **Application (Commands):** Definição das intenções de escrita e regras de validação.
4.  **Application (Queries):** Definição das intenções de leitura e DTOs de resposta.
5.  **Application (Handlers):** Implementação da lógica de orquestração utilizando Mediatr
6.  **API:** Exposição dos endpoints via Controllers.

---

## 2. CRUD de Categorias

### 2.1 Operações de Escrita (Commands)
| Operação                | Dados de Entrada                   | Regras de Negócio e Validações                                                  | Status |
| :---------------------- | :--------------------------------- | :------------------------------------------------------------------------------ | :----- |
| **Criar Categoria**     | Nome, IdCategoriaPai (Opcional)    | Gerar Slug automaticamente. Nome deve ser único. Validar existência do pai.     | ✅ Implementado |
| **Atualizar Categoria** | Id, Nome, IdCategoriaPai (Opcional) | Atualizar Slug se o nome mudar. Impedir auto-referência (id == idCategoriaPai). | ✅ Implementado |
| **Excluir Categoria**   | Id                                 | Impedir exclusão se houver produtos vinculados ou subcategorias ativas.         | ✅ Implementado |

### 2.2 Operações de Leitura (Queries)
| Operação              | Descrição                                                           | Status |
| :-------------------- | :------------------------------------------------------------------ | :----- |
| **Obter por Id**      | Retorna detalhes da categoria e nome da categoria pai.              | ✅ Implementado |
| **Listar Árvore**     | Retorna todas as categorias organizadas de forma hierárquica.       | ✅ Implementado |
| **Listar Raiz**       | Retorna apenas categorias que não possuem `idCategoriaPai`.         | ⏳ Parcial — `ObterRaizAsync` implementado no repositório, sem Query/Handler/endpoint expostos |

---

## 3. CRUD de Produtos

### 3.1 Operações de Escrita (Commands)
| Operação              | Dados de Entrada                                      | Regras de Negócio e Validações                                                         | Status |
| :-------------------- | :---------------------------------------------------- | :------------------------------------------------------------------------------------- | :----- |
| **Criar Produto**     | Nome, SKU, Preço, IdCategoria, Descrição, Imagens     | SKU deve ser único no sistema. idCategoria deve existir. Preço > 0.                    | ✅ Implementado |
| **Atualizar Produto** | Id, Nome, SKU, Preço, IdCategoria, Descrição, Imagens | Validar unicidade do SKU (exceto para o próprio registro).                             | ✅ Implementado |
| **Ajustar Estoque**   | Id, Quantidade (Delta)                                | Aumentar ou diminuir o estoque atual. O saldo final nunca pode ser < 0.                | ✅ Implementado |
| **Excluir Produto**   | Id                                                    | Aplicar Soft-Delete (marcar como inativo) para preservar histórico de pedidos futuros. | ⚠️ Pendente — atualmente implementado como hard-delete; entidade não possui campo `Ativo`/`DeletadoEm` |

### 3.2 Operações de Leitura (Queries)
| Operação                     | Descrição                                                             | Status |
| :--------------------------- | :-------------------------------------------------------------------- | :----- |
| **Obter Detalhes**           | Retorna todos os campos do produto + objeto Categoria simplificado.   | ✅ Implementado |
| **Listagem Paginada**        | Suporte a filtros por `idCategoria`, `Nome` (busca parcial) e `SKU`. | ✅ Implementado |
| **Verificar Disponibilidade** | Consulta rápida apenas do saldo de estoque por Id.                   | ❌ Não implementado |

---

## 4. Definição de Endpoints (API Admin)

### Categorias
| Método | Endpoint | Handler | Status |
| :----- | :------- | :------ | :----- |
| `POST` | `/api/admin/categorias` | `CriarCategoriaCommandHandler` | ✅ |
| `GET` | `/api/admin/categorias` | `ObterTodasCategoriasQueryHandler` | ✅ |
| `GET` | `/api/admin/categorias/{id}` | `ObterCategoriaPorIdQueryHandler` | ✅ |
| `PUT` | `/api/admin/categorias/{id}` | `AtualizarCategoriaCommandHandler` | ✅ |
| `DELETE` | `/api/admin/categorias/{id}` | `DeletarCategoriaCommandHandler` | ✅ |
| `GET` | `/api/admin/categorias/raiz` | — | ⏳ Pendente |

### Produtos
| Método | Endpoint | Handler | Status |
| :----- | :------- | :------ | :----- |
| `POST` | `/api/admin/produtos` | `CriarProdutoCommandHandler` | ✅ |
| `GET` | `/api/admin/produtos` | `ObterProdutosPaginadoQueryHandler` | ✅ |
| `GET` | `/api/admin/produtos/{id}` | `ObterProdutoPorIdQueryHandler` | ✅ |
| `PUT` | `/api/admin/produtos/{id}` | `AtualizarProdutoCommandHandler` | ✅ |
| `PATCH` | `/api/admin/produtos/{id}/estoque` | `AjustarEstoqueCommandHandler` | ✅ |
| `DELETE` | `/api/admin/produtos/{id}` | `DeletarProdutoCommandHandler` | ⚠️ Hard-delete — pendente soft-delete |
| `GET` | `/api/admin/produtos/{id}/disponibilidade` | — | ❌ Não implementado |

---

## 5. Requisitos Transversais
| Requisito | Status |
| :-------- | :----- |
| **Validação** — FluentValidation em todos os Commands antes do Handler | ✅ Implementado — `CriarProdutoCommandValidator`, `AtualizarProdutoCommandValidator`, `CriarCategoriaCommandValidator`, `AtualizarCategoriaCommandValidator` |
| **Logs** — Registrar: Usuário Executor, Operação, Data/Hora, Id do Registro | ❌ Não implementado |
| **Tratamento de Erros** — `400 Bad Request` para exceções de negócio; `500` para erros de sistema | ✅ Implementado — `InvalidOperationException` mapeada para `BadRequest` nos controllers |

---

## 6. Pendências

- [ ] **Soft-Delete de Produto:** Adicionar campo `Ativo` ou `DeletadoEm` na entidade `Produto`, criar migration, ajustar repositório para filtrar inativo nas queries de leitura e alterar `DeletarProdutoCommandHandler` para marcar inativo ao invés de deletar.
- [ ] **Listar Raiz (Categorias):** Criar `ObterRaizQuery`, `ObterRaizQueryHandler` e expor endpoint `GET /api/admin/categorias/raiz` usando `ObterRaizAsync` já existente no `ICategoriaRepository`.
- [ ] **Verificar Disponibilidade (Produto):** Criar `ObterEstoqueQuery`, handler e endpoint `GET /api/admin/produtos/{id}/disponibilidade` retornando apenas `{ id, estoque }`.
- [ ] **Logs de Auditoria:** Definir estratégia (middleware, interceptor EF Core, ou decorator MediatR) e implementar registro de operações.
- [ ] **Migrations:** Gerar migration inicial com `dotnet ef migrations add InitialCreate`.
- [ ] **Docker Compose:** Configurar ambiente local com MariaDB.
