# Quick Test Commands Reference

## Run Tests

```cmd
# Navigate to test project
cd Backend\STMS.Api.Tests

# Run ALL tests
dotnet test

# Run tests (skip UI tests that need servers)
dotnet test --filter "FullyQualifiedName!~TournamentUITests"

# Run specific test file
dotnet test --filter "FullyQualifiedName~LeaderboardAccuracyTests"

# Run specific test method
dotnet test --filter "TestMethodName"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Test Results Summary

### ✅ Current Status: 23/26 Tests Passing

**Passing Tests (23)**:
- Leaderboard tests (accuracy, filtering, gender, category, ranks)
- CRUD tests (players, universities)  
- Database integrity tests (cascading, relationships)
- Business logic tests (points, ranks)

**Failing Tests (3)** - Expected (need running servers):
- TournamentUITests (Selenium E2E tests)

---

## What to Test Next (Priority Order)

### 1. ⭐⭐⭐ Enhanced Leaderboard Tests
**File**: `LeaderboardControllerEnhancedTests.cs`
- Point calculation (10, 8, 7, 5, 4, 3, 2, 1, 0)
- Tie handling and rank skipping
- Multi-event aggregation
- Gender filtering
- Edge cases

### 2. ⭐⭐ Timings Controller Tests  
**File**: `TimingsControllerTests.cs`
- GET /api/timings/{playerId}/{eventId}
- POST /api/timings (create & update)
- Validation and timestamps

### 3. ⭐⭐ Tie Breaking Tests
**File**: `TieBreakingLogicTests.cs`
- 2-player ties, 3+ player ties
- Multiple tie groups
- University ties

### 4. ⭐ Performance Tests
**File**: `LeaderboardPerformanceTests.cs`
- 50, 100, 500 players
- 10+ events
- Query execution < 1 second

---

## Your Actual Models (DON'T FORGET!)

```csharp
// Tournament - CORRECT ✅
Tournament { Id, Name, Venue, Date, EndDate, CreatedAt }

// TournamentEvent - CORRECT ✅ (NO Category/Gender/Order!)
TournamentEvent { Id, TournamentId, Name, CreatedAt }

// Player - CORRECT ✅
Player { Id, Name, UniversityId, Age, Gender, CreatedAt }

// Timing - CORRECT ✅
Timing { Id, PlayerId, EventId, TimeMs, CreatedAt }

// University - CORRECT ✅
University { Id, Name, TournamentId, CreatedAt }
```

---

## Test Template

```csharp
public class MyTests : IDisposable
{
    private readonly StmsDbContext _context;
    private readonly MyController _controller;

    public MyTests()
    {
        var options = new DbContextOptionsBuilder<StmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
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
    public async Task Test_Scenario_Expected()
    {
        // Arrange
        var data = new Model { ... };
        _context.Add(data);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Action();

        // Assert
        Assert.NotNull(result);
    }
}
```

---

## Useful xUnit Assertions

```csharp
// Equality
Assert.Equal(expected, actual);
Assert.NotEqual(expected, actual);

// Nullability
Assert.Null(value);
Assert.NotNull(value);

// Boolean
Assert.True(condition);
Assert.False(condition);

// Collections
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Single(collection);
Assert.Contains(item, collection);
Assert.All(collection, item => Assert.True(...));

// Types
Assert.IsType<T>(obj);
Assert.IsAssignableFrom<T>(obj);

// Ranges
Assert.InRange(actual, low, high);

// Exceptions
Assert.Throws<Exception>(() => method());
await Assert.ThrowsAsync<Exception>(async () => await method());
```

---

## Documentation

📄 **TEST_PLAN.md** - Detailed test specifications
📄 **TESTING_SUMMARY.md** - Complete guide and next steps  
📄 **QUICK_REFERENCE.md** - This file!

---

**Ready to write tests? Start with Enhanced Leaderboard Tests! 🚀**
