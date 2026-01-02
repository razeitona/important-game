# Database Schema

## Vis�o Geral

Serve este documento para descrever o schema da base de dados utilizadas na aplica��o

---

## Modelo de Dados

### Tabela: `Competitions`
Armazena informação base de cada competição.
`UpdatedAt` indica a �ltima vez que dados de tabela foram sincronizados no formato "yyyy-MM-dd HH:mm:ss".

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
52	Trendyol S�per Lig	#f00515	#f00918	0.6	1	
155	Liga Profesional de F�tbol	#004a79	#33c5df	0.65	1	
185	Stoiximan Super League	#3c1c5a	#ffffff	0.55	1	
238	Liga Portugal Betclic	#001841	#ffc501	0.6	1	
325	Brasileir�o S�rie A	#141528	#C7FF00	0.7	1	
955	Saudi Pro League	#ffffff	#2c9146	0.4	1	
11621	Liga MX, Apertura	#3c1c5a	#ffffff	0.65	1	
```

---

### Tabela: `CompetitionSeasons`
Armazena informa��o de temporadas associado a cada `Competitions`.
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
Armazena informa��o de classifica��o de uma `Competition` associada a uma temporada `CompetitionSeasons`.

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
Armazena informa��o de equipas.

```sql
CREATE TABLE "Teams" (
	"Id"	INTEGER NOT NULL,
	"Name"	TEXT NOT NULL,
	"ShortName"	TEXT,
	"ThreeLetterName"	TEXT,
	"NormalizedName"	TEXT,
	"SlugName"	TEXT
	CONSTRAINT "PK_team" PRIMARY KEY("Id" AUTOINCREMENT)
);
```

---

### Tabela: `ExternalProviders`
Armazena informa��o de providers externas

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
Armazena informa��o do id da equipa interno da tabela `Teams` equipas com o Id externo da integra��o associada.

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
Armazena informa��o do id da competition interno da tabela `Competitions` com o Id externo da integra��o associada.

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
Armazena informa��o do id da competition season interno da tabela `Competitions` com o Id externo da integra��o associada.

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
Armazena informa��o do id interno da tabela `Matches` com o Id externo da integra��o associada.

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
Armazena informa��o de todos os jogos de uma dada competi��o.

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

---

## Authentication & User Management

### Tabela: `Users`
Armazena informação de utilizadores registados.

```sql
CREATE TABLE "Users" (
	"UserId"			INTEGER NOT NULL,
	"GoogleId"			TEXT NOT NULL UNIQUE,
	"Email"				TEXT NOT NULL,
	"Name"				TEXT,
	"ProfilePictureUrl"	TEXT,
	"PreferredTimezone"	TEXT DEFAULT 'UTC',
	"CreatedAt"			TEXT NOT NULL,
	"LastLoginAt"		TEXT,
	CONSTRAINT "PK_Users" PRIMARY KEY("UserId" AUTOINCREMENT)
);

CREATE INDEX "IX_Users_GoogleId" ON "Users" ("GoogleId");
CREATE INDEX "IX_Users_Email" ON "Users" ("Email");
```

#### Descrição dos campos:
- `UserId`: ID interno único do utilizador
- `GoogleId`: ID único fornecido pelo Google (sub claim do JWT)
- `Email`: Email do utilizador (do Google)
- `Name`: Nome completo do utilizador
- `ProfilePictureUrl`: URL da foto de perfil do Google
- `PreferredTimezone`: Timezone preferido (UTC, CET, PST, GMT)
- `CreatedAt`: Data de criação da conta (formato "yyyy-MM-dd HH:mm:ss")
- `LastLoginAt`: Última data de login

---

### Tabela: `UserFavoriteMatches`
Armazena matches marcados como favoritos pelos utilizadores.

```sql
CREATE TABLE "UserFavoriteMatches" (
	"UserId"		INTEGER NOT NULL,
	"MatchId"		INTEGER NOT NULL,
	"AddedAt"		TEXT NOT NULL,
	PRIMARY KEY("UserId", "MatchId"),
	FOREIGN KEY("UserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
	FOREIGN KEY("MatchId") REFERENCES "Matches"("MatchId") ON DELETE CASCADE
);

CREATE INDEX "IX_UserFavoriteMatches_UserId" ON "UserFavoriteMatches" ("UserId");
CREATE INDEX "IX_UserFavoriteMatches_MatchId" ON "UserFavoriteMatches" ("MatchId");
```

#### Descrição dos campos:
- `UserId`: Referência ao utilizador
- `MatchId`: Referência ao match favorito
- `AddedAt`: Data em que foi adicionado aos favoritos

---

### Tabela: `MatchVotes`
Armazena votos (likes) dos utilizadores em matches.

```sql
CREATE TABLE "MatchVotes" (
	"UserId"		INTEGER NOT NULL,
	"MatchId"		INTEGER NOT NULL,
	"VoteType"		INTEGER NOT NULL DEFAULT 1,
	"VotedAt"		TEXT NOT NULL,
	PRIMARY KEY("UserId", "MatchId"),
	FOREIGN KEY("UserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
	FOREIGN KEY("MatchId") REFERENCES "Matches"("MatchId") ON DELETE CASCADE
);

CREATE INDEX "IX_MatchVotes_MatchId" ON "MatchVotes" ("MatchId");
```

#### Descrição dos campos:
- `UserId`: Referência ao utilizador que votou
- `MatchId`: Referência ao match votado
- `VoteType`: Tipo de voto (1 = like/upvote, futuro: -1 = dislike)
- `VotedAt`: Data do voto

#### Agregação de votos:
Para obter o total de votos de um match:
```sql
SELECT MatchId, COUNT(*) as TotalVotes, SUM(VoteType) as VoteScore
FROM MatchVotes
GROUP BY MatchId;
```

---

### Tabela: `UserFavoriteTeams`
Armazena equipas marcadas como favoritas pelos utilizadores.

```sql
CREATE TABLE "UserFavoriteTeams" (
	"UserId"		INTEGER NOT NULL,
	"TeamId"		INTEGER NOT NULL,
	"AddedAt"		TEXT NOT NULL,
	PRIMARY KEY("UserId", "TeamId"),
	FOREIGN KEY("UserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
	FOREIGN KEY("TeamId") REFERENCES "Teams"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_UserFavoriteTeams_UserId" ON "UserFavoriteTeams" ("UserId");
CREATE INDEX "IX_UserFavoriteTeams_TeamId" ON "UserFavoriteTeams" ("TeamId");
```

#### Descrição dos campos:
- `UserId`: Referência ao utilizador
- `TeamId`: Referência à equipa favorita
- `AddedAt`: Data em que foi adicionada aos favoritos (formato "yyyy-MM-dd HH:mm:ss")

#### Query para obter matches de equipas favoritas:
```sql
SELECT DISTINCT m.*
FROM Matches m
INNER JOIN UserFavoriteTeams uft ON (m.HomeTeamId = uft.TeamId OR m.AwayTeamId = uft.TeamId)
WHERE uft.UserId = ?
  AND m.IsFinished = 0
  AND datetime(m.MatchDateUTC) >= datetime('now')
ORDER BY m.MatchDateUTC ASC;
```

---


