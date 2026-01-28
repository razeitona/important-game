-- Migration: Add NewsletterSubscribers table for email subscriptions
-- Purpose: Store email addresses for weekly newsletter subscribers
-- Created: 2026-01-24

CREATE TABLE IF NOT EXISTS NewsletterSubscribers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Email TEXT NOT NULL UNIQUE,
    SubscribedDateUTC TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    UnsubscribedDateUTC TEXT,
    UnsubscribeToken TEXT NOT NULL UNIQUE
);

-- Index for fast email lookup (duplicate checking, unsubscribe)
CREATE INDEX IF NOT EXISTS idx_newsletter_email ON NewsletterSubscribers(Email);

-- Index for active subscribers (for sending newsletters)
CREATE INDEX IF NOT EXISTS idx_newsletter_active ON NewsletterSubscribers(IsActive) WHERE IsActive = 1;

-- Index for unsubscribe token lookup
CREATE INDEX IF NOT EXISTS idx_newsletter_token ON NewsletterSubscribers(UnsubscribeToken);
