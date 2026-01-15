# Live Score Calculator - Technical Specification

## Overview

The Live Score Calculator is a background job that monitors currently live football matches and updates their excitement scores in real-time using SofaScore data. This feature enables dynamic excitement scoring that reflects match events as they happen.

## Architecture

### Components

```
┌─────────────────────────────────────────────────────────────────┐
│                    LiveScoreCalculatorJob                        │
│                   (important-game.app)                           │
│  - Runs every 10 minutes                                        │
│  - Creates service scope                                         │
│  - Invokes IMatchCalculatorOrchestrator                         │
└─────────────────┬───────────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────────┐
│            IMatchCalculatorOrchestrator                         │
│          (important-game.infrastructure)                        │
│  - CalculateLiveScoreAsync() [NEW METHOD]                      │
│  - Orchestrates live score calculation workflow                │
└─────────────────┬───────────────────────────────────────────────┘
                  │
                  ├──────────────┬──────────────┬─────────────────┐
                  ▼              ▼              ▼                 ▼
         ┌──────────────┐ ┌─────────────┐ ┌──────────┐ ┌──────────────┐
         │IMatchesRepo  │ │ISofaScore   │ │IExternal │ │IMatchCalc    │
         │              │ │Integration  │ │Providers │ │              │
         │GetLiveMatches│ │GetAllLive   │ │Repo      │ │Calculate     │
         │[NEW]         │ │MatchesAsync │ │          │ │LiveBonus     │
         └──────────────┘ └─────────────┘ └──────────┘ └──────────────┘
```

### Data Flow

```
1. LiveScoreCalculatorJob (every 10 minutes)
   │
   ├─► 2. Get Internal Live Matches (DB Query)
   │      SELECT * FROM Matches WHERE IsFinished = 0 AND StartTime <= NOW
   │
   ├─► 3. Get SofaScore Live Matches (1 API Request)
   │      GET /api/v1/sport/football/events/live
   │
   ├─► 4. ID Mapping (Fuzzy Match + Persist)
   │      - Match teams by name (FuzzySharp 85% similarity)
   │      - Match by date (±2 hours tolerance)
   │      - Save to ExternalProviderMatches table
   │
   ├─► 5. For Each Mapped Match (Top 10 Priority)
   │      ├─► Get Statistics (1 API Request per match)
   │      │    GET /api/v1/event/{eventId}/statistics
   │      │
   │      ├─► Calculate Live Excitement Bonus
   │      │    - Goals: +15 per goal
   │      │    - Yellow cards: +5 each
   │      │    - Red cards: +20 each
   │      │    - High shots: +10 if >20 total
   │      │    - Balanced possession: +8 if diff <10%
   │      │
   │      └─► Update Match Excitement Score (DB UPDATE)
   │           UPDATE Matches SET LiveExcitementBonus = X WHERE MatchId = Y
   │
   └─► 6. Log Results & Complete
```

## Database Schema

### New Column (Optional Enhancement)

```sql
-- Add to Matches table (optional, can use existing ExcitmentScore field)
ALTER TABLE Matches ADD COLUMN LiveExcitementBonus INTEGER DEFAULT 0;
```

### Existing Tables Used

#### Matches Table
```sql
- MatchId (PK)
- HomeTeamId
- AwayTeamId
- StartTime
- IsFinished
- ExcitmentScore (updated with live bonus)
- LastUpdated
```

#### ExternalProviderMatches Table
```sql
- ProviderId (PK) -- 1 = SofaScore
- InternalMatchId (PK)
- ExternalMatchId -- SofaScore Event ID
```

## Implementation Plan

### Phase 1: Interface & Query Extensions

#### 1.1 IMatchCalculatorOrchestrator
**File:** `src/important-game.infrastructure/Contexts/ScoreCalculator/IMatchCalculatorOrchestrator.cs`

**Add Method:**
```csharp
/// <summary>
/// Calculates excitement scores for currently live matches using SofaScore data.
/// Updates scores every 10 minutes based on live match events.
/// </summary>
Task CalculateLiveScoreAsync(CancellationToken cancellationToken = default);
```

#### 1.2 IMatchesRepository
**File:** `src/important-game.infrastructure/Contexts/Matches/Data/IMatchesRepository.cs`

**Add Method:**
```csharp
/// <summary>
/// Gets all matches that are currently live (started but not finished).
/// </summary>
Task<List<LiveMatchDto>> GetLiveMatchesAsync();
```

**New DTO:**
```csharp
public class LiveMatchDto
{
    public int MatchId { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public string HomeTeamName { get; set; }
    public string AwayTeamName { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public int CurrentExcitementScore { get; set; }
}
```

### Phase 2: Repository Implementation

#### 2.1 MatchesQueries
**File:** `src/important-game.infrastructure/Contexts/Matches/Data/Queries/MatchesQueries.cs`

**Add Query:**
```sql
internal const string SelectLiveMatches = @"
    SELECT
        m.MatchId,
        m.HomeTeamId,
        m.AwayTeamId,
        ht.Name AS HomeTeamName,
        at.Name AS AwayTeamName,
        m.StartTime,
        m.ExcitmentScore AS CurrentExcitementScore
    FROM Matches m
    INNER JOIN Teams ht ON m.HomeTeamId = ht.TeamId
    INNER JOIN Teams at ON m.AwayTeamId = at.TeamId
    WHERE m.IsFinished = 0
      AND datetime(m.StartTime) <= datetime('now')
    ORDER BY m.StartTime";
```

#### 2.2 MatchesRepository
**File:** `src/important-game.infrastructure/Contexts/Matches/Data/MatchesRepository.cs`

**Implement Method:**
```csharp
public async Task<List<LiveMatchDto>> GetLiveMatchesAsync()
{
    using var connection = _connectionFactory.CreateConnection();
    var result = await connection.QueryAsync<LiveMatchDto>(MatchesQueries.SelectLiveMatches);
    return result.ToList();
}
```

### Phase 3: Orchestrator Implementation

#### 3.1 MatchCalculatorOrchestrator
**File:** `src/important-game.infrastructure/Contexts/ScoreCalculator/MatchCalculatorOrchestrator.cs`

**Key Implementation Points:**

1. **Dependencies:**
   - Inject `ISofaScoreIntegration`
   - Inject `IExternalProvidersRepository`

2. **Main Workflow:**
   ```csharp
   public async Task CalculateLiveScoreAsync(CancellationToken cancellationToken = default)
   {
       // 1. Get internal live matches
       var internalLiveMatches = await matchesRepository.GetLiveMatchesAsync();

       // 2. Get SofaScore live matches
       var sofascoreLive = await sofaScoreIntegration.GetAllLiveMatchesAsync();

       // 3. Map IDs (fuzzy matching)
       await MapLiveMatchIdsAsync(internalLiveMatches, sofascoreLive.Events);

       // 4. Get top 10 priority matches
       var priorityMatches = SelectPriorityMatches(internalLiveMatches, 10);

       // 5. Update each match with live data
       foreach (var match in priorityMatches)
       {
           await UpdateMatchWithLiveDataAsync(match, cancellationToken);
       }
   }
   ```

3. **ID Mapping Logic:**
   ```csharp
   private async Task MapLiveMatchIdsAsync(
       List<LiveMatchDto> internalMatches,
       IReadOnlyList<SSEvent> sofascoreMatches)
   {
       foreach (var ssMatch in sofascoreMatches)
       {
           // Check if already mapped
           var existingMapping = await externalProvidersRepository
               .GetExternalProviderMatchAsync(SOFASCORE_PROVIDER_ID, ssMatch.Id);

           if (existingMapping != null) continue;

           // Fuzzy match
           var match = internalMatches.FirstOrDefault(m =>
               FuzzyMatchTeams(m, ssMatch) &&
               MatchDates(m.StartTime, ssMatch.StartTimestamp));

           if (match != null)
           {
               // Persist mapping
               await externalProvidersRepository.SaveExternalProviderMatchAsync(
                   new ExternalProviderMatchesEntity
                   {
                       ProviderId = SOFASCORE_PROVIDER_ID,
                       InternalMatchId = match.MatchId,
                       ExternalMatchId = ssMatch.Id.ToString()
                   });
           }
       }
   }
   ```

4. **Fuzzy Matching:**
   ```csharp
   private bool FuzzyMatchTeams(LiveMatchDto internal, SSEvent sofascore)
   {
       var homeMatch = Fuzz.Ratio(
           NormalizeTeamName(internal.HomeTeamName),
           NormalizeTeamName(sofascore.HomeTeam.Name)) >= 85;

       var awayMatch = Fuzz.Ratio(
           NormalizeTeamName(internal.AwayTeamName),
           NormalizeTeamName(sofascore.AwayTeam.Name)) >= 85;

       return homeMatch && awayMatch;
   }

   private string NormalizeTeamName(string name)
   {
       return name.ToLowerInvariant()
           .Replace("fc", "").Replace("cf", "")
           .Replace("sc", "").Replace(".", "")
           .Trim();
   }
   ```

5. **Live Bonus Calculation:**
   ```csharp
   private int CalculateLiveBonus(SSEventStatistics stats)
   {
       int bonus = 0;

       foreach (var period in stats.Statistics)
       {
           foreach (var group in period.Groups)
           {
               foreach (var stat in group.StatisticsItems)
               {
                   switch (stat.Name)
                   {
                       case "Goals":
                           bonus += (int)(stat.HomeValue + stat.AwayValue) * 15;
                           break;
                       case "Yellow cards":
                           bonus += (int)(stat.HomeValue + stat.AwayValue) * 5;
                           break;
                       case "Red cards":
                           bonus += (int)(stat.HomeValue + stat.AwayValue) * 20;
                           break;
                       case "Total shots":
                           if (stat.HomeValue + stat.AwayValue > 20)
                               bonus += 10;
                           break;
                       case "Ball possession":
                           if (Math.Abs(stat.HomeValue - stat.AwayValue) < 10)
                               bonus += 8;
                           break;
                   }
               }
           }
       }

       return bonus;
   }
   ```

### Phase 4: Background Job

#### 4.1 LiveScoreCalculatorJob
**File:** `src/important-game.app/Services/LiveScoreCalculatorJob.cs`

**Implementation:**
```csharp
public class LiveScoreCalculatorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LiveScoreCalculatorJob> _logger;

    public LiveScoreCalculatorJob(
        IServiceProvider serviceProvider,
        ILogger<LiveScoreCalculatorJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run immediately on startup
        await RunJobAsync(stoppingToken).ConfigureAwait(false);

        // Then run every 10 minutes
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                await RunJobAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Live score calculator job is stopping.");
        }
    }

    private async Task RunJobAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var calculator = scope.ServiceProvider
                .GetRequiredService<IMatchCalculatorOrchestrator>();

            await calculator.CalculateLiveScoreAsync(stoppingToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Live score calculation was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate live match excitement scores.");
        }
    }
}
```

#### 4.2 Program.cs Registration
**File:** `src/important-game.app/Program.cs`

**Add Line:**
```csharp
services.AddHostedService<LiveScoreCalculatorJob>();
```

## Rate Limiting Strategy

### Request Budget (SofaScore Free Tier)

**Every 10 minutes:**
- 1 request: `GetAllLiveMatchesAsync()` - all live matches
- 10 requests: `GetEventStatisticsAsync(eventId)` - top 10 priority matches
- **Total: 11 requests per 10 minutes**

**Per Hour:**
- 6 cycles × 11 requests = **66 requests/hour**

**Per Day:**
- 24 hours × 66 requests = **1,584 requests/day**

⚠️ **This exceeds free tier limits!**

### Optimization Strategy

**Option 1: Reduce Frequency**
```
Run every 30 minutes instead of 10:
- 2 cycles/hour × 11 requests = 22 requests/hour
- 24 hours × 22 = 528 requests/day ✅
```

**Option 2: Reduce Match Count**
```
Run every 10 minutes with top 5 matches only:
- 6 cycles/hour × 6 requests = 36 requests/hour
- 24 hours × 36 = 864 requests/day ✅
```

**Option 3: Dynamic Scaling**
```
- If 0-5 live matches: Update all
- If 6-10 live matches: Update top 8
- If 11-20 live matches: Update top 5
- If 20+ live matches: Update top 3
```

**Recommended: Option 3 (Dynamic Scaling)**

## Priority Calculation

Matches are prioritized by:

1. **Base Excitement Score** (highest weight)
2. **League Tier** (Premier League > Championship)
3. **User Interest** (matches with user favorites)
4. **Recency** (newer live matches prioritized)

```csharp
private List<LiveMatchDto> SelectPriorityMatches(
    List<LiveMatchDto> matches, int maxCount)
{
    return matches
        .OrderByDescending(m => m.CurrentExcitementScore)
        .ThenByDescending(m => GetLeagueTier(m.CompetitionId))
        .ThenByDescending(m => m.StartTime)
        .Take(maxCount)
        .ToList();
}
```

## Error Handling

### Scenarios

1. **No Live Matches**
   - Log info message
   - Return early (no processing)

2. **SofaScore API Failure**
   - Retry with exponential backoff (built-in)
   - Log error
   - Continue to next cycle

3. **ID Mapping Failure**
   - Log warning with match details
   - Skip that match
   - Continue with others

4. **Database Error**
   - Log error with context
   - Rollback transaction (if applicable)
   - Don't throw (job continues)

## Logging Strategy

```csharp
// Start
_logger.LogInformation("Starting live score calculation. Found {Count} live matches", count);

// Mapping
_logger.LogInformation("Mapped {Mapped} matches, {Unmapped} unmapped", mapped, unmapped);

// Updates
_logger.LogDebug("Updated match {MatchId} with live bonus {Bonus}", matchId, bonus);

// Errors
_logger.LogWarning("Could not map match: {Home} vs {Away}", home, away);
_logger.LogError(ex, "Failed to get statistics for event {EventId}", eventId);

// Complete
_logger.LogInformation("Live score calculation complete. Updated {Count} matches", count);
```

## Testing Strategy

### Unit Tests

1. **Fuzzy Matching Logic**
   - Test team name normalization
   - Test similarity threshold (85%)
   - Test date matching (±2 hours)

2. **Live Bonus Calculation**
   - Test each statistic type
   - Test combined bonuses
   - Test edge cases (null values)

3. **Priority Selection**
   - Test ordering logic
   - Test max count limiting

### Integration Tests

1. **Repository Queries**
   - Test GetLiveMatchesAsync with sample data
   - Verify JOIN correctness

2. **Orchestrator Workflow**
   - Mock SofaScore responses
   - Verify mapping persistence
   - Check update execution

### Manual Testing

1. **Run during live matches** (Saturday afternoon)
2. **Monitor logs** for errors/warnings
3. **Verify database updates** in real-time
4. **Check rate limiting** (no 429 errors)

## Monitoring & Metrics

### Key Metrics

- Live matches found per cycle
- Successful ID mappings
- Failed ID mappings (warnings)
- API request count per cycle
- Average processing time
- Error rate

### Alerts

- SofaScore API failures (3+ consecutive)
- Zero matches updated (5+ consecutive cycles)
- High error rate (>25% failures)
- Rate limit violations (429 errors)

## Configuration

### appsettings.json

```json
{
  "LiveScoreCalculator": {
    "Enabled": true,
    "IntervalMinutes": 10,
    "MaxMatchesPerCycle": 10,
    "FuzzyMatchThreshold": 85,
    "DateToleranceHours": 2,
    "SofaScoreProviderId": 1
  }
}
```

## Rollout Plan

### Phase 1: Development (Week 1)
- [ ] Implement interface methods
- [ ] Implement repository queries
- [ ] Implement orchestrator logic
- [ ] Create background job
- [ ] Unit tests

### Phase 2: Testing (Week 2)
- [ ] Integration tests
- [ ] Manual testing with real matches
- [ ] Performance testing
- [ ] Rate limit verification

### Phase 3: Deployment (Week 3)
- [ ] Deploy to staging
- [ ] Monitor for 3 days
- [ ] Fix any issues
- [ ] Deploy to production

### Phase 4: Optimization (Week 4)
- [ ] Tune priority algorithm
- [ ] Optimize request count
- [ ] Refine fuzzy matching
- [ ] Add user feedback

## Success Criteria

✅ **Functional**
- Live matches update every 10 minutes
- ID mapping success rate >90%
- Excitement scores reflect live events
- No duplicate mappings created

✅ **Performance**
- Job completes in <30 seconds
- No rate limit violations
- Cache hit rate >70%

✅ **Reliability**
- Error rate <5%
- Graceful degradation on failures
- No job crashes

✅ **User Experience**
- Live scores visible on UI
- Updates feel responsive
- Accurate excitement rankings

## Future Enhancements

1. **Real-Time Updates (SignalR)**
   - Push updates to connected clients
   - Show live indicator on match cards

2. **Historical Tracking**
   - Store excitement score timeline
   - Show graph of excitement over match time

3. **Event Detection**
   - Detect goals/cards in real-time
   - Send push notifications

4. **Multi-Provider Support**
   - Add Football-Data.org for premium leagues
   - Fallback logic for redundancy

---

**Document Version:** 1.0
**Created:** 2026-01-11
**Author:** Claude + Ricardo
**Status:** Ready for Implementation
