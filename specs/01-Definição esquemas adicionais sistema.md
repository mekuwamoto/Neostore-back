# Definição Técnica: CQRS, MediatR e Entidades de Domínio

Esta especificação detalha a estrutura lógica e as regras de negócio das entidades e a organização da camada de aplicação utilizando os padrões CQRS e MediatR.

## 1. Status de Implementação

✅ **Entidades de Domínio** — Implementadas com testes unitários (53 testes, 100% coverage).
✅ **Estrutura CQRS** — Commands e Queries definidas.
✅ **Validators** — FluentValidation configurado para Commands.
✅ **DTOs** — Criados para todas as entidades.
⏳ **Handlers** — Próximo passo (vide `02-Implementação cruds.md`).

## 2. Padrões de Aplicação
O projeto utiliza **Onion Architecture** para garantir o desacoplamento. A comunicação entre a camada de API e o Domínio será mediada pela camada de **Application** via **MediatR**.

### Estrutura de CQRS
As operações estão organizadas em:
- **Commands:** Operações de escrita (Create, Update, Delete). Validadas via FluentValidation antes de execução.
- **Queries:** Operações de leitura. Retornam DTOs otimizados para visualização.
- **Handlers:** Contêm a orquestração da lógica necessária para processar um Command ou Query (implementação em andamento).

### Dependency Injection
MediatR e FluentValidation registrados automaticamente em `Application/DependencyInjection.cs`:
```csharp
services.AddMediatR(typeof(DependencyInjection));
services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
```

## 3. Dicionário de Entidades (Domínio)

### 3.1 Produto
Entidade que representa os itens do catálogo.

| Campo           | Tipo              | Descrição/Regras                                |
| :-------------- | :---------------- | :---------------------------------------------- |
| **Id**          | Guid              | Identificador único (PK).                       |
| **Nome**        | String            | Nome comercial do produto. Obrigatório.         |
| **SKU**         | String            | Código de identificação único de estoque.       |
| **Preço**       | Decimal           | Valor de venda. Deve ser sempre > 0.            |
| **IdCategoria** | Guid              | Chave estrangeira para a categoria.             |
| **Descrição**   | String            | Detalhamento técnico/comercial.                 |
| **Imagens**     | Lista\<Imagem\>   | Imagens associadas (gerenciadas por S3).        |
| **Estoque**     | Inteiro           | Quantidade disponível. Nunca deve ser negativo. |

**Comportamentos Implementados:**
- `AjustarEstoque(quantidade)` — Incremento/decremento com validação.
- `AdicionarImagem(imagem)` — Adiciona Imagem à lista e vincula ao produto.
- `RemoverImagem(idImagem)` — Remove Imagem por ID.

### 3.2 Imagem
Nova entidade para gerenciar imagens futuramente armazenadas em S3.

| Campo | Tipo | Descrição/Regras |
| :--- | :--- | :--- |
| **Id** | Guid | Identificador único (PK). |
| **NomeArquivo** | String | Nome original do arquivo. |
| **ChaveS3** | String | Caminho no bucket S3 (obrigatório). |
| **TipoConteudo** | String | MIME type (ex: image/jpeg). |
| **TamanhoBytes** | Long | Tamanho em bytes. |
| **IdProduto** | Guid | Referência ao produto. |
| **DataCriacao** | DateTime | Timestamp UTC da criação. |

**Comportamentos Implementados:**
- `ObterUrlS3(bucketUrl)` — Gera URL completa do bucket S3 + ChaveS3.

**Notas:**
- Futuramente será integrado com AWS S3 para upload/download.
- ChaveS3 é obrigatório para validação de imagens válidas.

### 3.3 Categoria
Estrutura para organização hierárquica.

| Campo | Tipo | Descrição/Regras |
| :--- | :--- | :--- |
| **Id** | Guid | Identificador único (PK). |
| **Nome** | String | Nome da categoria. |
| **Slug** | String | Identificador amigável para URLs. |
| **IdCategoriaPai** | Guid (Anulável) | Auto-relacionamento para hierarquia. |

**Comportamentos Implementados:**
- `GerarSlug(nome)` — Gera slug normalizado (remove acentos, converte para lowercase, substitui espaços por hífen).
- `ValidarHierarquia(categoriaPai)` — Valida circularidade (categoria não pode ser pai de si mesma).

### 3.4 Usuário Admin
Gestão de credenciais administrativas.

| Campo | Tipo | Descrição/Regras |
| :--- | :--- | :--- |
| **Id** | Guid | Identificador único (PK). |
| **Email** | String | Endereço eletrônico (Único). |
| **SenhaHash** | String | Hash seguro da senha (nunca texto plano). |
| **Role** | String | Nível de acesso (ex: Admin, Gerente). |

**Comportamentos Implementados:**
- `AtualizarSenha(novoHash)` — Atualiza hash com validação de não-vazio.

## 4. Estrutura de Commands e Queries

### 4.1 Commands Produto
- `CriarProdutoCommand` → `ProdutoDto`
- `AtualizarProdutoCommand` → `ProdutoDto`
- `DeletarProdutoCommand` → `bool`
- `AjustarEstoqueCommand` → `int` (novo estoque)

### 4.2 Queries Produto
- `ObterProdutoPorIdQuery` → `ProdutoDto?`
- `ObterTodosProdutosQuery` → `List<ProdutoDto>`

### 4.3 Commands Categoria
- `CriarCategoriaCommand` → `CategoriaDto`
- `AtualizarCategoriaCommand` → `CategoriaDto`
- `DeletarCategoriaCommand` → `bool`

### 4.4 Queries Categoria
- `ObterCategoriaPorIdQuery` → `CategoriaDto?`
- `ObterTodasCategoriasQuery` → `List<CategoriaDto>`

### 4.5 Commands UsuarioAdmin
- `CriarUsuarioAdminCommand` → `UsuarioAdminDto`
- `AtualizarSenhaCommand` → `bool`
- `DeletarUsuarioAdminCommand` → `bool`

### 4.6 Queries UsuarioAdmin
- `ObterUsuarioAdminPorIdQuery` → `UsuarioAdminDto?`
- `ObterTodosUsuariosAdminQuery` → `List<UsuarioAdminDto>`

## 5. Validators
Todos os Commands possuem Validators associados usando **FluentValidation**:
- `CriarProdutoCommandValidator` — Valida campos obrigatórios e tipos.
- `AtualizarProdutoCommandValidator` — Valida Id + campos produto.
- `CriarCategoriaCommandValidator` — Valida Nome.
- `AtualizarCategoriaCommandValidator` — Valida Id + Nome.
- `CriarUsuarioAdminCommandValidator` — Valida Email + Senha + Role.
- `AtualizarSenhaCommandValidator` — Valida senhas antigas/novas.

## 6. DTOs

### 6.1 ProdutoDto
```csharp
public class ProdutoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string SKU { get; set; }
    public decimal Preço { get; set; }
    public Guid IdCategoria { get; set; }
    public string Descrição { get; set; }
    public List<ImagemDto> Imagens { get; set; }
    public int Estoque { get; set; }
}
```

### 6.2 ImagemDto
```csharp
public class ImagemDto
{
    public Guid Id { get; set; }
    public string NomeArquivo { get; set; }
    public string ChaveS3 { get; set; }
    public string TipoConteudo { get; set; }
    public long TamanhoBytes { get; set; }
    public DateTime DataCriacao { get; set; }
}
```

### 6.3 CategoriaDto & UsuarioAdminDto
Estruturas simples mapeando campos das entidades (sem exposição de SenhaHash).

## 7. Convenções de Nomenclatura
Conforme definido no `CLAUDE.md`, todas as chaves estrangeiras seguem o prefixo `Id` + nome da entidade (CamelCase):
- Exemplo: `IdCategoria`, `IdProduto`, `IdUsuario`, `IdCategoriaPai`.

## 8. Testes Unitários
✅ **53 testes implementados** (xUnit + AwesomeAssertions):
- **ProdutoTests:** 12 testes (criação, estoque, imagens)
- **ImagemTests:** 11 testes (criação, URL S3, validações)
- **CategoriaTests:** 13 testes (slug, hierarquia, circularidade)
- **UsuarioAdminTests:** 17 testes (criação, atualização senha)

Todos os testes executados com 100% de sucesso.

## 9. Próximos Passos
Vide `02-Implementação cruds.md`:
1. Implementar Handlers para Commands e Queries.
2. Criar Repositories (Persistence layer).
3. Integração com EF Core e banco de dados.
4. Endpoints da API.
5. Integração S3 (futuro).




 