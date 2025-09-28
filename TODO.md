# TODO

## High Priority
- [x] Guard zero denominators before computing live ratios to avoid `DivideByZeroException` (`src\\important-game.infrastructure\\ImportantMatch\\Live\\ExcitmentMatchLiveProcessor.cs`).
- [x] Fix live shots-on-target calculation to use away shots on goal, keeping excitement scoring accurate (`src\\important-game.infrastructure\\ImportantMatch\\Live\\ExcitmentMatchLiveProcessor.cs`).
- [x] Rework the goals component to use goal-based metrics instead of form scores (`src\\important-game.infrastructure\\ImportantMatch\\ExcitementMatchProcessor.cs`).

## Medium Priority
- [x] Move Telegram bot token/chat ID into configuration and wire the bot through DI-provided `HttpClient` (`src\\important-game.infrastructure\\Telegram\\TelegramBot.cs`).
- [x] Trigger hosted jobs immediately on startup and surface/log exceptions (`src\\important-game.web\\Services\\MatchCalculatorJob.cs`, `src\\important-game.web\\Services\\LiveMatchCalculatorJob.cs`).
- [x] Restore EF tracking behavior after no-tracking operations to prevent global side effects (`src\\important-game.infrastructure\\ImportantMatch\\Data\\ExcitmentMatchRepository.cs`).
- [x] Replace per-entity `SaveChangesAsync` and `Count()` checks with batched `AnyAsync` patterns to cut SQLite round trips (`src\\important-game.infrastructure\\ImportantMatch\\Data\\ExcitmentMatchRepository.cs`).

## Lower Priority
- [x] Parallelize league processing with bounded concurrency while observing SofaScore throttling (`src\important-game.infrastructure\ImportantMatch\ExcitementMatchProcessor.cs`).
- [x] Cache rivalry lookups during a processing run to avoid redundant repository calls (`src\important-game.infrastructure\ImportantMatch\ExcitementMatchProcessor.cs`).
- [x] Confirm the big-chance metric reads the correct SofaScore statistic instead of possession (`src\important-game.infrastructure\ImportantMatch\Live\ExcitmentMatchLiveProcessor.cs`).
- [x] Introduce cache eviction or size limits for non-expiring SofaScore responses (`src\important-game.infrastructure\SofaScoreAPI\SofaScoreIntegration.cs`).

## Natural Next Steps
1. Patch the scoring bugs and zero checks.
2. Add startup execution/logging improvements to hosted services.
3. Externalize Telegram credentials.
4. Refactor repository data access patterns before tuning concurrency.






