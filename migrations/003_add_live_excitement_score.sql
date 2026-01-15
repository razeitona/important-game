-- Migration: Add Live Excitement Score columns to Matches table
-- Purpose: Store live match excitement score and its components
-- Created: 2026-01-11
--
-- The LiveExcitementScore is the real-time excitement score during live matches.
-- It starts from the pre-match ExcitementScore and evolves based on live statistics.
-- Components are individual metrics that contribute to the live score calculation.

-- Add main LiveExcitementScore column (0-100 range, replaces LiveExcitementBonus)
ALTER TABLE Matches DROP COLUMN LiveExcitementScore;
ALTER TABLE Matches ADD COLUMN LiveExcitementScore REAL;

-- Add live score component columns (individual metrics from live match data)
ALTER TABLE Matches DROP COLUMN ScoreLineScore;
ALTER TABLE Matches ADD COLUMN ScoreLineScore REAL;      -- Goal difference & competitiveness
ALTER TABLE Matches DROP COLUMN XGoalsScore;
ALTER TABLE Matches ADD COLUMN XGoalsScore REAL;         -- Expected goals quality
ALTER TABLE Matches DROP COLUMN TotalFoulsScore;
ALTER TABLE Matches ADD COLUMN TotalFoulsScore REAL;     -- Match intensity from fouls
ALTER TABLE Matches DROP COLUMN TotalCardsScore;
ALTER TABLE Matches ADD COLUMN TotalCardsScore REAL;     -- Cards (yellow/red) intensity
ALTER TABLE Matches DROP COLUMN PossessionScore;
ALTER TABLE Matches ADD COLUMN PossessionScore REAL;     -- Ball possession balance
ALTER TABLE Matches DROP COLUMN BigChancesScore;
ALTER TABLE Matches ADD COLUMN BigChancesScore REAL;     -- Big chances created

-- Add index for performance on live matches
CREATE INDEX IF NOT EXISTS idx_matches_live_excitement
ON Matches(LiveExcitementScore)
WHERE IsFinished = 0;

-- Initialize all existing matches with 0 values
--UPDATE Matches SET LiveExcitementScore = 0 WHERE LiveExcitementScore IS NULL;
--UPDATE Matches SET ScoreLineScore = 0 WHERE ScoreLineScore IS NULL;
--UPDATE Matches SET XGoalsScore = 0 WHERE XGoalsScore IS NULL;
--UPDATE Matches SET TotalFoulsScore = 0 WHERE TotalFoulsScore IS NULL;
--UPDATE Matches SET TotalCardsScore = 0 WHERE TotalCardsScore IS NULL;
--UPDATE Matches SET PossessionScore = 0 WHERE PossessionScore IS NULL;
--UPDATE Matches SET BigChancesScore = 0 WHERE BigChancesScore IS NULL;

-- Add comment explaining the difference between ExcitementScore and LiveExcitementScore
-- ExcitementScore: Pre-match calculated score based on competition, form, rivalry, etc.
-- LiveExcitementScore: Real-time score during live matches, starts from ExcitementScore and evolves with live events

-- ============================================================================
-- PART 2: Convert INTEGER columns to REAL (for decimal support 0.0-1.0)
-- ============================================================================
-- Since this migration was already executed with INTEGER types, we need to
-- recreate the table to change column types. SQLite doesn't support ALTER COLUMN TYPE.
--
-- BACKUP YOUR DATABASE BEFORE RUNNING THIS SECTION!

BEGIN TRANSACTION;

-- Step 1: Rename existing table
ALTER TABLE Matches RENAME TO Matches_old_migration003;

-- Step 2: Create new Matches table with REAL types
-- (This is a complete table recreation - adjust based on your actual schema)
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

    -- Pre-match excitement scores (REAL)
    ExcitmentScore REAL DEFAULT 0.0,
    CompetitionScore REAL DEFAULT 0.0,
    CompetitionStandingScore REAL DEFAULT 0.0,
    FixtureScore REAL DEFAULT 0.0,
    FormScore REAL DEFAULT 0.0,
    GoalsScore REAL DEFAULT 0.0,
    HeadToHeadScore REAL DEFAULT 0.0,
    RivalryScore REAL DEFAULT 0.0,
    TitleHolderScore REAL DEFAULT 0.0,

    -- Team info
    HomeForm TEXT,
    AwayForm TEXT,
    HomeTeamPosition INTEGER,
    AwayTeamPosition INTEGER,

    -- Live excitement scores (REAL instead of INTEGER)
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

-- Step 3: Copy all data from old table
-- Live scores remain as 0 (they will be recalculated by the LiveScoreCalculatorJob)
INSERT INTO Matches SELECT * FROM Matches_old_migration003;

-- Step 4: Drop old table
DROP TABLE Matches_old_migration003;

-- Step 5: Recreate indexes
CREATE INDEX IF NOT EXISTS idx_matches_competition ON Matches(CompetitionId);
CREATE INDEX IF NOT EXISTS idx_matches_date ON Matches(MatchDateUTC);
CREATE INDEX IF NOT EXISTS idx_matches_finished ON Matches(IsFinished);
CREATE INDEX IF NOT EXISTS idx_matches_live_excitement ON Matches(LiveExcitementScore) WHERE IsFinished = 0;

COMMIT;
