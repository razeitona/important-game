# important-game

Important Game surfaces the most exciting football fixtures of the day by combining SofaScore data with domain heuristics. The site ranks upcoming and live matches, explains why they matter, and provides detailed excitement breakdowns for fans who want to know what to watch next.

## Features

- Match excitement scoring based on league strength, form, fixtures, rivalries, goals, and live match statistics.
- Hourly background job that refreshes upcoming match calculations and a 10-minute job that keeps live matches up to date.
- Trending matches section that highlights the top fixtures for the current week alongside other noteworthy games.
- Match detail view with head-to-head history, team form, excitement factor explanations, and shareable metadata.
- Telegram notification hook that broadcasts high-priority matches to a configured channel.
- Puppeteer-driven SofaScore integration that fetches tournament, standings, fixture, and live event data directly from the public site.

## Solution Overview

- `src/important-game.web` - Razor Pages front end, background hosted services, Sass-driven styling, and API endpoints for manual recalculation.
- `src/important-game.infrastructure` - EF Core data access layer, excitement calculators, SofaScore integration, and Telegram client.
- `data/matchwatch.db` - Seeded SQLite database that the site mounts in Docker. A copy is also kept under `src/important-game.web` for local runs.
- `docker-compose.yml` and `src/important-game.web/Dockerfile` - Container definitions that publish the web app and mount the database volume.
- `invoke-calculator.sh` - Helper script that installs a cron job to poke the `/calculator` endpoint from a Linux host.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/).
- SQLite (only required if you want to inspect or modify the `matchwatch.db` file manually).
- The first run of the SofaScore integration downloads a headless Chromium build via PuppeteerSharp; make sure the host can write to its user profile folder.

## Local Development

1. Restore dependencies:

   ```bash
   dotnet restore important-game.sln
   ```

2. Ensure the application can find a database. The repository already includes `src/important-game.web/matchwatch.db`. To start from a fresh copy, replace it with `data/matchwatch.db` or point the `ConnectionStrings__DefaultConnection` setting to another SQLite file.

3. Run the site:

   ```bash
   dotnet run --project src/important-game.web
   ```

   The default launch profile listens on `https://localhost:7160` (with HTTP fallback at `http://localhost:5247`).

4. For faster feedback while editing Razor pages or Sass, use:

   ```bash
   dotnet watch --project src/important-game.web run
   ```

   In Debug builds the `AspNetCore.SassCompiler` package recompiles `.scss` files automatically.

## Refreshing Match Data

- Background services:
  - `MatchCalculatorJob` recalculates upcoming match excitement scores every hour.
  - `LiveMatchCalculatorJob` refreshes live match excitement every 10 minutes.
- Manual triggers:
  - `GET /calculator` recalculates upcoming matches on demand.
  - `GET /livematchcalculator` refreshes excitement scores for in-progress matches.
  - The `/calculator` endpoint is what the `invoke-calculator.sh` cron helper hits.
- Each completed calculation may push a summary message to the configured Telegram channel.

## Configuration

- Update `ConnectionStrings:DefaultConnection` in `src/important-game.web/appsettings.json` or provide `ConnectionStrings__DefaultConnection` as an environment variable when deploying.
- To use Telegram notifications, replace the placeholder token and chat id in `src/important-game.infrastructure/Telegram/TelegramBot.cs` or refactor the class to pull secrets from user secrets or environment variables before deploying.
- SofaScore calls rely on PuppeteerSharp. If your deployment environment restricts outbound network calls or headless browsers, configure an alternative league processor in `DependencyInjectionSetup`.

## Docker

1. Build and run with Docker Compose:

   ```bash
   docker compose up --build
   ```

2. The compose file:
   - Publishes ports `80` and `443`.
   - Mounts the host `./data` folder into `/app/data` so the container reads `matchwatch.db` and writes updates back to the host.
   - Supplies the connection string via `ConnectionStrings__DefaultConnection`.
   - Includes a health check and basic resource limits.

Stop the stack with `docker compose down`. The mounted volume keeps `matchwatch.db` between runs.

## Project Status

There are currently no automated tests in the repository. Before extending the calculators or integrations, consider adding unit tests around the excitement processor and an end-to-end smoke check for the Razor pages.
