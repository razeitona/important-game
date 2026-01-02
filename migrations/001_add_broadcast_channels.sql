-- Migration: Add Broadcast Channels functionality
-- Date: 2026-01-02
-- Description: Adds tables for managing broadcast channels, match broadcasts, and user favorite channels

-- =====================================================
-- Table: BroadcastChannels
-- Stores information about TV broadcast channels
-- =====================================================
CREATE TABLE IF NOT EXISTS "BroadcastChannels" (
	"ChannelId"			INTEGER NOT NULL,
	"Name"				TEXT NOT NULL,
	"Code"				TEXT NOT NULL UNIQUE,
	"CountryCode"		TEXT NOT NULL,
	"LogoUrl"			TEXT,
	"PrimaryColor"		TEXT,
	"IsActive"			INTEGER NOT NULL DEFAULT 1,
	"CreatedAt"			TEXT NOT NULL,
	CONSTRAINT "PK_BroadcastChannels" PRIMARY KEY("ChannelId" AUTOINCREMENT)
);

CREATE INDEX "IX_BroadcastChannels_CountryCode" ON "BroadcastChannels" ("CountryCode");
CREATE INDEX "IX_BroadcastChannels_Code" ON "BroadcastChannels" ("Code");
CREATE INDEX "IX_BroadcastChannels_IsActive" ON "BroadcastChannels" ("IsActive");

-- Description of fields:
-- ChannelId: Unique internal ID of the broadcast channel
-- Name: Display name of the channel (e.g., "ESPN", "Sky Sports", "DAZN")
-- Code: Unique code identifier for the channel (e.g., "ESPN_US", "SKY_UK")
-- CountryCode: ISO 3166-1 alpha-2 country code (e.g., "US", "UK", "ES", "PT")
-- LogoUrl: URL or path to the channel logo image
-- PrimaryColor: Hex color code for the channel brand color (e.g., "#FF0000")
-- IsActive: Flag to indicate if the channel is active (1) or inactive (0)
-- CreatedAt: Timestamp when the channel was created (format "yyyy-MM-dd HH:mm:ss")

-- =====================================================
-- Table: MatchBroadcasts
-- Links matches to broadcast channels
-- =====================================================
CREATE TABLE IF NOT EXISTS "MatchBroadcasts" (
	"MatchId"			INTEGER NOT NULL,
	"ChannelId"			INTEGER NOT NULL,
	"BroadcastStartTime" TEXT,
	"Notes"				TEXT,
	"CreatedAt"			TEXT NOT NULL,
	PRIMARY KEY("MatchId", "ChannelId"),
	FOREIGN KEY("MatchId") REFERENCES "Matches"("MatchId") ON DELETE CASCADE,
	FOREIGN KEY("ChannelId") REFERENCES "BroadcastChannels"("ChannelId") ON DELETE CASCADE
);

CREATE INDEX "IX_MatchBroadcasts_MatchId" ON "MatchBroadcasts" ("MatchId");
CREATE INDEX "IX_MatchBroadcasts_ChannelId" ON "MatchBroadcasts" ("ChannelId");
CREATE INDEX "IX_MatchBroadcasts_BroadcastStartTime" ON "MatchBroadcasts" ("BroadcastStartTime");

-- Description of fields:
-- MatchId: Reference to the match being broadcast
-- ChannelId: Reference to the broadcast channel
-- BroadcastStartTime: Optional specific start time for the broadcast (may differ from match time)
-- Notes: Optional notes about the broadcast (e.g., "Pre-match coverage from 7:30 PM")
-- CreatedAt: Timestamp when the broadcast entry was created

-- =====================================================
-- Table: UserFavoriteBroadcastChannels
-- Stores user favorite broadcast channels
-- =====================================================
CREATE TABLE IF NOT EXISTS "UserFavoriteBroadcastChannels" (
	"UserId"			INTEGER NOT NULL,
	"ChannelId"			INTEGER NOT NULL,
	"AddedAt"			TEXT NOT NULL,
	PRIMARY KEY("UserId", "ChannelId"),
	FOREIGN KEY("UserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
	FOREIGN KEY("ChannelId") REFERENCES "BroadcastChannels"("ChannelId") ON DELETE CASCADE
);

CREATE INDEX "IX_UserFavoriteBroadcastChannels_UserId" ON "UserFavoriteBroadcastChannels" ("UserId");
CREATE INDEX "IX_UserFavoriteBroadcastChannels_ChannelId" ON "UserFavoriteBroadcastChannels" ("ChannelId");

-- Description of fields:
-- UserId: Reference to the user
-- ChannelId: Reference to the favorite broadcast channel
-- AddedAt: Timestamp when the channel was added to favorites (format "yyyy-MM-dd HH:mm:ss")

-- =====================================================
-- Sample Data (Optional - for testing)
-- =====================================================

-- Sample UK Channels
INSERT INTO "BroadcastChannels" ("Name", "Code", "CountryCode", "LogoUrl", "PrimaryColor", "IsActive", "CreatedAt")
VALUES
	('Sky Sports Premier League', 'SKY_SPORTS_PL_UK', 'GB', '/images/channels/sky-sports-pl.png', '#0B0F3C', 1, datetime('now')),
	('BT Sport 1', 'BT_SPORT_1_UK', 'GB', '/images/channels/bt-sport-1.png', '#6C1D5F', 1, datetime('now')),
	('TNT Sports 1', 'TNT_SPORTS_1_UK', 'GB', '/images/channels/tnt-sports-1.png', '#E50914', 1, datetime('now')),
	('BBC One', 'BBC_ONE_UK', 'GB', '/images/channels/bbc-one.png', '#000000', 1, datetime('now'));

-- Sample US Channels
INSERT INTO "BroadcastChannels" ("Name", "Code", "CountryCode", "LogoUrl", "PrimaryColor", "IsActive", "CreatedAt")
VALUES
	('ESPN', 'ESPN_US', 'US', '/images/channels/espn.png', '#D50A0A', 1, datetime('now')),
	('Fox Sports', 'FOX_SPORTS_US', 'US', '/images/channels/fox-sports.png', '#003A70', 1, datetime('now')),
	('CBS Sports', 'CBS_SPORTS_US', 'US', '/images/channels/cbs-sports.png', '#0B3D91', 1, datetime('now')),
	('NBC Sports', 'NBC_SPORTS_US', 'US', '/images/channels/nbc-sports.png', '#FFD100', 1, datetime('now'));

-- Sample Spanish Channels
INSERT INTO "BroadcastChannels" ("Name", "Code", "CountryCode", "LogoUrl", "PrimaryColor", "IsActive", "CreatedAt")
VALUES
	('DAZN España', 'DAZN_ES', 'ES', '/images/channels/dazn.png', '#FFED00', 1, datetime('now')),
	('Movistar LaLiga', 'MOVISTAR_LALIGA_ES', 'ES', '/images/channels/movistar-laliga.png', '#0095C9', 1, datetime('now')),
	('GOL', 'GOL_ES', 'ES', '/images/channels/gol.png', '#FF0000', 1, datetime('now'));

-- Sample Portuguese Channels
INSERT INTO "BroadcastChannels" ("Name", "Code", "CountryCode", "LogoUrl", "PrimaryColor", "IsActive", "CreatedAt")
VALUES
	('Sport TV1', 'SPORT_TV1_PT', 'PT', '/images/channels/sport-tv1.png', '#E20613', 1, datetime('now')),
	('Eleven Sports 1', 'ELEVEN_SPORTS_1_PT', 'PT', '/images/channels/eleven-sports-1.png', '#00A859', 1, datetime('now')),
	('TVI', 'TVI_PT', 'PT', '/images/channels/tvi.png', '#003F87', 1, datetime('now'));

-- Sample German Channels
INSERT INTO "BroadcastChannels" ("Name", "Code", "CountryCode", "LogoUrl", "PrimaryColor", "IsActive", "CreatedAt")
VALUES
	('Sky Sport Bundesliga', 'SKY_SPORT_BL_DE', 'DE', '/images/channels/sky-sport-bundesliga.png', '#0B0F3C', 1, datetime('now')),
	('DAZN Deutschland', 'DAZN_DE', 'DE', '/images/channels/dazn.png', '#FFED00', 1, datetime('now')),
	('ARD', 'ARD_DE', 'DE', '/images/channels/ard.png', '#001F5B', 1, datetime('now'));

-- Sample Italian Channels
INSERT INTO "BroadcastChannels" ("Name", "Code", "CountryCode", "LogoUrl", "PrimaryColor", "IsActive", "CreatedAt")
VALUES
	('DAZN Italia', 'DAZN_IT', 'IT', '/images/channels/dazn.png', '#FFED00', 1, datetime('now')),
	('Sky Sport Calcio', 'SKY_SPORT_CALCIO_IT', 'IT', '/images/channels/sky-sport-calcio.png', '#0B0F3C', 1, datetime('now')),
	('RAI 1', 'RAI_1_IT', 'IT', '/images/channels/rai-1.png', '#00428C', 1, datetime('now'));

-- Sample French Channels
INSERT INTO "BroadcastChannels" ("Name", "Code", "CountryCode", "LogoUrl", "PrimaryColor", "IsActive", "CreatedAt")
VALUES
	('Canal+ Sport', 'CANAL_PLUS_SPORT_FR', 'FR', '/images/channels/canal-plus-sport.png', '#000000', 1, datetime('now')),
	('beIN Sports 1', 'BEIN_SPORTS_1_FR', 'FR', '/images/channels/bein-sports-1.png', '#C8102E', 1, datetime('now')),
	('TF1', 'TF1_FR', 'FR', '/images/channels/tf1.png', '#004C97', 1, datetime('now'));
