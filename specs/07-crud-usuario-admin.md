# ADR-07: CRUD de Usuários Admin

## Status
Proposed

## Date
2026-05-10

## Context
Entidade `UsuarioAdmin`, repositório `IUsuarioAdminRepository`, commands, queries e validators já existem. Handlers e controller ainda não implementados. Sem CRUD de usuário admin, não é possível criar credenciais para autenticação futura (JWT — ADR futuro).

Diferença de `Produto` e `Categoria`: senha nunca é exposta em DTO; hash de senha deve ocorrer no handler antes de persistir.

## Decision
Implementar handlers, DTO e controller para CRUD completo de `UsuarioAdmin`.

### Endpoints (`/api/admin/usuarios`)

| Método | Rota | Command/Query | Response |
| ------ | ---- | ------------- | -------- |
| `POST` | `/` | `CriarUsuarioAdminCommand` | `201 UsuarioAdminDto` |
| `GET` | `/` | `ObterTodosUsuariosAdminQuery` | `200 List<UsuarioAdminDto>` |
| `GET` | `/{id}` | `ObterUsuarioAdminPorIdQuery` | `200 UsuarioAdminDto` / `404` |
| `PUT` | `/{id}` | `AtualizarUsuarioAdminCommand` | `200 UsuarioAdminDto` / `404` |
| `PATCH` | `/{id}/senha` | `AtualizarSenhaCommand` | `204` / `400` / `404` |
| `DELETE` | `/{id}` | `DeletarUsuarioAdminCommand` | `204` / `404` |

### DTO

```csharp
public class UsuarioAdminDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    // SenhaHash nunca exposta
}
```

Adicionar ao `MappingProfile`:
```csharp
CreateMap<UsuarioAdmin, UsuarioAdminDto>();
```

### Commands e Queries

#### Existentes (definidos, handlers ausentes)
- `CriarUsuarioAdminCommand(Email, Senha, Role)` → `UsuarioAdminDto`
- `AtualizarSenhaCommand(Id, SenhaAtual, NovaSenha)` → `bool`
- `DeletarUsuarioAdminCommand(Id)` → `bool`
- `ObterTodosUsuariosAdminQuery` → `List<UsuarioAdminDto>`
- `ObterUsuarioAdminPorIdQuery(Id)` → `UsuarioAdminDto?`

#### Novo (ainda não definido)
- `AtualizarUsuarioAdminCommand(Id, Email, Role)` → `UsuarioAdminDto`

### Hashing de Senha

Handler `CriarUsuarioAdminCommandHandler` deve hashear a senha antes de persistir. Usar **BCrypt**:

```bash
dotnet add src/Neostore.Application/Neostore.Application.csproj package BCrypt.Net-Next
```

No handler:
```csharp
string senhaHash = BCrypt.Net.BCrypt.HashPassword(command.Senha);
UsuarioAdmin usuario = new(Guid.NewGuid(), command.Email, senhaHash, command.Role);
```

Handler `AtualizarSenhaCommandHandler`:
```csharp
// Verificar senha atual antes de atualizar
bool senhaCorreta = BCrypt.Net.BCrypt.Verify(command.SenhaAtual, usuario.SenhaHash);
if (!senhaCorreta) throw new InvalidOperationException("Senha atual incorreta.");

string novoHash = BCrypt.Net.BCrypt.HashPassword(command.NovaSenha);
usuario.AtualizarSenha(novoHash);
```

### Handlers

#### `CriarUsuarioAdminCommandHandler`
1. `IUsuarioAdminRepository.ExistePorEmailAsync(email)` — lança `InvalidOperationException` se já existe
2. Hashear senha com BCrypt
3. `new UsuarioAdmin(Guid.NewGuid(), email, senhaHash, role)`
4. `CriarAsync(usuario)` + `SaveChangesAsync()`
5. `mapper.Map<UsuarioAdmin, UsuarioAdminDto>(usuario)`

#### `AtualizarUsuarioAdminCommandHandler`
1. `ObterPorIdAsync(id)` — lança `InvalidOperationException` se null
2. Se email mudou: `ExistePorEmailAsync(novoEmail)` — lança se já existe em outro usuário
3. Atualizar `Email` e `Role` diretamente nas propriedades
4. `AtualizarAsync(usuario)` + `SaveChangesAsync()`
5. `mapper.Map<UsuarioAdmin, UsuarioAdminDto>(usuario)`

#### `AtualizarSenhaCommandHandler`
1. `ObterPorIdAsync(id)` — lança se null
2. `BCrypt.Verify(senhaAtual, usuario.SenhaHash)` — lança `InvalidOperationException` se incorreta
3. `usuario.AtualizarSenha(BCrypt.HashPassword(novaSenha))`
4. `AtualizarAsync(usuario)` + `SaveChangesAsync()`
5. Retorna `true`

#### `DeletarUsuarioAdminCommandHandler`
1. `ObterPorIdAsync(id)` — retorna `false` se null
2. `DeletarAsync(id)` — soft delete via repositório (`Ativo = false`, `DeletadoEm = UtcNow`)
3. Retorna `true`

#### `ObterTodosUsuariosAdminQueryHandler`
1. `ObterTodosAsync()` — filtro global `HasQueryFilter(u => u.Ativo)` já exclui inativos
2. `mapper.Map<List<UsuarioAdmin>, List<UsuarioAdminDto>>(usuarios)`

#### `ObterUsuarioAdminPorIdQueryHandler`
1. `ObterPorIdAsync(id)` — filtro global exclui inativos → retorna `null` se não encontrado
2. `mapper.Map<UsuarioAdmin, UsuarioAdminDto>(usuario)` ou `null`

### Validators

#### `CriarUsuarioAdminCommandValidator` (já existe — verificar regras)
- `Email`: obrigatório, formato válido
- `Senha`: obrigatório, mínimo 8 caracteres
- `Role`: obrigatório

#### `AtualizarSenhaCommandValidator` (já existe — verificar regras)
- `Id`: obrigatório
- `SenhaAtual`: obrigatório
- `NovaSenha`: obrigatório, mínimo 8 caracteres, diferente de `SenhaAtual`

#### `AtualizarUsuarioAdminCommandValidator` (novo)
- `Id`: obrigatório
- `Email`: obrigatório, formato válido
- `Role`: obrigatório

### Soft Delete

`UsuarioAdmin` implementa `ISoftDeletable`. `UsuarioAdminRepository.DeletarAsync` já faz soft delete. `HasQueryFilter(u => u.Ativo)` já configurado em `UsuarioAdminConfiguration`. Nenhuma mudança necessária no repositório.

## Consequences
### Positivo
- CRUD completo de usuário admin pronto para integração com autenticação JWT.
- Soft delete preserva trilha de auditoria.
- Senha nunca exposta — DTO só contém Id, Email, Role.
- Padrão idêntico a Produto/Categoria — sem nova infraestrutura.

### Trade-offs
- BCrypt adiciona dependência nova em `Neostore.Application`.
- `AtualizarUsuarioAdminCommand` precisa ser criado (não existe ainda).
- Verificação de senha atual em `AtualizarSenha` requer acesso a `SenhaHash` — handler precisa do hash da entidade, não do DTO.

## Decisões de Design

| Decisão | Escolha | Alternativa descartada |
| ------- | ------- | ---------------------- |
| Hashing | BCrypt (BCrypt.Net-Next) | SHA256 — sem salt nativo, inseguro |
| Atualizar senha | `PATCH /{id}/senha` separado | Campo senha em `PUT` — mistura auth com dados de perfil |
| Verificação de senha atual | Obrigatória em `AtualizarSenha` | Sem verificação — inseguro, permite troca sem autenticação |
| Exposição de `SenhaHash` | Nunca — ausente do DTO | Campo no DTO — vazamento de credencial |
| Soft delete | Herdado de `ISoftDeletable` | Hard delete — perde trilha de auditoria |

## Sequência de Implementação

1. Criar `AtualizarUsuarioAdminCommand` + `AtualizarUsuarioAdminCommandValidator`
2. Instalar `BCrypt.Net-Next` em `Neostore.Application`
3. Adicionar `CreateMap<UsuarioAdmin, UsuarioAdminDto>()` ao `MappingProfile`
4. Implementar 5 handlers (Criar, AtualizarUsuario, AtualizarSenha, Deletar, ObterTodos, ObterPorId)
5. Criar `UsuarioAdminController` com 6 endpoints + `[ProducesResponseType]`
6. Testes: handler tests + validator tests

## Testes Necessários

### Handler Tests (`Application/Handlers/UsuarioAdmin/`)
- `CriarUsuarioAdminCommandHandlerTests` — criação, email duplicado → exception
- `AtualizarUsuarioAdminCommandHandlerTests` — update, email duplicado em outro → exception, não encontrado → exception
- `AtualizarSenhaCommandHandlerTests` — senha atual incorreta → exception, sucesso
- `DeletarUsuarioAdminCommandHandlerTests` — soft delete, não encontrado → false
- `ObterTodosUsuariosAdminQueryHandlerTests` — lista, lista vazia
- `ObterUsuarioAdminPorIdQueryHandlerTests` — encontrado, não encontrado → null

### Validator Tests
- Verificar `CriarUsuarioAdminCommandValidator` e `AtualizarSenhaCommandValidator` existentes
- Adicionar testes para `AtualizarUsuarioAdminCommandValidator`
