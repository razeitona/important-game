-- Migration: Update Broadcast Channels Schema
-- Date: 2026-01-02
-- Description: Restructure broadcast channels with Countries table and channel-country relations

-- =====================================================
-- Table: Countries
-- Stores country information
-- =====================================================
CREATE TABLE IF NOT EXISTS "Countries" (
	"CountryCode"		TEXT NOT NULL,
	"CountryName"		TEXT NOT NULL,
	CONSTRAINT "PK_Countries" PRIMARY KEY("CountryCode")
);

-- Insert common countries
INSERT INTO "Countries" ("CountryCode", "CountryName") VALUES
	('GB', 'United Kingdom'),
	('US', 'United States'),
	('ES', 'Spain'),
	('PT', 'Portugal'),
	('DE', 'Germany'),
	('IT', 'Italy'),
	('FR', 'France'),
	('BR', 'Brazil'),
	('AR', 'Argentina'),
	('MX', 'Mexico'),
	('NL', 'Netherlands'),
	('BE', 'Belgium'),
	('TR', 'Turkey'),
	('GR', 'Greece'),
	('SA', 'Saudi Arabia');

-- =====================================================
-- Drop and recreate BroadcastChannels without CountryCode, PrimaryColor
-- =====================================================
DROP TABLE IF EXISTS "BroadcastChannels_OLD";
ALTER TABLE "BroadcastChannels" RENAME TO "BroadcastChannels_OLD";

CREATE TABLE "BroadcastChannels" (
	"ChannelId"			INTEGER NOT NULL,
	"Name"				TEXT NOT NULL,
	"Code"				TEXT NOT NULL UNIQUE,
	"LogoUrl"			TEXT,
	"IsActive"			INTEGER NOT NULL DEFAULT 1,
	"CreatedAt"			TEXT NOT NULL,
	CONSTRAINT "PK_BroadcastChannels" PRIMARY KEY("ChannelId" AUTOINCREMENT)
);

CREATE INDEX "IX_BroadcastChannels_Code" ON "BroadcastChannels" ("Code");
CREATE INDEX "IX_BroadcastChannels_IsActive" ON "BroadcastChannels" ("IsActive");

-- Migrate old data (removing CountryCode and PrimaryColor)
INSERT INTO "BroadcastChannels" ("ChannelId", "Name", "Code", "LogoUrl", "IsActive", "CreatedAt")
SELECT "ChannelId", "Name", "Code", "LogoUrl", "IsActive", "CreatedAt"
FROM "BroadcastChannels_OLD";

-- =====================================================
-- Table: BroadcastChannelCountries
-- Links channels to countries where they are available
-- =====================================================
CREATE TABLE IF NOT EXISTS "BroadcastChannelCountries" (
	"ChannelId"			INTEGER NOT NULL,
	"CountryCode"		TEXT NOT NULL,
	PRIMARY KEY("ChannelId", "CountryCode"),
	FOREIGN KEY("ChannelId") REFERENCES "BroadcastChannels"("ChannelId") ON DELETE CASCADE,
	FOREIGN KEY("CountryCode") REFERENCES "Countries"("CountryCode") ON DELETE CASCADE
);

CREATE INDEX "IX_BroadcastChannelCountries_ChannelId" ON "BroadcastChannelCountries" ("ChannelId");
CREATE INDEX "IX_BroadcastChannelCountries_CountryCode" ON "BroadcastChannelCountries" ("CountryCode");

-- Migrate country relationships from old schema
INSERT INTO "BroadcastChannelCountries" ("ChannelId", "CountryCode")
SELECT DISTINCT "ChannelId", "CountryCode"
FROM "BroadcastChannels_OLD";

-- Drop old table
DROP TABLE "BroadcastChannels_OLD";

-- =====================================================
-- Update MatchBroadcasts table (remove BroadcastStartTime and Notes)
-- =====================================================
DROP TABLE IF EXISTS "MatchBroadcasts_OLD";
ALTER TABLE "MatchBroadcasts" RENAME TO "MatchBroadcasts_OLD";

CREATE TABLE "MatchBroadcasts" (
	"MatchId"			INTEGER NOT NULL,
	"ChannelId"			INTEGER NOT NULL,
	"CountryCode"		TEXT NOT NULL,
	"CreatedAt"			TEXT NOT NULL,
	PRIMARY KEY("MatchId", "ChannelId", "CountryCode"),
	FOREIGN KEY("MatchId") REFERENCES "Matches"("MatchId") ON DELETE CASCADE,
	FOREIGN KEY("ChannelId") REFERENCES "BroadcastChannels"("ChannelId") ON DELETE CASCADE,
	FOREIGN KEY("CountryCode") REFERENCES "Countries"("CountryCode") ON DELETE CASCADE
);

CREATE INDEX "IX_MatchBroadcasts_MatchId" ON "MatchBroadcasts" ("MatchId");
CREATE INDEX "IX_MatchBroadcasts_ChannelId" ON "MatchBroadcasts" ("ChannelId");
CREATE INDEX "IX_MatchBroadcasts_CountryCode" ON "MatchBroadcasts" ("CountryCode");

-- Migrate old match broadcasts data (if exists)
-- Note: This requires manual intervention as we need CountryCode now
-- For now, we'll skip migration and let you add new data

-- Drop old table
DROP TABLE "MatchBroadcasts_OLD";
