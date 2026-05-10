# ADR-00: Escopo Inicial — API de Catálogo Admin

## Status
Accepted

## Date
2025-01-01

## Context
Neostore precisa de uma plataforma e-commerce. Ponto de entrada mais crítico: cadastro e gestão de produtos. Funcionalidades de carrinho, checkout e pagamento dependem de um catálogo estável.

## Decision
Construir primeiro uma **API admin REST** (.NET 10, ASP.NET Core) focada em gestão de catálogo. Escopo inicial:

- CRUD de Produtos (`/api/admin/produtos`)
- CRUD de Categorias (`/api/admin/categorias`)
- Gestão de Usuários Admin (`/api/admin/usuarios`)

### Endpoints Essenciais (Produtos)

| Método | Caminho | Descrição |
| ------ | ------- | --------- |
| `POST` | `/api/admin/produtos` | Criar produto |
| `GET` | `/api/admin/produtos/{id}` | Detalhes por Id |
| `PUT` | `/api/admin/produtos/{id}` | Atualizar produto |
| `PATCH` | `/api/admin/produtos/{id}/estoque` | Ajustar estoque |
| `DELETE` | `/api/admin/produtos/{id}` | Remover produto |

### Stack Definida
- **Runtime:** .NET 10 / ASP.NET Core
- **ORM:** EF Core + Pomelo (MariaDB)
- **Arquitetura:** Clean Architecture (Onion)
- **CQRS:** MediatR
- **Validação:** FluentValidation
- **Testes:** xUnit + Moq + AwesomeAssertions
- **Infra local:** Docker Compose (MariaDB)

## Consequences
### Positivo
- Catálogo funcional antes de depender de pedidos/checkout.
- API admin isolada facilita controle de acesso (JWT admin-only).
- Arquitetura limpa permite adicionar módulos sem reescrever core.

### Trade-offs
- Carrinho e checkout ficam fora do escopo inicial.
- Integração S3 (imagens) planejada mas não implementada na primeira fase.

## Próximos ADRs
- [ADR-01](01-Definição%20esquemas%20adicionais%20sistema.md) — Arquitetura CQRS, MediatR e Entidades
- [ADR-02](02-Implementação%20cruds.md) — Implementação dos CRUDs
