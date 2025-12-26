# Manual Intervention Required: Database Migrations

## Tabelas a Criar

### 1. `competition_table`
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

CREATE INDEX idx_competition_table_competition_id ON competition_table(competition_id);
CREATE INDEX idx_competition_table_updated_at ON competition_table(updated_at);
```

### 2. `team_season_stats`
```sql
CREATE TABLE team_season_stats (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    team_id INTEGER NOT NULL,
    competition_id INTEGER NOT NULL,
    goals_for_5 INTEGER NOT NULL,
    goals_against_5 INTEGER NOT NULL,
    wins_5 INTEGER NOT NULL,
    draws_5 INTEGER NOT NULL,
    losses_5 INTEGER NOT NULL,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (team_id) REFERENCES team(id),
    FOREIGN KEY (competition_id) REFERENCES competition(id),
    UNIQUE(team_id, competition_id)
);

CREATE INDEX idx_team_season_stats_team_competition ON team_season_stats(team_id, competition_id);
CREATE INDEX idx_team_season_stats_updated_at ON team_season_stats(updated_at);
```

## Alteração à Tabela Existente

### 1. `match` - Adicionar Coluna
```sql
ALTER TABLE match ADD COLUMN gemini_stats_updated_at DATETIME;
```

## Configuração Necessária

### 1. `appsettings.json`
Adicione a seguinte seção de configuração:

```json
"Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "RateLimitPerMinute": 15,
    "RequestTimeoutSeconds": 30,
    "CompetitionTableCacheDays": 1,
    "TeamStatsMaxAgeDays": 1
}
```

**Obtenha a chave em:** https://ai.google.dev/

## Dependency Injection

Adicione ao `DependencyInjectionSetup.cs`:

```csharp
services.AddScoped<ICompetitionTableRepository, CompetitionTableRepository>();
services.AddScoped<ITeamSeasonStatsRepository, TeamSeasonStatsRepository>();
services.AddHttpClient<IGeminiApiClient, GeminiApiClient>();
```

## Status de Implementação

- ? Entidades criadas: `CompetitionTable`, `TeamSeasonStats`
- ? Repositórios criados com queries centralizadas
- ? Cliente Gemini implementado com rate limiting
- ? Campo `GeminiStatsUpdatedAt` adicionado a `Match`
- ? Migração de BD (manual)
- ? Configuração de injeção de dependências (manual)
- ? Integração com `CalculateUpcomingMatchsExcitment` (próximo passo)
