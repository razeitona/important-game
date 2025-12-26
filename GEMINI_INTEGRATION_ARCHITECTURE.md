# Gemini API Integration - Arquitetura do Processo de Enriquecimento de Dados

## ?? Visão Geral

O processo `CalculateUpcomingMatchsExcitment` será estendido para integrar a **Gemini API**, substituindo SofaScore e FootballAPI. O objetivo é:

1. Obter dados de calendário, classificação e estatísticas de equipas
2. Guardar de forma otimizada na BD
3. Determinar inteligentemente se necessita atualizar ou reutilizar dados existentes
4. Respeitar limites do free tier (15 req/min)

---

## ??? Modelo de Dados

### Tabela: `competition_table` (Nova)
Armazena snapshot da classificação de cada competição.

```sql
CREATE TABLE competition_table (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    competition_id INTEGER NOT NULL,
    team_id INTEGER NOT NULL,
    position INTEGER NOT NULL,
    points INTEGER NOT NULL,
    matches INTEGER NOT NULL,
    wins INTEGER NOT NULL,
    draws INTEGER NOT NULL,
    losses INTEGER NOT NULL,
    goals_for INTEGER NOT NULL,
    goals_against INTEGER NOT NULL,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (competition_id) REFERENCES competition(id),
    FOREIGN KEY (team_id) REFERENCES team(id),
    UNIQUE(competition_id, team_id)
);
```

**Índices:**
- `idx_competition_table_competition_id` ? queries por competição
- `idx_competition_table_updated_at` ? queries para determinar relevância

---

### Tabela: `team_season_stats` (Nova)
Armazena estatísticas de equipa nos últimos 5 jogos.

```sql
CREATE TABLE team_season_stats (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    team_id INTEGER NOT NULL,
    competition_id INTEGER NOT NULL,
    goals_for_5 INTEGER NOT NULL,      -- Golos marcados últimos 5 jogos
    goals_against_5 INTEGER NOT NULL,  -- Golos sofridos últimos 5 jogos
    wins_5 INTEGER NOT NULL,
    draws_5 INTEGER NOT NULL,
    losses_5 INTEGER NOT NULL,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (team_id) REFERENCES team(id),
    FOREIGN KEY (competition_id) REFERENCES competition(id),
    UNIQUE(team_id, competition_id)
);
```

**Índices:**
- `idx_team_season_stats_team_competition` ? queries específicas
- `idx_team_season_stats_updated_at` ? rastrear relevância

---

### Campo Adicional: `Match.gemini_stats_updated_at` (Novo)
Adicionar à tabela `match` para rastrear quando foram obtidas as estatísticas de equipa.

```sql
ALTER TABLE match ADD COLUMN gemini_stats_updated_at DATETIME;
```

---

## ?? Fluxo Principal: `CalculateUpcomingMatchsExcitment`

### Fase 1: Inicialização
```
1. Obter competições ativas da BD
2. Por cada competição:
   - Determinar se tabela precisa atualização (timestamp < now)
   - Determinar se calendário precisa atualização (houve Match nova?)
   - Construir lista de dados a pedir ao Gemini
```

### Fase 2: Integração Gemini
```
3. Agrupar requisições por competição:
   ? Calendar (se necessário)
   ? Competition Table (se necessário)
   ? Team Stats para jogos da próxima semana (se necessário)

4. Fazer requisições respeitando rate limit (1 req/competição)
5. Parse JSON responses e guardar em BD
```

### Fase 3: Processamento de Jogos
```
6. Obter jogos não terminados da próxima semana
7. Por cada jogo:
   - Se não tem stats ou stats desatualizadas ? pedir ao Gemini
   - Calcular ExcitmentScore (conforme atual)
   - Guardar Match atualizado
```

### Fase 4: Otimização (Sem Requisição Gemini)
```
8. Atualizar tabela de classificação manualmente:
   - Calcular pontos a partir de Matches já existentes
   - Atualizar competition_table sem chamar API
```

---

## ?? Lógica de Relevância de Dados

### Quando Pedir Calendar ao Gemini?
```csharp
// Primeira execução OU calendário vazio
var hasCalendar = await matchRepository
    .GetMatchesFromCompetitionAsync(competitionId)
    .AnyAsync();

if (!hasCalendar)
    requestCalendar = true;
```

### Quando Pedir Competition Table ao Gemini?
```csharp
var lastTableUpdate = await competitionTableRepository
    .GetLastUpdateAsync(competitionId);

// Se nunca foi atualizada OU se há Matches novas desde última atualização
var hasNewMatches = await matchRepository
    .HasMatchesCreatedAfterAsync(competitionId, lastTableUpdate);

if (lastTableUpdate == null || hasNewMatches)
    requestTable = true;
```

### Quando Pedir Team Stats ao Gemini?
```csharp
// Apenas para jogos da próxima semana
var upcomingWeekMatches = await matchRepository
    .GetUpcomingMatchesFromCompetitionAsync(competitionId)
    .Where(m => m.MatchDateUTC < now.AddDays(7))
    .ToListAsync();

foreach (var match in upcomingWeekMatches)
{
    var stats = await teamSeasonStatsRepository
        .GetByTeamAndCompetitionAsync(match.HomeTeamId, competitionId);
    
    // Se stats não existem ou foram solicitadas há >X tempo
    if (stats == null || stats.UpdatedAt < now.AddDays(-1))
        requestTeamStats.Add(match.HomeTeamId);
}
```

---

## ?? Estrutura de Request ao Gemini

```json
{
  "competitions": [
    {
      "id": 7,
      "name": "Champions League",
      "requests": {
        "calendar": {
          "needed": true,
          "reason": "first_execution"
        },
        "table": {
          "needed": true,
          "reason": "new_matches_detected"
        },
        "teamStats": {
          "needed": true,
          "teams": [
            {
              "id": 2829,
              "name": "Real Madrid",
              "lastDays": 5
            }
          ]
        }
      }
    }
  ]
}
```

---

## ?? Estratégia de Persistência

### 1. Calendário
```csharp
foreach (var geminiMatch in geminiCalendar.Matches)
{
    var match = new Match
    {
        Id = geminiMatch.Id,
        MatchDateUTC = geminiMatch.DateTime,
        // ... mapear demais campos
        UpdatedDateUTC = DateTime.UtcNow,
        GeminiStatsUpdatedAt = null // Será preenchido depois
    };
    
    await matchRepository.SaveMatchAsync(match);
}
```

### 2. Tabela Classificativa
```csharp
var tableRows = new List<CompetitionTable>();

foreach (var team in geminiTable.Teams)
{
    tableRows.Add(new CompetitionTable
    {
        CompetitionId = competitionId,
        TeamId = team.Id,
        Position = team.Position,
        Points = team.Points,
        // ... demais campos
        UpdatedAt = DateTime.UtcNow
    });
}

await competitionTableRepository.SaveAllAsync(tableRows);
```

### 3. Team Stats (Últimos 5 Jogos)
```csharp
var stats = new TeamSeasonStats
{
    TeamId = team.Id,
    CompetitionId = competitionId,
    GoalsFor5 = geminiStats.GoalsFor,
    GoalsAgainst5 = geminiStats.GoalsAgainst,
    Wins5 = geminiStats.Wins,
    Draws5 = geminiStats.Draws,
    Losses5 = geminiStats.Losses,
    UpdatedAt = DateTime.UtcNow
};

await teamSeasonStatsRepository.SaveAsync(stats);

// Atualizar Match com timestamp
match.GeminiStatsUpdatedAt = DateTime.UtcNow;
await matchRepository.SaveMatchAsync(match);
```

---

## ?? Segunda Execução (Subsequentes)

**O processo será mais eficiente:**

1. **Calendário:** Já existe ? skipado
2. **Tabela:** Verifica se há Matches novas
   - Se SIM ? pedir ao Gemini
   - Se NÃO ? recalcular a partir de `Match` existentes
3. **Team Stats:** Apenas para jogos que:
   - Ainda não têm stats (`GeminiStatsUpdatedAt` NULL)
   - Stats expiradas (`GeminiStatsUpdatedAt` < now - 1 dia)

---

## ?? Error Handling

### Se Gemini Falhar
```csharp
try
{
    var geminiData = await geminiApiClient.FetchCompetitionDataAsync(request);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Gemini API failed for competition {CompetitionId}", competitionId);
    // Abortar processamento desta competição
    continue; // Próxima competição
}
```

### Rate Limiting
- Max 15 req/min (free tier)
- 1 req/competição
- Se houver >15 competições ? distribuir em múltiplas execuções ou aumentar intervalo do job

---

## ?? Otimização: Recalcular Tabela Sem API

**Quando não há Match nova:**
```csharp
// Recalcular pontos a partir de Matches existentes
var matchesForCompetition = await matchRepository
    .GetMatchesFromCompetitionAsync(competitionId)
    .Where(m => m.MatchStatus != MatchStatus.Upcoming)
    .ToListAsync();

var standings = new Dictionary<int, (int Wins, int Draws, int Losses, int GF, int GA)>();

foreach (var match in matchesForCompetition)
{
    // Agregar resultados
    // Home team
    if (match.HomeScore > match.AwayScore)
        standings[match.HomeTeamId].Wins++;
    else if (match.HomeScore == match.AwayScore)
        standings[match.HomeTeamId].Draws++;
    else
        standings[match.HomeTeamId].Losses++;
    
    standings[match.HomeTeamId].GF += match.HomeScore;
    standings[match.HomeTeamId].GA += match.AwayScore;
    
    // Away team (similar logic)
    // ...
}

// Guardar em competition_table
await competitionTableRepository.SaveAllAsync(standings);
```

---

## ?? Configurações (appsettings.json)

```json
{
  "Gemini": {
    "ApiKey": "",
    "RateLimitPerMinute": 15,
    "RequestTimeoutSeconds": 30,
    "CompetitionTableCacheDays": 1,
    "TeamStatsMaxAgeDays": 1
  }
}
```

---

## ?? Timing e Scheduling

**Job Atual:** `MatchCalculatorJob` - A cada 1 hora

**Com Gemini:**
- Primeira execução: pode levar mais tempo (múltiplas requisições)
- Execuções seguintes: mais rápidas (apenas dados vencidos)
- Rate limit: distribuir requisições ao longo da hora se necessário

---

## ?? Próximos Passos de Implementação

1. ? Criar tabelas `competition_table` e `team_season_stats`
2. ? Criar repositórios: `ICompetitionTableRepository`, `ITeamSeasonStatsRepository`
3. ? Criar cliente Gemini: `IGeminiApiClient`
4. ? Implementar lógica de relevância em `CalculateUpcomingMatchsExcitment`
5. ? Integrar requests ao Gemini
6. ? Mapear respostas JSON para entidades
7. ? Testes unitários

---

**Versão:** 1.0  
**Data:** 2024  
**Status:** Em Revisão
