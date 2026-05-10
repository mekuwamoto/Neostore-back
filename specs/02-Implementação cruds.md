# Plano de Implementação: CRUD de Categorias e Produtos

Este documento especifica o roteiro técnico para a implementação das operações de CRUD, seguindo os padrões de CQRS e Clean Architecture definidos nas especificações anteriores.

## 1. Fluxo de Implementação (Padrão)
Para cada entidade, a implementação deve seguir esta ordem lógica para garantir a integridade da arquitetura:
1.  **Domínio:** Definição da entidade e métodos de negócio.
2.  **Persistência:** Mapeamento (Fluent API) para MariaDB e implementação do Repositório.
3.  **Application (Commands):** Definição das intenções de escrita e regras de validação.
4.  **Application (Queries):** Definição das intenções de leitura e DTOs de resposta.
5.  **Application (Handlers):** Implementação da lógica de orquestração utilizando Mediatr
6.  **API:** Exposição dos endpoints via Controllers.

---

## 2. CRUD de Categorias

### 2.1 Operações de Escrita (Commands)
| Operação                | Dados de Entrada                   | Regras de Negócio e Validações                                                  |
| :---------------------- | :--------------------------------- | :------------------------------------------------------------------------------ |
| **Criar Categoria**     | Nome, IdCategoriaPai (Opcional)    | Gerar Slug automaticamente. Nome deve ser único. Validar existência do pai.     |
| **Atualizar Categoria** | Id, Nome,IdCategoriaPai (Opcional) | Atualizar Slug se o nome mudar. Impedir auto-referência (id == idCategoriaPai). |
| **Excluir Categoria**   | Id                                 | Impedir exclusão se houver produtos vinculados ou subcategorias ativas.         |

### 2.2 Operações de Leitura (Queries)
- **Obter por Id:** Retorna detalhes da categoria e nome da categoria pai.
- **Listar Árvore:** Retorna todas as categorias organizadas de forma hierárquica.
- **Listar Raiz:** Retorna apenas categorias que não possuem `idCategoriaPai`.

---

## 3. CRUD de Produtos

### 3.1 Operações de Escrita (Commands)
| Operação              | Dados de Entrada                                      | Regras de Negócio e Validações                                                         |
| :-------------------- | :---------------------------------------------------- | :------------------------------------------------------------------------------------- |
| **Criar Produto**     | Nome, SKU, Preço, IdCategoria, Descrição, Imagens     | SKU deve ser único no sistema. idCategoria deve existir. Preço > 0.                    |
| **Atualizar Produto** | Id, Nome, SKU, Preço, IdCategoria, Descrição, Imagens | Validar unicidade do SKU (exceto para o próprio registro).                             |
| **Ajustar Estoque**   | Id, Quantidade (Delta)                                | Aumentar ou diminuir o estoque atual. O saldo final nunca pode ser < 0.                |
| **Excluir Produto**   | Id                                                    | Aplicar Soft-Delete (marcar como inativo) para preservar histórico de pedidos futuros. |

### 3.2 Operações de Leitura (Queries)
- **Obter Detalhes:** Retorna todos os campos do produto + objeto Categoria simplificado.
- **Listagem Paginada:** Suporte a filtros por `idCategoria`, `Nome` (busca parcial) e `SKU`.
- **Verificar Disponibilidade:** Consulta rápida apenas do saldo de estoque por Id.

---

## 4. Definição de Endpoints (API Admin)

### Categorias
- `POST /api/admin/categorias` -> CreateCategory
- `GET /api/admin/categorias` -> GetCategoryTree
- `GET /api/admin/categorias/{id}` -> GetCategoryById
- `PUT /api/admin/categorias/{id}` -> UpdateCategory
- `DELETE /api/admin/categorias/{id}` -> DeleteCategory

### Produtos
- `POST /api/admin/produtos` -> CreateProduct
- `GET /api/admin/produtos` -> GetPagedProducts
- `GET /api/admin/produtos/{id}` -> GetProductById
- `PUT /api/admin/produtos/{id}` -> UpdateProduct
- `PATCH /api/admin/produtos/{id}/estoque` -> UpdateStock
- `DELETE /api/admin/produtos/{id}` -> DeleteProduct

---

## 5. Requisitos Transversais
- **Validação:** Uso obrigatório de `FluentValidation` em todos os Commands antes de chegar ao Handler.
- **Logs:** Registrar no log de auditoria: Usuário Executor, Operação, Data/Hora e Id do Registro afetado.
- **Tratamento de Erros:** Exceções de negócio (ex: SKU Duplicado) devem retornar `400 Bad Request` com mensagens claras. Erros de sistema devem retornar `500`.
