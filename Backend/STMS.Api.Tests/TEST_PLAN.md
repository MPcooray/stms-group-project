# STMS Testing Plan - High Priority Tests

## Overview
This document outlines comprehensive tests for the STMS project based on the **actual** model structure.

## Actual Model Structure

### Models (as they exist in your code):
- **Tournament**: Id, Name, Venue, Date, EndDate?, CreatedAt
- **University**: Id, Name, TournamentId, CreatedAt
- **Player**: Id, Name, UniversityId, Age?, Gender?, CreatedAt
- **TournamentEvent**: Id, TournamentId, Name, CreatedAt
- **Timing**: Id, PlayerId, EventId, TimeMs, CreatedAt
- **PlayerEvents**: (Junction table for Player-Event relationships)

---

## ✅ Tests Already Implemented

You already have these 14 test files:
1. ✅ AdminTimingEntryTests.cs
2. ✅ CascadingDeletionTests.cs
3. ✅ DatabaseRelationshipConsistencyTests.cs
4. ✅ EventResultsGenderFilterTests.cs
5. ✅ LeaderboardAccuracyTests.cs
6. ✅ LeaderboardCategoryAccuracyTests.cs
7. ✅ LeaderboardFilteringTests.cs
8. ✅ LeaderboardGenderFilterTests.cs
9. ✅ PlayerUpdateDeleteTests.cs
10. ✅ PointUpdateLogicTests.cs
11. ✅ RankAssignmentLogicTests.cs
12. ✅ RankDatabaseOrderTests.cs
13. ✅ TournamentUITests.cs
14. ✅ UniversityUpdateDeleteTests.cs

---

## 🆕 High Priority Tests to Add

### 1. **Leaderboard Controller Tests** (Enhanced)

#### Test File: `LeaderboardControllerEnhancedTests.cs`

**Point Calculation Tests:**
- ✓ Test rank 1-8 get correct points (10, 8, 7, 5, 4, 3, 2, 1)
- ✓ Test rank 9+ get 0 points
- ✓ Test negative/invalid times are excluded

**Tie Handling:**
- ✓ Test 2 players with same time get same rank
- ✓ Test next rank skips correctly after tie (e.g., 1, 1, 3)
- ✓ Test 3+ players tied for same position
- ✓ Test ties in multiple events
- ✓ Test university points aggregate correctly with ties

**Multi-Event Aggregation:**
- ✓ Test player points accumulate across 2+ events
- ✓ Test player can score in some but not all events
- ✓ Test player DNF (TimeMs=0) is excluded from scoring

**Player/University Exclusion:**
- ✓ Test players without timings don't appear in leaderboard
- ✓ Test universities with no player points don't appear

**Gender Filtering:**
- ✓ Test `?gender=male` filters only male players
- ✓ Test `?gender=female` filters only female players
- ✓ Test `?gender=Male` (case-insensitive)
- ✓ Test `?gender=m` and `?gender=f` shortcuts
- ✓ Test `?gender=invalid` returns 400 Bad Request
- ✓ Test gender filter affects university totals correctly

**Edge Cases:**
- ✓ Test tournament with no events returns empty leaderboard
- ✓ Test tournament with events but no timings returns empty
- ✓ Test all players have same points (all tied for 1st)

---

### 2. **Timings Controller Tests**

#### Test File: `TimingsControllerEnhancedTests.cs`

**GET /api/timings/{playerId}/{eventId}:**
- ✓ Test returns timing for valid player-event pair
- ✓ Test returns 404 if timing doesn't exist
- ✓ Test returns correct TimeMs value
- ✓ Test returns correct CreatedAt timestamp

**POST /api/timings (Create):**
- ✓ Test creates new timing successfully
- ✓ Test validates PlayerId exists (foreign key)
- ✓ Test validates EventId exists (foreign key)
- ✓ Test accepts TimeMs > 0
- ✓ Test accepts TimeMs = 0 (DNF scenario)
- ✓ Test sets CreatedAt timestamp automatically

**POST /api/timings (Update - Upsert):**
- ✓ Test updates existing timing (same PlayerId + EventId)
- ✓ Test keeps same ID when updating
- ✓ Test only one timing exists per player-event pair
- ✓ Test CreatedAt doesn't change on update
- ✓ Test TimeMs is updated correctly

**Multiple Timings:**
- ✓ Test different players can have timings for same event
- ✓ Test same player can have timings for different events
- ✓ Test cannot create duplicate player-event pairs

---

### 3. **Tie Breaking Logic Tests**

#### Test File: `TieBreakingLogicTests.cs`

**Basic Ties:**
- ✓ Test 2 players tied for 1st → both get rank 1, 10 points each
- ✓ Test next player after tie gets rank 3 (not rank 2)
- ✓ Test 3 players tied → all get same rank and points

**Complex Tie Scenarios:**
- ✓ Test multiple tie groups in one event (1,1, 3,3,3, 6)
- ✓ Test ties at different ranks (2nd place tie, 5th place tie)
- ✓ Test all players tied (everyone rank 1)

**Multi-Event Ties:**
- ✓ Test ties in Event 1, different results in Event 2
- ✓ Test overall leaderboard ties after aggregating multiple events
- ✓ Test tie-breaking is independent per event

**University Ties:**
- ✓ Test 2 universities with same total points → both rank 1
- ✓ Test 3+ universities tied
- ✓ Test next rank skips correctly after university tie

---

### 4. **Performance Tests**

#### Test File: `LeaderboardPerformanceTests.cs`

**Scale Tests:**
- ✓ Test 50 players, 5 events < 500ms
- ✓ Test 100 players, 5 events < 1000ms
- ✓ Test 500 players, 5 events < 2000ms
- ✓ Test 100 players, 10 events < 1000ms
- ✓ Test 100 players, 15 events < 1500ms

**Stress Tests:**
- ✓ Test 500 players, 10 events < 3000ms
- ✓ Test 50 universities, 500 total players < 3000ms

**Consistency Tests:**
- ✓ Test 10 consecutive queries have consistent performance (low std dev)
- ✓ Test average execution time < 1 second
- ✓ Test gender filtering doesn't significantly slow down queries

---

## 📝 Test Implementation Notes

### Important Considerations:

1. **Use InMemory Database**: All tests use `UseInMemoryDatabase()` for fast, isolated testing
2. **Fresh Database Per Test**: Each test gets a new GUID database name to avoid conflicts
3. **Dispose Properly**: Implement `IDisposable` to clean up databases after each test
4. **No Mocking Needed**: Controllers use real DbContext (InMemory) - no Moq required

### Example Test Pattern:

```csharp
public class MyControllerTests : IDisposable
{
    private readonly StmsDbContext _context;
    private readonly MyController _controller;

    public MyControllerTests()
    {
        var options = new DbContextOptionsBuilder<StmsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StmsDbContext(options);
        _controller = new MyController(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task TestName_Scenario_ExpectedResult()
    {
        // Arrange
        var tournament = new Tournament { Name = "Test", Venue = "Venue", Date = DateTime.Today };
        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SomeAction(tournament.Id);

        // Assert
        Assert.NotNull(result);
    }
}
```

---

## 🎯 Current Test Status

### Completed:
- 14 existing test files covering basics

### In Progress (High Priority):
1. ⏳ Leaderboard Controller Enhanced Tests
2. ⏳ Timings Controller Tests
3. ⏳ Tie Breaking Logic Tests
4. ⏳ Performance Tests

### Future (Medium Priority):
- Controller Integration Tests (Players, Tournaments, Universities)
- Model Validation Tests
- Business Logic Tests
- Edge Case Tests

---

## 🚀 How to Run Tests

```cmd
# Run all tests
cd Backend\STMS.Api.Tests
dotnet test

# Run specific test file
dotnet test --filter "FullyQualifiedName~LeaderboardControllerEnhancedTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📊 Test Coverage Goals

- **Controllers**: 80%+ coverage
- **Business Logic** (Leaderboard, Points, Ranking): 95%+ coverage
- **Critical Paths**: 100% coverage
- **Edge Cases**: Well documented and tested

---

## Next Steps

1. Implement the 4 high-priority test files listed above
2. Run all tests to ensure they pass
3. Check test coverage with `dotnet test --collect:"XPlat Code Coverage"`
4. Add integration tests for remaining controllers
5. Document any discovered bugs or edge cases

---

**Note**: All tests should be written to match the ACTUAL models in your codebase. Do not assume properties like `Category`, `Gender`, or `Order` exist on `TournamentEvent` unless they are actually in the model!
