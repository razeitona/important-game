# Database Schema

## Visão Geral

Serve este documento para descrever o schema da base de dados utilizadas na aplicação

---

## Modelo de Dados

### Tabela: `Competition`
Armazena informação base de cada competição.
`UpdatedAt` indica a última vez que dados de tabela foram sincronizados no formato "yyyy-MM-dd HH:mm:ss".

```sql
CREATE TABLE "Competitions" (
	"CompetitionId"	INTEGER NOT NULL,
	"Name"	TEXT NOT NULL,
	"PrimaryColor"	TEXT NOT NULL,
	"BackgroundColor"	TEXT NOT NULL,
	"LeagueRanking"	REAL NOT NULL,
	"IsActive"	INTEGER NOT NULL,
	"UpdatedAt"	TEXT,
	CONSTRAINT "PK_competition" PRIMARY KEY("CompetitionId" AUTOINCREMENT)
);
```

---

### Tabela: `CompetitionSeasons`
Armazena informação de temporadas associado a cada `Competitions`.
O campo `SeasonYear` indica o ano da temporada (e.g., "2023/2024").

```sql
CREATE TABLE "CompetitionSeasons" (
	"SeasonId"	INTEGER,
	"CompetitionId"	INTEGER NOT NULL,
	"SeasonYear"	TEXT NOT NULL,
	"TitleHolderId"	INTEGER,
	PRIMARY KEY("SeasonId" AUTOINCREMENT),
	FOREIGN KEY("CompetitionId") REFERENCES "Competitions"("CompetitionId"),
	FOREIGN KEY("TitleHolderId") REFERENCES "Teams"("Id")
);
```

---

### Tabela: `CompetitionSeasons`
Armazena informação de classificação de uma `Competition` associada a uma temporada `CompetitionSeasons`.

```sql
CREATE TABLE "CompetitionTable" (
	"CompetitionId"	INTEGER NOT NULL,
	"SeasonId"	INTEGER NOT NULL,
	"TeamId"	INTEGER NOT NULL,
	"Position"	INTEGER NOT NULL,
	"Points"	INTEGER NOT NULL,
	"Matches"	INTEGER NOT NULL,
	"Wins"	INTEGER NOT NULL,
	"Draws"	INTEGER NOT NULL,
	"Losses"	INTEGER NOT NULL,
	"GoalsFor"	INTEGER NOT NULL,
	"GoalsAgainst"	INTEGER NOT NULL,
	FOREIGN KEY("SeasonId") REFERENCES "CompetitionSeasons"("SeasonId"),
	FOREIGN KEY("CompetitionId") REFERENCES "Competitions"("CompetitionId"),
	FOREIGN KEY("TeamId") REFERENCES "Teams"("Id"),
	UNIQUE("CompetitionId","TeamId"),
	PRIMARY KEY("CompetitionId","SeasonId","TeamId")
);
```

---

### Tabela: `Team`
Armazena informação de equipas.

```sql
CREATE TABLE "Teams" (
	"Id"	INTEGER NOT NULL,
	"Name"	TEXT NOT NULL,
	CONSTRAINT "PK_team" PRIMARY KEY("Id" AUTOINCREMENT)
);
```

---

### Tabela: `ExternalIntegration`
Armazena informação de integrações externas

```sql
CREATE TABLE "ExternalIntegration" (
	"IntegrationId"	INTEGER NOT NULL,
	"Name"	TEXT NOT NULL,
	CONSTRAINT "PK_ExternalIntegration" PRIMARY KEY("IntegrationId" AUTOINCREMENT)
);
```

---

### Tabela: `ExternalIntegrationTeam`
Armazena informação do id da equipa interno da tabela `Teams` equipas com o Id externo da integração associada.

```sql
CREATE TABLE "ExternalIntegrationTeam" (
	"IntegrationId"	INTEGER NOT NULL,
	"InternalTeamId"	INTEGER NOT NULL,
	"ExternalTeamId"	INTEGER NOT NULL,
	FOREIGN KEY("IntegrationId") REFERENCES "ExternalIntegration"("IntegrationId"),
	FOREIGN KEY("InternalTeamId") REFERENCES "Teams"("Id"),
	PRIMARY KEY("IntegrationId","InternalTeamId","ExternalTeamId")
);
```