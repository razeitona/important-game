# Próximas Etapas - Integração Gemini API

## Status Atual ?

- ? Entidades criadas: `CompetitionTable`, `TeamSeasonStats`
- ? Repositórios com interface e implementação Dapper
- ? Queries SQL centralizadas
- ? Campo `GeminiStatsUpdatedAt` adicionado a `Match`
- ? Cliente Gemini com rate limiting (15 req/min)
- ? Modelos de request/response JSON estruturados
- ? Projeto compilável

---

## Fases Pendentes

### Fase 1: Setup de Dependências (Manual)
**Ficheiros:** `Manual_intervention.md`

**Ações necessárias:**
1. Executar migrations de BD (create tables + alter match)
2. Adicionar configurações Gemini ao `DependencyInjectionSetup.cs`
3. Configurar API key em `appsettings.json` (obter em https://ai.google.dev/)

---

### Fase 2: Lógica de Enriquecimento de Dados (A Implementar)

**Ficheiro:** `ExcitementMatchProcessor.cs`

**Responsabilidades:**
1. Determinar se dados precisam atualização (lógica de relevância)
2. Construir requests ao Gemini
3. Chamar `IGeminiApiClient.FetchCompetitionDataAsync()`
4. Mapear respostas para entidades
5. Persistir em repositórios

**Métodos a criar:**

```csharp
private async Task EnrichCompetitionDataAsync(Competition competition, CancellationToken ct)
{
    // 1. Verificar se calendário precisa atualização
    // 2. Verificar se tabela precisa atualização
    // 3. Obter jogos da próxima semana sem stats
    // 4. Construir request ao Gemini
    // 5. Chamar API e processar resposta
    // 6. Guardar em BD
}

private GeminiCompetitionRequest BuildGeminiRequest(Competition comp)
{
    // Construir estrutura de request
}

private async Task ProcessGeminiResponseAsync(GeminiCompetitionResponse response)
{
    // Mapear e persistir dados
}

private async Task RecalculateCompetitionTableAsync(int competitionId)
{
    // Calcular tabela a partir de Matches existentes
}
```

---

### Fase 3: Integração com Background Job

**Ficheiro:** `MatchCalculatorJob.cs`

**Mudanças:**
1. Injetar `IGeminiApiClient` no job
2. Chamar `EnrichCompetitionDataAsync()` antes de calcular scores
3. Adicionar tratamento de erros robusto

---

### Fase 4: Testes Unitários

**Ficheiro:** `tests/important-game.infrastructure.tests/`

**Casos de teste:**
1. Parser de resposta Gemini válida
2. Rate limiting funciona corretamente
3. Erro de timeout é capturado
4. Dados são mapeados corretamente

---

## Configuração Obrigatória

### 1. `appsettings.json`
```json
"Gemini": {
    "ApiKey": "CHAVE_DO_GEMINI_AQUI",
    "RateLimitPerMinute": 15,
    "RequestTimeoutSeconds": 30,
    "CompetitionTableCacheDays": 1,
    "TeamStatsMaxAgeDays": 1
}
```

### 2. `DependencyInjectionSetup.cs`
```csharp
services.AddScoped<ICompetitionTableRepository, CompetitionTableRepository>();
services.AddScoped<ITeamSeasonStatsRepository, TeamSeasonStatsRepository>();
services.AddHttpClient<IGeminiApiClient, GeminiApiClient>();
```

### 3. `ImportantMatchDbContext.cs` (EF Core)
```csharp
public DbSet<CompetitionTable> CompetitionTables { get; set; }
public DbSet<TeamSeasonStats> TeamSeasonStats { get; set; }
```

---

## Estrutura de Ficheiros Criada

```
src/important-game.infrastructure/
??? GeminiAPI/
?   ??? IGeminiApiClient.cs (?)
?   ??? GeminiApiClient.cs (?)
?   ??? Models/
?       ??? GeminiCompetitionRequest.cs (?)
?       ??? GeminiCompetitionResponse.cs (?)
??? Data/Repositories/
?   ??? ICompetitionTableRepository.cs (?)
?   ??? CompetitionTableRepository.cs (?)
?   ??? ITeamSeasonStatsRepository.cs (?)
?   ??? TeamSeasonStatsRepository.cs (?)
?   ??? Queries/
?       ??? CompetitionTableQueries.cs (?)
?       ??? TeamSeasonStatsQueries.cs (?)
??? Contexts/
?   ??? Competitions/Data/Entities/
?   ?   ??? CompetitionTable.cs (?)
?   ??? Matches/Data/Entities/
?       ??? Match.cs (? atualizado)
?       ??? TeamSeasonStats.cs (?)
```

---

## Fluxo de Dados Completo

```
MatchCalculatorJob (a cada 1h)
    ?
EnrichCompetitionDataAsync()
    ?
BuildGeminiRequest() ? Determina o que pedir
    ?
IGeminiApiClient.FetchCompetitionDataAsync()
    ?
Gemini API (com rate limiting)
    ?
ParseGeminiResponse() ? Extrai JSON
    ?
Mapeia para entidades:
    - Match (calendário)
    - CompetitionTable (tabela)
    - TeamSeasonStats (stats)
    ?
Persiste em BD via repositórios
    ?
CalculateUpcomingMatchsExcitment() ? Calcula scores com dados enriquecidos
```

---

## Notas Importantes

1. **Rate Limiting:** Máximo 15 requisições/minuto. Se >15 competições, distribuir em múltiplas execuções.

2. **Erro Handling:** Se Gemini falhar, o processamento aborta gracefully (log + continue próxima competição).

3. **Otimização:** Segunda execução em diante será muito mais rápida (apenas dados vencidos).

4. **Recalcular Tabela:** Sem necessidade de API se não há Matches novas.

---

## Checklist para Implementação

- [ ] Executar migrations BD
- [ ] Adicionar injeção de dependências
- [ ] Configurar API key Gemini
- [ ] Implementar lógica de enriquecimento em `ExcitementMatchProcessor`
- [ ] Testes unitários do cliente Gemini
- [ ] Testes de integração com BD
- [ ] Deploy e monitoramento

