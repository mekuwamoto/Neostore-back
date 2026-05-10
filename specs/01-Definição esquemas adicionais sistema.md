
 # Definição Técnica: CQRS, MediatR e Entidades de Domínio

Esta especificação detalha a estrutura lógica e as regras de negócio das entidades e a organização da camada de aplicação utilizando os padrões CQRS e MediatR.

## 1. Padrões de Aplicação
O projeto utiliza **Onion Architecture** para garantir o desacoplamento. A comunicação entre a camada de API e o Domínio será mediada pela camada de **Application** via **MediatR**.

### Estrutura de CQRS
As operações serão divididas em:
- **Commands:** Operações de escrita (Create, Update, Delete). Devem ser validadas antes da execução.
- **Queries:** Operações de leitura. Devem retornar DTOs otimizados para a visualização.
- **Handlers:** Contêm a orquestração da lógica necessária para processar um Command ou Query.

## 2. Dicionário de Entidades (Domínio)

### 2.1 Produto
Entidade que representa os itens do catálogo.

| Campo           | Tipo          | Descrição/Regras                                |
| :-------------- | :------------ | :---------------------------------------------- |
| **Id**          | Guid          | Identificador único (PK).                       |
| **Nome**        | String        | Nome comercial do produto. Obrigatório.         |
| **SKU**         | String        | Código de identificação único de estoque.       |
| **Preço**       | Decimal       | Valor de venda. Deve ser sempre > 0.            |
| **IdCategoria** | Guid          | Chave estrangeira para a categoria.             |
| **Descrição**   | String        | Detalhamento técnico/comercial.                 |
| **Imagens**     | Lista<String> | URLs das imagens associadas.                    |
| **Estoque**     | Inteiro       | Quantidade disponível. Nunca deve ser negativo. |

**Comportamentos Esperados:**
- Ajuste de estoque (incremento/decremento).
- Atualização de preço com validação histórica (opcional).
- Adição/Remoção de imagens.

### 2.2 Categoria
Estrutura para organização hierárquica.

| Campo | Tipo | Descrição/Regras |
| :--- | :--- | :--- |
| **Id** | Guid | Identificador único (PK). |
| **Nome** | String | Nome da categoria. |
| **Slug** | String | Identificador amigável para URLs. |
| **idCategoriaPai** | Guid (Anulável) | Auto-relacionamento para hierarquia. |

**Comportamentos Esperados:**
- Geração automática de Slug a partir do Nome.
- Validação de circularidade na hierarquia (uma categoria não pode ser pai de si mesma).

### 2.3 Usuário Admin
Gestão de credenciais administrativas.

| Campo | Tipo | Descrição/Regras |
| :--- | :--- | :--- |
| **Id** | Guid | Identificador único (PK). |
| **Email** | String | Endereço eletrônico (Único). |
| **SenhaHash** | String | Hash seguro da senha (nunca texto plano). |
| **Role** | String | Nível de acesso (ex: Admin, Gerente). |

## 3. Convenções de Nomenclatura
Conforme definido no `CLAUDE.md`, todas as chaves estrangeiras e referências entre entidades devem seguir o prefixo `id` seguido do nome da entidade alvo (CamelCase).
- Exemplo: `idCategoria`, `idProduto`, `idUsuario`.

## 4. Requisitos de Implementação (Application)
- Os Handlers de Command devem utilizar **FluentValidation** para validar o estado das requisições antes de interagir com o Domínio.
- O MediatR deve ser configurado para registrar todos os Handlers da assembly de Application automaticamente.








 