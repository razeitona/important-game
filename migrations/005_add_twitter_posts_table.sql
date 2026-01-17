-- Migration: Add TwitterPosts table for tracking posted tweets
-- Purpose: Prevent duplicate posts for the same match and track monthly quota usage
-- Created: 2026-01-15

CREATE TABLE IF NOT EXISTS TwitterPosts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MatchId INTEGER NOT NULL,
    TweetId TEXT,
    Content TEXT NOT NULL,
    PostedDateUTC TEXT NOT NULL,
    FOREIGN KEY (MatchId) REFERENCES Matches(MatchId)
);

-- Index for fast duplicate checking by match
CREATE INDEX IF NOT EXISTS idx_twitter_posts_match ON TwitterPosts(MatchId);

-- Index for monthly quota queries (counting posts within date range)
CREATE INDEX IF NOT EXISTS idx_twitter_posts_date ON TwitterPosts(PostedDateUTC);
