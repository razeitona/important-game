-- Migration: Create UserFavoriteTeams table
-- Date: 2026-01-01
-- Description: Adds table to store user favorite teams

CREATE TABLE IF NOT EXISTS "UserFavoriteTeams" (
	"UserId"		INTEGER NOT NULL,
	"TeamId"		INTEGER NOT NULL,
	"AddedAt"		TEXT NOT NULL,
	PRIMARY KEY("UserId", "TeamId"),
	FOREIGN KEY("UserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
	FOREIGN KEY("TeamId") REFERENCES "Teams"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_UserFavoriteTeams_UserId" ON "UserFavoriteTeams" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserFavoriteTeams_TeamId" ON "UserFavoriteTeams" ("TeamId");
