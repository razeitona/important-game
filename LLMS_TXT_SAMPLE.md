# Sample llms.txt Output

This file shows what the generated `llms.txt` will look like when the service runs.

---

# Match to Watch

> Match to Watch helps football fans discover the most exciting matches worth watching.
> We analyze hundreds of matches weekly using our Excitement Score (ES) algorithm that considers
> historical rivalries, team form, league standings, and fixture importance to predict match excitement.

## Overview

Match to Watch is a platform dedicated to helping football enthusiasts identify the most thrilling matches
to watch from the hundreds of games played each week. Our unique Excitement Score (ES) system analyzes
various factors to predict how exciting a match will be, ranging from 0 to 100.

### How It Works

The Excitement Score algorithm considers multiple factors:

- **Historical Rivalries**: Classic rivalries bring extra intensity
- **Form & Momentum**: Recent team performance and playing style
- **League Standings**: Title races, relegation battles, top-4 fights
- **Head-to-Head History**: Past encounters and competitiveness patterns
- **Fixture Importance**: Cup finals, derbies, crucial seasonal matches
- **League Quality**: Top-tier competitions feature higher quality football

We also track matches in real-time, updating excitement scores based on live events such as goals,
cards, and possession battles. This means users can discover thrilling matches even after they've kicked off.

## Pages

- [Home](https://matchtowatch.com/): Main dashboard showing live matches, trending matches, and upcoming fixtures
- [Matches](https://matchtowatch.com/matches): Browse all upcoming matches sorted by excitement score and competition
- [Calendar](https://matchtowatch.com/calendar): Monthly calendar view of all matches with excitement indicators
- [About](https://matchtowatch.com/about): Detailed information about our mission and how the Excitement Score works
- [Favorites](https://matchtowatch.com/favorites): Personalized page for authenticated users to track their favorite matches

## Features

- **Excitement Score (ES)**: 0-100 score predicting match excitement
- **Live Match Tracking**: Real-time updates for ongoing matches
- **Multi-Competition Coverage**: Tracks top football leagues worldwide
- **Match Details**: Comprehensive analysis including head-to-head records, team form, and rivalry information
- **Favorites System**: Users can save matches for quick access (requires Google authentication)
- **Responsive Design**: Optimized for desktop and mobile viewing

## Optional

### Match Detail Pages

Each match has a dedicated detail page accessible via URL pattern:
`https://matchtowatch.com/match/{home-team-slug}-vs-{away-team-slug}`

Match pages include:
- Excitement Score breakdown and explanation
- Team statistics and current form
- Head-to-head history between teams
- Competition context and standings relevance
- Match date and time in UTC

### Technical Information

- Built with ASP.NET Core 8.0 and Razor Pages
- Uses SQLite database for match data storage
- Integrates with football data APIs for match information
- Google OAuth for user authentication
- Responsive SCSS-based styling with Bootstrap Icons
