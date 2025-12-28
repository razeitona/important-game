# Database Schema

## Visão Geral

Serve este documento para descrever o schema da base de dados utilizadas na aplicação

---

## Modelo de Dados

### Tabela: `Competitions`
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

#### Exemplo de dados

```sql
7	UEFA Champions League	#3c1c5a	#ffffff	1.0	1	
8	LaLiga	#ffffff	#2f4a89	0.85	1	
17	Premier League	#3c1c5a	#3d195b	0.95	1	
18	Championship	#3c1c5a	#ffffff	0.5	1	
23	Serie A	#ffffff	#09519e	0.82	1	
34	Ligue 1	#3c1c5a	#ffffff	0.75	1	
35	Bundesliga	#ffffff	#e2080e	0.85	1	
36	Scottish Premiership	#311b77	#ffffff	0.55	1	
37	Eredivisie	#122e62	#122e62	0.7	1	
52	Trendyol Süper Lig	#f00515	#f00918	0.6	1	
155	Liga Profesional de Fútbol	#004a79	#33c5df	0.65	1	
185	Stoiximan Super League	#3c1c5a	#ffffff	0.55	1	
238	Liga Portugal Betclic	#001841	#ffc501	0.6	1	
325	Brasileirão Série A	#141528	#C7FF00	0.7	1	
955	Saudi Pro League	#ffffff	#2c9146	0.4	1	
11621	Liga MX, Apertura	#3c1c5a	#ffffff	0.65	1	
```

---

### Tabela: `CompetitionSeasons`
Armazena informação de temporadas associado a cada `Competitions`.
O campo `SeasonYear` indica o ano da temporada (e.g., "2023/2024").

```sql
CREATE TABLE "CompetitionSeasons" (
	"SeasonId"	INTEGER NOT NULL,
	"CompetitionId"	INTEGER NOT NULL,
	"SeasonYear"	TEXT NOT NULL,
	"TitleHolderId"	INTEGER,
	"IsFinished"	INTEGER NOT NULL DEFAULT 0,
	"SyncStandingsDate"	TEXT,
	"SyncMatchesDate"	TEXT,
	FOREIGN KEY("CompetitionId") REFERENCES "Competitions"("CompetitionId"),
	FOREIGN KEY("TitleHolderId") REFERENCES "Teams"("Id"),
	PRIMARY KEY("SeasonId" AUTOINCREMENT)
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
	"ShortName"	TEXT,
	"ThreeLetterName"	TEXT,
	"NormalizedName"	TEXT
	CONSTRAINT "PK_team" PRIMARY KEY("Id" AUTOINCREMENT)
);
```

---

### Tabela: `ExternalProviders`
Armazena informação de providers externas

```sql
CREATE TABLE "ExternalProviders" (
	"ProviderId"			INTEGER NOT NULL,
	"Name"					TEXT NOT NULL,
	"MaxRequestsPerSecond"	INTEGER,
	"MaxRequestsPerMinute"	INTEGER,
	"MaxRequestsPerHour"	INTEGER,
	"MaxRequestsPerDay"		INTEGER,
	"MaxRequestsPerMonth"	INTEGER,
	CONSTRAINT "PK_ExternalProviders" PRIMARY KEY("ProviderId")
);
```

---

### Tabela: `ExternalProvidersLogs`
Armazena logs de pedidos a cada provider

```sql
CREATE TABLE "ExternalProvidersLogs" (
	"ProviderId"	INTEGER NOT NULL,
	"RequestPath"	TEXT NOT NULL,
	"RequestDate"	TEXT NOT NULL,
	PRIMARY KEY("ProviderId")
);
```

---

#### Exemplo de dados

```sql
1	FootballData
```

---

### Tabela: `ExternalProviderTeams`
Armazena informação do id da equipa interno da tabela `Teams` equipas com o Id externo da integração associada.

```sql
CREATE TABLE "ExternalProviderTeams" (
	"ProviderId"		INTEGER NOT NULL,
	"InternalTeamId"	INTEGER NOT NULL,
	"ExternalTeamId"	INTEGER NOT NULL,
	PRIMARY KEY("ProviderId","InternalTeamId","ExternalTeamId"),
	FOREIGN KEY("InternalTeamId") REFERENCES "Teams"("Id")
);
```

---

### Tabela: `ExternalProviderCompetitions`
Armazena informação do id da competition interno da tabela `Competitions` com o Id externo da integração associada.

```sql
CREATE TABLE "ExternalProviderCompetitions" (
	"ProviderId"			INTEGER NOT NULL,
	"InternalCompetitionId"	INTEGER NOT NULL,
	"ExternalCompetitionId"	TEXT NOT NULL,
	FOREIGN KEY("InternalCompetitionId") REFERENCES "Competitions"("CompetitionId"),
	PRIMARY KEY("ProviderId","InternalCompetitionId","ExternalCompetitionId")
);
```

---

### Tabela: `ExternalProviderCompetitionSeasons`
Armazena informação do id da competition season interno da tabela `Competitions` com o Id externo da integração associada.

```sql
CREATE TABLE "ExternalProviderCompetitionSeasons" (
	"ProviderId"	INTEGER NOT NULL,
	"InternalSeasonId"	INTEGER NOT NULL,
	"ExternalSeasonId"	TEXT NOT NULL,
	FOREIGN KEY("InternalSeasonId") REFERENCES "CompetitionSeasons"("SeasonId"),
	PRIMARY KEY("InternalSeasonId","ProviderId","ExternalSeasonId")
);
```

---

### Tabela: `ExternalProviderMatches`
Armazena informação do id interno da tabela `Matches` com o Id externo da integração associada.

```sql
CREATE TABLE "ExternalProviderMatches" (
	"ProviderId"	INTEGER NOT NULL,
	"InternalMatchId"	INTEGER NOT NULL,
	"ExternalMatchId"	TEXT NOT NULL,
	FOREIGN KEY("InternalMatchId") REFERENCES "Matches"("MatchId"),
	PRIMARY KEY("ProviderId","InternalMatchId","ExternalMatchId")
);
```

---

### Tabela: `Matches`
Armazena informação de todos os jogos de uma dada competição.

```sql
CREATE TABLE "Matches" (
	"MatchId"	INTEGER NOT NULL,
	"CompetitionId"	INTEGER NOT NULL,
	"SeasonId"	INTEGER,
	"MatchDateUTC"	TEXT NOT NULL,
	"HomeTeamId"	INTEGER NOT NULL,
	"HomeTeamPosition"	INTEGER,
	"AwayTeamId"	INTEGER NOT NULL,
	"AwayTeamPosition"	INTEGER,
	"HomeScore"	INTEGER,
	"AwayScore"	INTEGER,
	"HomeForm"	TEXT,
	"AwayForm"	TEXT,
	"IsFinished"	INTEGER NOT NULL,
	"ExcitmentScore"	REAL,
	"CompetitionScore"	REAL,
	"FixtureScore"	REAL,
	"FormScore"	REAL,
	"GoalsScore"	REAL,
	"CompetitionStandingScore"	REAL,
	"HeadToHeadScore"	REAL,
	"RivalryScore"	REAL,
	"TitleHolderScore"	REAL,
	"UpdatedDateUTC"	TEXT,
	CONSTRAINT "PK_match" PRIMARY KEY("MatchId" AUTOINCREMENT),
	CONSTRAINT "FK_match_team_AwayTeamId" FOREIGN KEY("AwayTeamId") REFERENCES "Teams"("Id") ON DELETE CASCADE,
	CONSTRAINT "FK_match_team_HomeTeamId" FOREIGN KEY("HomeTeamId") REFERENCES "Teams"("Id") ON DELETE CASCADE,
	FOREIGN KEY("SeasonId") REFERENCES "CompetitionSeasons"("SeasonId"),
	CONSTRAINT "FK_match_competition_CompetitionId" FOREIGN KEY("CompetitionId") REFERENCES "Competitions"("CompetitionId") ON DELETE CASCADE
);
```



