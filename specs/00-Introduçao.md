# Neostore – Visão Geral e Escopo

## Objetivo Principal
Focar na criação de uma API robusta para **cadastro de itens** (produtos) que será a base do catálogo. Essa funcionalidade será implementada primeiro, permitindo depois expandir para carrinho, checkout e demais recursos.

## Modelo de Dados Simplificado
- **Produto**: `Id`, `Nome`, `SKU`, `Preço`, `idCategoria`, `Descrição`, `Imagens` (array), `Estoque`
- **Categoria**: `Id`, `Nome`, `Slug`, `idCategoriaPai`
- **Usuário Admin**: `Id`, `Email`, `SenhaHash`, `Role`

## Endpoints Essenciais (API Admin)
| Método | Caminho                            | Descrição                                                         |
| ------ | ---------------------------------- | ----------------------------------------------------------------- |
| POST   | `/api/admin/produtos`              | Criar produto – recebe JSON com os campos acima e grava no banco. |
| GET    | `/api/admin/produtos/{id}`         | Recupera detalhes de um produto pelo `Id`.                        |
| PUT    | `/api/admin/produtos/{id}`         | Atualiza todos os campos de um produto existente.                 |
| PATCH  | `/api/admin/produtos/{id}/estoque` | Ajusta apenas a quantidade em estoque (`Estoque`).                |
| DELETE | `/api/admin/produtos/{id}`         | Remove o produto do catálogo (soft‑delete opcional).              |

## Segurança e Boas Práticas na API de Cadastro
- Autenticação JWT para endpoints administrativos.
- Validação de entrada com `DataAnnotations` + `FluentValidation`.
- Proteção contra injeções SQL através de EF Core.
- Logging auditável das operações CRUD (usuário, timestamp, dados alterados).

## Fluxo de Trabalho Inicial
1. **Definir o esquema de banco** no MariaDB e gerar migrations EF Core.
2. **Implementar** os endpoints acima na API .NET 10.
3. **Testes unitários** para cada endpoint usando xUnit/Moq.
4. **Documentação Swagger/OpenAPI** auto‑gerada.
5. **Deploy local** (Docker Compose) e verificação funcional.

[[01-Definição esquemas adicionais sistema]]
[[02-Implementação cruds]]

