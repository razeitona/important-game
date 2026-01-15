-- Migration: Add REAL columns for Live Excitement Scores (alongside existing INTEGER columns)
-- Purpose: Add new REAL-type columns to properly store decimal values (0.0-1.0 range)
-- Created: 2026-01-11
--
-- Strategy: Since SQLite doesn't support ALTER COLUMN TYPE, we:
-- 1. Add new REAL columns with _real suffix
-- 2. The application will use these new columns going forward
-- 3. Old INTEGER columns can be kept for reference or dropped later
--
-- This is the safest approach - no data loss risk.

-- Add new REAL columns
ALTER TABLE Matches ADD COLUMN LiveExcitementScore_real REAL DEFAULT NULL;
ALTER TABLE Matches ADD COLUMN ScoreLineScore_real REAL DEFAULT 0.0;
ALTER TABLE Matches ADD COLUMN XGoalsScore_real REAL DEFAULT 0.0;
ALTER TABLE Matches ADD COLUMN TotalFoulsScore_real REAL DEFAULT 0.0;
ALTER TABLE Matches ADD COLUMN TotalCardsScore_real REAL DEFAULT 0.0;
ALTER TABLE Matches ADD COLUMN PossessionScore_real REAL DEFAULT 0.0;
ALTER TABLE Matches ADD COLUMN BigChancesScore_real REAL DEFAULT 0.0;

-- Copy existing data from INTEGER columns to REAL columns (converting values)
-- Divide by 100 to convert from INTEGER (0-100) to REAL (0.0-1.0)
UPDATE Matches
SET LiveExcitementScore_real = CASE
    WHEN LiveExcitementScore IS NULL THEN NULL
    WHEN LiveExcitementScore = 0 THEN NULL
    ELSE CAST(LiveExcitementScore AS REAL) / 100.0
END
WHERE LiveExcitementScore IS NOT NULL;

UPDATE Matches
SET ScoreLineScore_real = CAST(ScoreLineScore AS REAL) / 100.0
WHERE ScoreLineScore IS NOT NULL;

UPDATE Matches
SET XGoalsScore_real = CAST(XGoalsScore AS REAL) / 100.0
WHERE XGoalsScore IS NOT NULL;

UPDATE Matches
SET TotalFoulsScore_real = CAST(TotalFoulsScore AS REAL) / 100.0
WHERE TotalFoulsScore IS NOT NULL;

UPDATE Matches
SET TotalCardsScore_real = CAST(TotalCardsScore AS REAL) / 100.0
WHERE TotalCardsScore IS NOT NULL;

UPDATE Matches
SET PossessionScore_real = CAST(PossessionScore AS REAL) / 100.0
WHERE PossessionScore IS NOT NULL;

UPDATE Matches
SET BigChancesScore_real = CAST(BigChancesScore AS REAL) / 100.0
WHERE BigChancesScore IS NOT NULL;

-- Create index for the new REAL column
CREATE INDEX IF NOT EXISTS idx_matches_live_excitement_real
ON Matches(LiveExcitementScore_real)
WHERE IsFinished = 0;

-- Notes:
-- 1. Application code should be updated to use the _real columns
-- 2. Old INTEGER columns can be dropped in a future migration after verification
-- 3. To drop old columns later, you'll need to recreate the table (SQLite limitation)
