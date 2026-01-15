-- Migration: Convert Live Excitement Score columns from INTEGER to REAL
-- Purpose: Change data type to support decimal values (0.0-1.0 range)
-- Created: 2026-01-11
--
-- Since SQLite doesn't support ALTER COLUMN TYPE directly, we need to:
-- 1. Rename old table
-- 2. Create new table with correct types
-- 3. Copy data (converting INTEGER to REAL)
-- 4. Drop old table
-- 5. Recreate indexes

-- IMPORTANT: This script assumes the Matches table structure.
-- Make sure to backup your database before running this migration!

-- Step 1: Start transaction for safety
BEGIN TRANSACTION;

-- Step 2: Rename existing table
ALTER TABLE Matches RENAME TO Matches_old;

-- Step 3: Create new Matches table with REAL types for live score columns
CREATE TABLE Matches (
    MatchId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompetitionId INTEGER NOT NULL,
    SeasonId INTEGER,
    Round INTEGER,
    MatchDateUTC TEXT NOT NULL,
    HomeTeamId INTEGER NOT NULL,
    AwayTeamId INTEGER NOT NULL,
    HomeScore INTEGER,
    AwayScore INTEGER,
    IsFinished INTEGER NOT NULL DEFAULT 0,

    -- Pre-match excitement score and components
    ExcitmentScore REAL DEFAULT 0.0,
    CompetitionScore REAL DEFAULT 0.0,
    CompetitionStandingScore REAL DEFAULT 0.0,
    FixtureScore REAL DEFAULT 0.0,
    FormScore REAL DEFAULT 0.0,
    GoalsScore REAL DEFAULT 0.0,
    HeadToHeadScore REAL DEFAULT 0.0,
    RivalryScore REAL DEFAULT 0.0,
    TitleHolderScore REAL DEFAULT 0.0,

    -- Team form and positions
    HomeForm TEXT,
    AwayForm TEXT,
    HomeTeamPosition INTEGER,
    AwayTeamPosition INTEGER,

    -- Live excitement score and components (REAL type for decimal values)
    LiveExcitementScore REAL DEFAULT NULL,
    ScoreLineScore REAL DEFAULT 0.0,
    XGoalsScore REAL DEFAULT 0.0,
    TotalFoulsScore REAL DEFAULT 0.0,
    TotalCardsScore REAL DEFAULT 0.0,
    PossessionScore REAL DEFAULT 0.0,
    BigChancesScore REAL DEFAULT 0.0,

    -- Timestamps
    UpdatedDateUTC TEXT,

    -- Foreign keys
    FOREIGN KEY (CompetitionId) REFERENCES Competitions(CompetitionId),
    FOREIGN KEY (SeasonId) REFERENCES Seasons(SeasonId),
    FOREIGN KEY (HomeTeamId) REFERENCES Teams(Id),
    FOREIGN KEY (AwayTeamId) REFERENCES Teams(Id)
);

-- Step 4: Copy data from old table, converting INTEGER to REAL where needed
INSERT INTO Matches (
    MatchId, CompetitionId, SeasonId, Round, MatchDateUTC,
    HomeTeamId, AwayTeamId, HomeScore, AwayScore, IsFinished,
    ExcitmentScore, CompetitionScore, CompetitionStandingScore,
    FixtureScore, FormScore, GoalsScore, HeadToHeadScore,
    RivalryScore, TitleHolderScore,
    HomeForm, AwayForm, HomeTeamPosition, AwayTeamPosition,
    LiveExcitementScore, ScoreLineScore, XGoalsScore,
    TotalFoulsScore, TotalCardsScore, PossessionScore, BigChancesScore,
    UpdatedDateUTC
)
SELECT
    MatchId, CompetitionId, SeasonId, Round, MatchDateUTC,
    HomeTeamId, AwayTeamId, HomeScore, AwayScore, IsFinished,
    ExcitmentScore, CompetitionScore, CompetitionStandingScore,
    FixtureScore, FormScore, GoalsScore, HeadToHeadScore,
    RivalryScore, TitleHolderScore,
    HomeForm, AwayForm, HomeTeamPosition, AwayTeamPosition,
    -- Convert INTEGER to REAL (divide by 100 if value exists, otherwise NULL)
    CASE WHEN LiveExcitementScore IS NULL OR LiveExcitementScore = 0 THEN NULL ELSE CAST(LiveExcitementScore AS REAL) / 100.0 END,
    CAST(ScoreLineScore AS REAL) / 100.0,
    CAST(XGoalsScore AS REAL) / 100.0,
    CAST(TotalFoulsScore AS REAL) / 100.0,
    CAST(TotalCardsScore AS REAL) / 100.0,
    CAST(PossessionScore AS REAL) / 100.0,
    CAST(BigChancesScore AS REAL) / 100.0,
    UpdatedDateUTC
FROM Matches_old;

-- Step 5: Drop old table
DROP TABLE Matches_old;

-- Step 6: Recreate indexes
CREATE INDEX IF NOT EXISTS idx_matches_competition
ON Matches(CompetitionId);

CREATE INDEX IF NOT EXISTS idx_matches_date
ON Matches(MatchDateUTC);

CREATE INDEX IF NOT EXISTS idx_matches_finished
ON Matches(IsFinished);

CREATE INDEX IF NOT EXISTS idx_matches_live_excitement
ON Matches(LiveExcitementScore)
WHERE IsFinished = 0;

-- Step 7: Commit transaction
COMMIT;

-- Verification query (run this after migration to verify)
-- SELECT MatchId, ExcitmentScore, LiveExcitementScore, ScoreLineScore
-- FROM Matches
-- WHERE LiveExcitementScore IS NOT NULL
-- LIMIT 10;
