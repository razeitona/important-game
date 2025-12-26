# Important Game - Arquitetura e Princípios SOLID

## ????? Nível de Complexidade: Senior C# Developer

Este projeto requer conhecimento avançado de C# e arquitetura de software. O código segue rigorosamente os princípios SOLID e padrões de design modernos.

---

## ??? Princípios SOLID Implementados

### **S - Single Responsibility Principle**
- Cada repositório é responsável por apenas um agregado:
  - `CompetitionRepository` ? operações de Competition
  - `TeamRepository` ? operações de Team
  - `MatchRepository` (MatchRepositoryDapper) ? operações de Match
  - `LiveMatchRepository` ? operações de LiveMatch
  - `RivalryRepository` ? operações de Rivalry
  - `HeadToHeadRepository` ? operações de HeadToHead

### **O - Open/Closed Principle**
- Repositórios são abertos para extensão através de interfaces:
  - Novas implementações (e.g., SqlServer, PostgreSQL) podem ser adicionadas sem modificar código existente
  - `IDbConnectionFactory` permite diferentes proveniências de dados

### **L - Liskov Substitution Principle**
- Todas as implementações de repositório respeitam seus contratos de interface
- `IMatchRepository` pode ser substituído por qualquer implementação que cumpra o contrato

### **I - Interface Segregation Principle**
- Interfaces especializadas por responsabilidade:
  - `ICompetitionRepository` - apenas operações de Competition
  - `ITeamRepository` - apenas operações de Team
  - Clients dependem apenas das interfaces que precisam

### **D - Dependency Inversion Principle**
- Dependência através de abstrações (`IDbConnectionFactory`, interfaces de repositório)
- `DependencyInjectionSetup.cs` centraliza a configuração
- Injeção de dependências em construtores

---

## ??? Arquitetura de Dados

### Camada de Queries
**Local:** `Data/Repositories/Queries/`

Todas as queries SQL estão centralizadas como constantes estáticas:
- `CompetitionQueries.cs` - 5 queries
- `TeamQueries.cs` - 4 queries
- `MatchQueries.cs` - 10 queries
- `LiveMatchQueries.cs` - 3 queries
- `RivalryQueries.cs` - 3 queries
- `HeadToHeadQueries.cs` - 2 queries

**Benefícios:**
- Queries centralizadas e reutilizáveis
- Fácil auditoria e otimização SQL
- Versionamento de queries simplificado

### Camada de Conexão
**Local:** `Data/Connections/`

- `IDbConnectionFactory` - Abstração para criar conexões
- `SqliteConnectionFactory` - Implementação SQLite
- Padrão Factory para criar conexões

### Camada de Repositório
**Local:** `Data/Repositories/`

Cada repositório:
1. Depende de `IDbConnectionFactory`
2. Utiliza queries estáticas de seu ficheiro correspondente
3. Implementa sua interface específica
4. Mapeia resultados SQL para entidades

---

## ?? Fluxo de Dados

```
Serviço (e.g., ExcitementMatchProcessor)
    ?
Interface de Repositório (e.g., IMatchRepository)
    ?
Implementação de Repositório (e.g., MatchRepositoryDapper)
    ?
Query Estática (e.g., MatchQueries.SelectMatchById)
    ?
IDbConnectionFactory (SqliteConnectionFactory)
    ?
SQLite Database
```

---

## ?? Padrões de Design Utilizados

### Factory Pattern
- `IDbConnectionFactory` cria instâncias de `IDbConnection`
- Separa a criação de conexões da sua utilização

### Repository Pattern
- Abstrai acesso a dados
- Fornece interface orientada a objetos para operações de BD

### Dependency Injection
- Todos os repositórios recebem dependências no construtor
- Configurado centralmente em `DependencyInjectionSetup.cs`

---

## ?? Guidelines para Desenvolvimento

### Ao Adicionar Uma Nova Entidade:

1. **Criar a Interface do Repositório**
   ```csharp
   public interface INewEntityRepository
   {
       Task SaveNewEntityAsync(NewEntity entity);
       Task<NewEntity?> GetNewEntityByIdAsync(int id);
   }
   ```

2. **Criar Queries Estáticas**
   ```csharp
   internal static class NewEntityQueries
   {
       internal const string SelectNewEntityById = "SELECT ... FROM new_entity WHERE id = @Id";
   }
   ```

3. **Implementar Repositório**
   ```csharp
   public class NewEntityRepository : INewEntityRepository
   {
       private readonly IDbConnectionFactory _connectionFactory;
       
       public NewEntityRepository(IDbConnectionFactory connectionFactory)
       {
           _connectionFactory = connectionFactory;
       }
       
       public async Task<NewEntity?> GetNewEntityByIdAsync(int id)
       {
           using (var connection = _connectionFactory.CreateConnection())
           {
               return await connection.QueryFirstOrDefaultAsync<NewEntity>(
                   NewEntityQueries.SelectNewEntityById, 
                   new { Id = id });
           }
       }
   }
   ```

4. **Registar no DependencyInjection**
   ```csharp
   services.AddScoped<INewEntityRepository, NewEntityRepository>();
   ```

### Ao Modificar Queries:

1. **Atualizar constante em `*Queries.cs`**
2. **Não modificar queries inline em repositórios**
3. **Criar testes unitários para validar mudanças**

---

## ?? Regras Importantes

### NÃO FAZER:
- ? Colocar lógica de negócio em repositórios
- ? Usar `IMatchRepository` para operações de múltiplas entidades
- ? Queries hardcoded em repositórios (devem estar em `*Queries.cs`)
- ? Modificar `IDbConnectionFactory` sem avaliar impacto

### FAZER:
- ? Uma interface por agregado/entidade
- ? Queries centralizadas como constantes
- ? Tratamento de nulo em métodos públicos
- ? Usar `ArgumentNullException.ThrowIfNull()` para validação
- ? Documentação XML em interfaces e métodos públicos
- ? Injetar apenas dependências necessárias

---

## ?? Testing

Ao escrever testes:

```csharp
// Sempre mockar IDbConnectionFactory
var mockFactory = new Mock<IDbConnectionFactory>();
var mockConnection = new Mock<IDbConnection>();

mockFactory
    .Setup(f => f.CreateConnection())
    .Returns(mockConnection.Object);

var repository = new MatchRepository(mockFactory.Object);
```

---

## ?? Referências

- **SOLID Principles**: https://en.wikipedia.org/wiki/SOLID
- **Repository Pattern**: https://martinfowler.com/eaaCatalog/repository.html
- **Dependency Injection**: https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
- **Dapper ORM**: https://github.com/DapperLib/Dapper

---

## ?? Estrutura do Projeto

```
src/important-game.infrastructure/
??? Data/
?   ??? Connections/
?   ?   ??? IDbConnectionFactory.cs
?   ?   ??? SqliteConnectionFactory.cs
?   ??? Repositories/
?       ??? Queries/
?       ?   ??? CompetitionQueries.cs
?       ?   ??? TeamQueries.cs
?       ?   ??? MatchQueries.cs
?       ?   ??? LiveMatchQueries.cs
?       ?   ??? RivalryQueries.cs
?       ?   ??? HeadToHeadQueries.cs
?       ??? ICompetitionRepository.cs
?       ??? CompetitionRepository.cs
?       ??? ITeamRepository.cs
?       ??? TeamRepository.cs
?       ??? IMatchRepository.cs
?       ??? MatchRepositoryDapper.cs
?       ??? ILiveMatchRepository.cs
?       ??? LiveMatchRepository.cs
?       ??? IRivalryRepository.cs
?       ??? RivalryRepository.cs
?       ??? IHeadToHeadRepository.cs
?       ??? HeadToHeadRepository.cs
??? ImportantMatch/
?   ??? ExcitementMatchProcessor.cs
?   ??? ExcitmentMatchService.cs
?   ??? ...
??? DependencyInjectionSetup.cs
```

---

## ?? Notas Importantes

1. **Migrations**: As queries foram criadas para SQLite. Adaptar conforme necessário para outros DBs.

2. **Performance**: Queries são otimizadas para Dapper. Não adicionar lógica complexa no repositório.

3. **Escalabilidade**: A separação de repositórios permite fácil implementação de padrões como:
   - Unit of Work
   - Query Objects
   - Specifications Pattern

4. **Testes**: Todas as dependências são injetáveis, facilitando testes unitários com Moq ou xUnit.

---

**Última Atualização:** 2024
**Versão:** 1.0 - Migração Dapper com SOLID
