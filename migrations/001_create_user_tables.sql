-- Migration: Create User Authentication Tables
-- Date: 2025-12-31
-- Description: Creates tables for user authentication, favorite matches, and match voting

-- Users Table
CREATE TABLE IF NOT EXISTS "Users" (
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

CREATE INDEX IF NOT EXISTS "IX_Users_GoogleId" ON "Users" ("GoogleId");
CREATE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");

-- User Favorite Matches Table
CREATE TABLE IF NOT EXISTS "UserFavoriteMatches" (
	"UserId"		INTEGER NOT NULL,
	"MatchId"		INTEGER NOT NULL,
	"AddedAt"		TEXT NOT NULL,
	PRIMARY KEY("UserId", "MatchId"),
	FOREIGN KEY("UserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
	FOREIGN KEY("MatchId") REFERENCES "Matches"("MatchId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_UserFavoriteMatches_UserId" ON "UserFavoriteMatches" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserFavoriteMatches_MatchId" ON "UserFavoriteMatches" ("MatchId");

-- Match Votes Table
CREATE TABLE IF NOT EXISTS "MatchVotes" (
	"UserId"		INTEGER NOT NULL,
	"MatchId"		INTEGER NOT NULL,
	"VoteType"		INTEGER NOT NULL DEFAULT 1,
	"VotedAt"		TEXT NOT NULL,
	PRIMARY KEY("UserId", "MatchId"),
	FOREIGN KEY("UserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
	FOREIGN KEY("MatchId") REFERENCES "Matches"("MatchId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_MatchVotes_MatchId" ON "MatchVotes" ("MatchId");
