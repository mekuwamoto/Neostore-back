# Neostore.Tests

Unit tests for Application handlers, validators, and Domain entities.

## Stack

- **xUnit** — test runner
- **Moq** — mocking repositories and dependencies
- **AwesomeAssertions** — assertions (same API as FluentAssertions)
- **Coverlet** — coverage collection

**Never use `Assert` from xUnit.** Always use AwesomeAssertions.

## Structure

```
Application/
  Handlers/
    Categoria/     — 5 handler test files
    Produto/       — 6 handler test files
  Validators/
    CategoriaValidatorsTests.cs
    ProdutoValidatorsTests.cs
Domain/
  CategoriaTests.cs
  ProdutoTests.cs
  ImagemTests.cs
  UsuarioAdminTests.cs
Factories/
  AutoMapperFactory.cs
```

## AutoMapper in Tests

Never mock `IMapper` with Moq. Use the real profile:
```csharp
IMapper mapper = AutoMapperFactory.Create();
// or inline:
IMapper mapper = new MapperConfiguration(cfg =>
    cfg.AddProfile<MappingProfile>()).CreateMapper();
```

Pass `mapper` directly to handler constructors. This validates the mapping profile alongside the handler logic.

## Handler Test Pattern

```csharp
public class CriarCategoriaCommandHandlerTests
{
    private readonly Mock<ICategoriaRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly CriarCategoriaCommandHandler _handler;

    public CriarCategoriaCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICategoriaRepository>();
        _mapper = AutoMapperFactory.Create();
        _handler = new CriarCategoriaCommandHandler(_repositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_RetornaCategoriaDto()
    {
        // Arrange
        CriarCategoriaCommand command = new("Eletrônicos", null);
        _repositoryMock.Setup(r => r.ExistePorNomeAsync("Eletrônicos")).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.CriarAsync(It.IsAny<Categoria>())).Returns(Task.CompletedTask);

        // Act
        CategoriaDto resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Nome.Should().Be("Eletrônicos");
        _repositoryMock.Verify(r => r.CriarAsync(It.IsAny<Categoria>()), Times.Once);
    }
}
```

## Test Focus

Prioritize **business rules** over implementation details:
- Test what happens when a business invariant is violated (e.g., duplicate SKU, circular category, negative stock).
- Test that the correct repository methods are called (via `Verify`).
- Do NOT test internal implementation (e.g., which private method was called).

## Domain Test Pattern

Domain tests verify entity behavior without any mocks:
```csharp
[Fact]
public void AjustarEstoque_QuantidadeNegativa_LancaExcecao()
{
    Produto produto = new(Guid.NewGuid(), "Notebook", "SKU-001", 1000m, Guid.NewGuid(), "", 5);

    Action acao = () => produto.AjustarEstoque(-10);

    acao.Should().Throw<InvalidOperationException>();
}
```

## Validator Test Pattern

```csharp
[Fact]
public async Task Validar_NomeVazio_RetornaErro()
{
    CriarCategoriaCommandValidator validator = new();
    CriarCategoriaCommand command = new("", null);

    ValidationResult resultado = await validator.ValidateAsync(command);

    resultado.IsValid.Should().BeFalse();
    resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Nome");
}
```

## Handler Tests — Current Coverage

### Categoria (5 files)
| File | Scenarios |
|------|-----------|
| `CriarCategoriaCommandHandlerTests` | Criação com/sem pai, slug gerado, nome duplicado → exception |
| `AtualizarCategoriaCommandHandlerTests` | Update com pai, auto-referência → exception, não encontrado → exception |
| `DeletarCategoriaCommandHandlerTests` | Deleção, bloquear se tem produtos/subcategorias |
| `ObterTodasCategoriasQueryHandlerTests` | Lista retornada, lista vazia |
| `ObterCategoriaPorIdQueryHandlerTests` | Encontrado, não encontrado → null |

### Produto (6 files)
| File | Scenarios |
|------|-----------|
| `CriarProdutoCommandHandlerTests` | Criação, SKU duplicado → exception, categoria inexistente → exception |
| `AtualizarProdutoCommandHandlerTests` | Update, SKU duplicado em outro produto → exception |
| `DeletarProdutoCommandHandlerTests` | Soft-delete, produto não encontrado |
| `ObterTodosProdutosQueryHandlerTests` | Lista retornada |
| `ObterProdutoPorIdQueryHandlerTests` | Com imagens, não encontrado |
| `ObterProdutosPaginadoQueryHandlerTests` | Paginação, filtros por categoria/nome/SKU |

## Adding New Tests

1. Handler tests go in `Application/Handlers/<Entidade>/`.
2. Domain tests go in `Domain/`.
3. Validator tests go in `Application/Validators/`.
4. Always use `AutoMapperFactory.Create()` for handlers that receive `IMapper`.
5. Mock only external dependencies (repositories, services) — never mock domain entities.
