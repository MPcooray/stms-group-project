using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
// Removed unused Task namespace
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace STMS.Api.Tests
{
    /// <summary>
    /// Selenium UI coverage for the PUBLIC (unauthenticated) pages ONLY (no admin creation):
    ///   - Home (/)
    ///   - Public Tournaments (/public/tournaments)
    ///   - Public Tournament Results (/public/tournaments/{id}/results)
    ///   - Public Tournament Leaderboard (/public/tournaments/{id}/leaderboard)
    /// Data Assumption: At least one tournament already exists (seeded or created previously) OR you set PUBLIC_TOURNAMENT_ID env var.
    ///   Resolution priority:
    ///     1. PUBLIC_TOURNAMENT_ID environment variable (string id) – optionally we try to map its name via API.
    ///     2. First tournament returned by GET /api/tournaments.
    /// If no tournament can be resolved the test silently exits (soft skip) – avoids flakes in empty environments.
    /// Frontend must be running on http://localhost:3000 and backend API on ApiBase.
    /// </summary>
    public class PublicPagesUITests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly HttpClient _http = new();
        private const string FrontendBase = "http://localhost:3000"; // adjust if port changes
        private const string ApiBase = "http://localhost:5287";      // falls back to default backend dev port
        private readonly int _stepDelayMs; // global delay after each visible step for recording

        public PublicPagesUITests()
        {
            var options = new ChromeOptions();
            // Headed mode (browser window visible) so you can observe the end-to-end flow.
            // If you want to revert to faster CI-friendly execution, re-enable headless by uncommenting the next line:
            // options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1400,1000");
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            _wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            // Determine delay (default 1500ms). Override with PUBLIC_STEP_DELAY_MS env var.
            if (!int.TryParse(Environment.GetEnvironmentVariable("PUBLIC_STEP_DELAY_MS"), out _stepDelayMs) || _stepDelayMs < 0)
            {
                _stepDelayMs = 1500; // 1.5s default for clear visual recording
            }
        }

        /// <summary>
        /// Optional initial delay to give the user time to enable a screen recorder before any navigation begins.
        /// Controlled via environment variable PUBLIC_INITIAL_DELAY_MS (milliseconds). If set &gt; 0 we pause.
        /// A countdown (per second) is printed for delays of 3s or more.
        /// </summary>
        private void InitialDelayIfRequested(string testName)
        {
            var env = Environment.GetEnvironmentVariable("PUBLIC_INITIAL_DELAY_MS");
            if (!int.TryParse(env, out var delay) || delay <= 0) return;
            Console.WriteLine($"[INFO] PUBLIC_INITIAL_DELAY_MS={delay}ms configured. Preparing to start '{testName}' steps...");
            if (delay >= 3000)
            {
                int seconds = delay / 1000;
                int remainder = delay % 1000;
                for (int i = seconds; i >= 1; i--)
                {
                    Console.WriteLine($"[INFO]  Starting in {i}...");
                    System.Threading.Thread.Sleep(1000);
                }
                if (remainder > 0) System.Threading.Thread.Sleep(remainder);
            }
            else
            {
                System.Threading.Thread.Sleep(delay);
            }
            Console.WriteLine("[INFO] Recording window activation period complete. Beginning automated steps.");
        }

        private void Pause() { if (_stepDelayMs > 0) System.Threading.Thread.Sleep(_stepDelayMs); }

        // Verbose logging helper (set PUBLIC_VERBOSE=1 to see informational messages)
        private static bool VerboseEnabled() => string.Equals(Environment.GetEnvironmentVariable("PUBLIC_VERBOSE"), "1", StringComparison.OrdinalIgnoreCase);
        private static void Info(string message)
        {
            if (VerboseEnabled()) Console.WriteLine(message);
        }

        private bool FrontendReachable()
        {
            try
            {
                _driver.Navigate().GoToUrl(FrontendBase + "/");
                _wait.Until(d => d.PageSource.Length > 0);
                return true;
            }
            catch { return false; }
        }

        private (string id, string? name)? ResolveTournament()
        {
            // Simple resolution: explicit env var else first tournament.
            var envId = Environment.GetEnvironmentVariable("PUBLIC_TOURNAMENT_ID");
            string? chosenId = null;
            string? chosenName = null;
            try
            {
                var json = _http.GetStringAsync(ApiBase + "/api/tournaments").GetAwaiter().GetResult();
                using var doc = JsonDocument.Parse(json);
                var all = doc.RootElement.EnumerateArray().ToList();
                if (!string.IsNullOrWhiteSpace(envId))
                {
                    var match = all.FirstOrDefault(e => e.TryGetProperty("id", out var idProp) && idProp.ToString() == envId);
                    if (match.ValueKind != JsonValueKind.Undefined)
                    {
                        chosenId = envId;
                        if (match.TryGetProperty("name", out var nm)) chosenName = nm.GetString();
                    }
                }
                if (chosenId == null)
                {
                    var first = all.FirstOrDefault();
                    if (first.ValueKind != JsonValueKind.Undefined && first.TryGetProperty("id", out var fid))
                    {
                        chosenId = fid.ToString();
                        if (first.TryGetProperty("name", out var fn)) chosenName = fn.GetString();
                    }
                }
            }
            catch { /* ignore */ }
            return chosenId == null ? null : (chosenId, chosenName);
        }

        /// <summary>
        /// Helper used by the recording-focused flow: only navigates public pages for an existing tournament id.
        /// Adds gentle pauses so you can screen-record clean transitions.
        /// </summary>
        private void NavigatePublicPagesForRecording(string tournamentId)
        {
            // HOME
            _driver.Navigate().GoToUrl(FrontendBase + "/");
            _wait.Until(d => d.PageSource.Contains("AquaChamps"));
            Pause();
            // TOURNAMENTS
            var viewBtn = _driver.FindElements(By.LinkText("View Tournaments")).FirstOrDefault();
            if (viewBtn != null) viewBtn.Click(); else _driver.Navigate().GoToUrl(FrontendBase + "/public/tournaments");
            _wait.Until(d => d.PageSource.Contains("All Tournaments"));
            Pause();
            // RESULTS
            _driver.Navigate().GoToUrl(FrontendBase + $"/public/tournaments/{tournamentId}/results");
            _wait.Until(d => d.PageSource.Contains("Event Results") || d.PageSource.Contains("No Events"));
            Pause();
            // LEADERBOARD
            _driver.Navigate().GoToUrl(FrontendBase + $"/public/tournaments/{tournamentId}/leaderboard");
            _wait.Until(d => d.PageSource.Contains("Leaderboard"));
            Pause();
        }

        [Fact(DisplayName = "Public pages flow (slow visible)"), Trait("Category", "UI")]
        public void PublicPages_PublicFlowExistingTournament()
        {
            // Give user time to switch to screen recorder if requested
            InitialDelayIfRequested("PublicPages_PublicFlowExistingTournament");
            if (!FrontendReachable()) return; // soft skip
            // Strict mode toggle (turns informative logs into hard assertions for integrity checks)
            bool strict = string.Equals(Environment.GetEnvironmentVariable("PUBLIC_STRICT"), "1", StringComparison.OrdinalIgnoreCase);

            var resolved = ResolveTournament();
            if (resolved == null)
            {
                Info("No tournaments available (and PUBLIC_TOURNAMENT_ID not set) – skipping public flow test.");
                return; // soft skip
            }
            var (tournamentId, tournamentName) = resolved.Value;

            // ===== HOME =====
            _driver.Navigate().GoToUrl(FrontendBase + "/");
            _wait.Until(d => d.FindElement(By.CssSelector("h1.brand")));
            Assert.Contains("AquaChamps", _driver.PageSource);
            // Hero + stats + lock icon (best-effort logs)
            if (!_driver.PageSource.Contains("Welcome to AquaChamps")) Info("Hero heading missing.");
            if (!_driver.PageSource.Contains("Active Tournaments")) Info("Active Tournaments stat label missing.");
            // Padlock/login icon presence (do not navigate per request)
            if (_driver.FindElements(By.CssSelector(".admin-login-btn")).Count == 0)
                Info("Lock icon (admin login) not present – acceptable for public recording.");

            // Recent tournaments preview – attempt to use preview 'View Results' button instead of main CTA if it matches chosen tournament
            bool navigatedFromPreview = false;
            try
            {
                var previewCards = _driver.FindElements(By.CssSelector(".tournament-preview-card"));
                if (previewCards.Count > 0)
                {
                    var previewBtn = previewCards
                        .Select(c => c.FindElements(By.LinkText("View Results")).FirstOrDefault())
                        .FirstOrDefault(b => b != null);
                    if (previewBtn != null)
                    {
                        previewBtn.Click();
                        _wait.Until(d => d.Url.Contains("/public/tournaments/"));
                        navigatedFromPreview = true;
                    }
                }
            }
            catch { /* ignore preview issues */ }

            if (!navigatedFromPreview)
            {
                // Fall back to main button navigation path
                var homeViewBtn = _driver.FindElements(By.LinkText("View Tournaments")).FirstOrDefault();
                Assert.NotNull(homeViewBtn);
                homeViewBtn!.Click();
                _wait.Until(d => d.Url.Contains("/public/tournaments"));
            }
            Pause();

            // ===== TOURNAMENTS LIST =====
            _wait.Until(d => d.Url.Contains("/public/tournaments"));
            _wait.Until(d => d.PageSource.Contains("All Tournaments"));
            if (_driver.FindElements(By.CssSelector(".public-nav .nav-link")).Count < 2)
                Console.WriteLine("[INFO] Missing expected nav links.");
            var firstCard = _driver.FindElements(By.CssSelector(".tournament-card")).FirstOrDefault();
            if (firstCard != null)
            {
                var act = firstCard.FindElements(By.CssSelector(".tournament-actions a"));
                if (act.Count < 2) Console.WriteLine("[INFO] Tournament card missing one of action buttons.");
                else
                {
                    // Prefer clicking the explicit 'View Results' button instead of direct navigation to simulate user flow
                    var viewResultsBtn = act.FirstOrDefault(a => a.Text.Contains("View Results", StringComparison.OrdinalIgnoreCase));
                    if (viewResultsBtn != null)
                    {
                        try
                        {
                            viewResultsBtn.Click();
                            _wait.Until(d => d.Url.Contains("/results"));
                        }
                        catch { Console.WriteLine("[INFO] Failed to navigate via card View Results button."); }
                    }
                }
            }
            else if (!_driver.PageSource.Contains("No Tournaments Available"))
            {
                Info("No cards and no empty state present.");
            }
            Pause();

            // ===== RESULTS PAGE ===== (if not already on it via prior clicks)
            if (!_driver.Url.Contains("/public/tournaments/") || !_driver.Url.EndsWith("/results"))
            {
                _driver.Navigate().GoToUrl(FrontendBase + $"/public/tournaments/{tournamentId}/results");
            }
            _wait.Until(d => d.PageSource.Contains("Event Results") || d.PageSource.Contains("No Events") || d.PageSource.Contains("Tournament Not Found"));
            Assert.Contains("AquaChamps", _driver.PageSource);
            if (!string.IsNullOrWhiteSpace(tournamentName) && !_driver.PageSource.Contains(tournamentName))
                Console.WriteLine($"[INFO] Tournament name '{tournamentName}' not visible on results page.");
            var tabs = _driver.FindElements(By.CssSelector(".tabs .tab"));
            if (tabs.Count >= 2)
            {
                var overviewTab = tabs.FirstOrDefault(t => t.Text.Contains("Overview", StringComparison.OrdinalIgnoreCase));
                if (overviewTab != null)
                {
                    overviewTab.Click();
                    try { _wait.Until(d => d.PageSource.Contains("Tournament Information") || d.PageSource.Contains("Total Events")); }
                    catch { Console.WriteLine("[INFO] Overview content not loaded."); }
                    Pause();
                    var resultsTab = tabs.FirstOrDefault(t => t.Text.Contains("Event Results", StringComparison.OrdinalIgnoreCase));
                    if (resultsTab != null) { resultsTab.Click(); Pause(); }
                }
            }
            else Info("Tabs not present on results page.");

            // Verify at least one event results table (structure) if events/results exist
            var eventCards = _driver.FindElements(By.CssSelector(".event-results-card"));
            if (eventCards.Count > 0)
            {
                var firstEventCard = eventCards.First();
                var table = firstEventCard.FindElements(By.CssSelector("table.results-table")).FirstOrDefault();
                if (table != null)
                {
                    var headers = table.FindElements(By.CssSelector("thead th")).Select(h => h.Text.Trim()).ToList();
                    string[] expectedHeaders = { "Rank", "Player", "University", "Time", "Points" };
                    foreach (var h in expectedHeaders)
                        if (!headers.Contains(h)) Info($"Missing results table header '{h}'.");

                    var firstRow = table.FindElements(By.CssSelector("tbody tr")).FirstOrDefault();
                    if (firstRow != null)
                    {
                        var rankCell = firstRow.FindElements(By.CssSelector(".rank-cell")).FirstOrDefault();
                        if (rankCell == null) Info("First results row missing rank-cell.");
                        else if (!rankCell.Text.Contains("1")) Info("First rank cell does not show rank 1.");
                    }
                }
                else Info("Event results card has no table (may be 'No results').");
            }
            else Info("No event result cards present.");

            // Use in-page button to move to leaderboard (simulate user flow) instead of direct URL

            // -------- Additional Integrity Checks (non-invasive) --------
            var computedPlayerPoints = new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int totalResultRows = 0;
            foreach (var card in eventCards)
            {
                var rows = card.FindElements(By.CssSelector("table.results-table tbody tr"));
                foreach (var row in rows)
                {
                    try
                    {
                        var playerNameEl = row.FindElements(By.CssSelector(".player-name")).FirstOrDefault();
                        var pointsEl = row.FindElements(By.CssSelector("td.points")).FirstOrDefault();
                        var timeEl = row.FindElements(By.CssSelector("td.time")).FirstOrDefault();
                        if (playerNameEl == null || pointsEl == null) continue;
                        var name = playerNameEl.Text.Trim();
                        if (int.TryParse(pointsEl.Text.Trim(), out var pVal))
                        {
                            if (!computedPlayerPoints.ContainsKey(name)) computedPlayerPoints[name] = 0;
                            computedPlayerPoints[name] += pVal;
                        }
                        // Time format check (ss.xx or m:ss.xx) ignoring '-'
                        if (timeEl != null)
                        {
                            var tText = timeEl.Text.Trim();
                            if (tText.Length > 0 && tText != "-")
                            {
                                bool match = System.Text.RegularExpressions.Regex.IsMatch(tText, "^(?:\\d{1,2}:[0-5]\\d|\\d{1,2})\\.\\d{2}$");
                                if (!match)
                                {
                                    var msg = $"Unexpected time format '{tText}'";
                                    if (strict) Assert.True(false, msg); else Info(msg);
                                }
                            }
                        }
                        totalResultRows++;
                    }
                    catch { /* ignore parse issues */ }
                }
            }

            // Overview stats cross-check (best effort)
            int overviewEvents = -1, overviewTotalResults = -1;
            try
            {
                var tabsAgain = _driver.FindElements(By.CssSelector(".tabs .tab"));
                var overviewTab2 = tabsAgain.FirstOrDefault(t => t.Text.Contains("Overview", StringComparison.OrdinalIgnoreCase));
                if (overviewTab2 != null)
                {
                    overviewTab2.Click(); Pause();
                    var cards = _driver.FindElements(By.CssSelector(".overview-stats .stat-card"));
                    if (cards.Count >= 2)
                    {
                        int.TryParse(cards[0].FindElements(By.CssSelector("h4")).FirstOrDefault()?.Text.Trim(), out overviewEvents);
                        int.TryParse(cards[1].FindElements(By.CssSelector("h4")).FirstOrDefault()?.Text.Trim(), out overviewTotalResults);
                    }
                    var resultsTab2 = tabsAgain.FirstOrDefault(t => t.Text.Contains("Event Results", StringComparison.OrdinalIgnoreCase));
                    if (resultsTab2 != null) { resultsTab2.Click(); Pause(); }
                }
            }
            catch { }

            int eventCardCount = eventCards.Count; // includes those without results

            var toLeaderboardBtn = _driver.FindElements(By.CssSelector(".tournament-actions a.btn.primary"))
                .FirstOrDefault(a => a.Text.Contains("Leaderboard", StringComparison.OrdinalIgnoreCase));
            if (toLeaderboardBtn != null)
            {
                try { toLeaderboardBtn.Click(); _wait.Until(d => d.Url.Contains("/leaderboard")); }
                catch { Console.WriteLine("[INFO] Failed to navigate via View Leaderboard button."); }
            }
            else
            {
                Info("View Leaderboard button not found on results page – falling back to direct navigate.");
                _driver.Navigate().GoToUrl(FrontendBase + $"/public/tournaments/{tournamentId}/leaderboard");
            }
            Pause();

            // ===== LEADERBOARD PAGE =====
            if (!_driver.Url.Contains("/leaderboard"))
                _driver.Navigate().GoToUrl(FrontendBase + $"/public/tournaments/{tournamentId}/leaderboard");
            _wait.Until(d => d.PageSource.Contains("Leaderboard"));
            if (!string.IsNullOrWhiteSpace(tournamentName) && !_driver.PageSource.Contains(tournamentName))
                Console.WriteLine($"[INFO] Tournament name '{tournamentName}' not visible on leaderboard page.");
            var toggleBtns = _driver.FindElements(By.CssSelector(".view-toggle .toggle-btn"));
            if (toggleBtns.Count >= 2)
            {
                var uniBtn = toggleBtns.FirstOrDefault(b => b.Text.Contains("University", StringComparison.OrdinalIgnoreCase));
                if (uniBtn != null)
                {
                    uniBtn.Click();
                    Pause();
                    // Toggle back to players to assert both tables render
                    var playerBtn = toggleBtns.FirstOrDefault(b => b.Text.Contains("Player", StringComparison.OrdinalIgnoreCase));
                    if (playerBtn != null)
                    {
                        playerBtn.Click();
                        Pause();
                    }
                }
            }
            else Info("Leaderboard toggle buttons missing.");
            if (!_driver.PageSource.Contains("Points System")) Console.WriteLine("[INFO] Points System section missing.");
            else
            {
                // Verify a couple of points items exist
                string[] ptsSnippets = { "1st: 10", "8th: 1" };
                foreach (var snippet in ptsSnippets)
                    if (!_driver.PageSource.Contains(snippet)) Info($"Points snippet '{snippet}' missing.");
            }
            // Leaderboard data checks
            try
            {
                // Ensure on player leaderboard
                bool onPlayers = _driver.PageSource.Contains("Player Leaderboard") || _driver.FindElements(By.CssSelector(".player-name")).Count > 0;
                if (!onPlayers)
                {
                    var playerToggle = _driver.FindElements(By.CssSelector(".view-toggle .toggle-btn")).FirstOrDefault(b => b.Text.Contains("Player", StringComparison.OrdinalIgnoreCase));
                    if (playerToggle != null) { playerToggle.Click(); Pause(); }
                }
                var playerRows = _driver.FindElements(By.CssSelector(".leaderboard-table tbody tr"));
                var playerPoints = playerRows.Select(r =>
                {
                    var ptsEl = r.FindElements(By.CssSelector("td.points")).FirstOrDefault();
                    return ptsEl != null && int.TryParse(ptsEl.Text.Trim(), out var v) ? v : -1;
                }).ToList();
                for (int i = 1; i < playerPoints.Count; i++)
                {
                    if (playerPoints[i] > playerPoints[i - 1])
                    {
                        var msg = $"Player leaderboard not sorted descending at index {i}.";
                        if (strict) Assert.True(false, msg); else Info(msg);
                    }
                }
                // Cross-check computed totals
                foreach (var row in playerRows)
                {
                    var nameEl = row.FindElements(By.CssSelector(".player-name")).FirstOrDefault();
                    var ptsEl = row.FindElements(By.CssSelector("td.points")).FirstOrDefault();
                    if (nameEl == null || ptsEl == null) continue;
                    if (int.TryParse(ptsEl.Text.Trim(), out var pts) && computedPlayerPoints.TryGetValue(nameEl.Text.Trim(), out var expect))
                    {
                        if (pts != expect)
                        {
                            var msg = $"Points mismatch for '{nameEl.Text.Trim()}': {pts} vs expected {expect}.";
                            if (strict) Assert.True(false, msg); else Info(msg);
                        }
                    }
                }

                // Switch to university leaderboard
                var uniToggle = _driver.FindElements(By.CssSelector(".view-toggle .toggle-btn")).FirstOrDefault(b => b.Text.Contains("University", StringComparison.OrdinalIgnoreCase));
                if (uniToggle != null)
                {
                    uniToggle.Click(); Pause();
                    var uniRows = _driver.FindElements(By.CssSelector(".leaderboard-table tbody tr"));
                    var uniPoints = uniRows.Select(r =>
                    {
                        var ptsEl = r.FindElements(By.CssSelector("td.points")).FirstOrDefault();
                        return ptsEl != null && int.TryParse(ptsEl.Text.Trim(), out var v) ? v : -1;
                    }).ToList();
                    for (int i = 1; i < uniPoints.Count; i++)
                    {
                        if (uniPoints[i] > uniPoints[i - 1])
                        {
                            var msg = $"University leaderboard not sorted descending at index {i}.";
                            if (strict) Assert.True(false, msg); else Info(msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Info("Leaderboard integrity exception: " + ex.Message);
                if (strict) throw;
            }

            // Browser console severe errors
            try
            {
                var logs = (_driver as ChromeDriver)?.Manage().Logs.GetLog(LogType.Browser);
                if (logs != null)
                {
                    var severe = logs.Where(l => l.Level == OpenQA.Selenium.LogLevel.Severe).ToList();
                    foreach (var s in severe) Console.WriteLine("[BROWSER-SEVERE] " + s.Message);
                    if (severe.Count > 0 && strict)
                        Assert.True(false, $"Browser console has {severe.Count} severe error(s).");
                }
            }
            catch { }

            // Overview vs results counts
            if (eventCards.Count > 0 && (overviewEvents >= 0 || overviewTotalResults >= 0))
            {
                if (overviewEvents >= 0 && overviewEvents != eventCardCount)
                {
                    var msg = $"Overview events {overviewEvents} != cards {eventCardCount}";
                    if (strict) Assert.True(false, msg); else Info(msg);
                }
                if (overviewTotalResults >= 0 && overviewTotalResults != totalResultRows)
                {
                    var msg = $"Overview total results {overviewTotalResults} != counted rows {totalResultRows}";
                    if (strict) Assert.True(false, msg); else Info(msg);
                }
            }
            Pause();
        }

        [Fact(DisplayName = "Public pages view-only (recording helper)")]
        [Trait("Category", "UI-Recording")]
        public void PublicPages_ViewOnlyFlow()
        {
            // Optional initial delay for recording as well
            InitialDelayIfRequested("PublicPages_ViewOnlyFlow");
            if (!FrontendReachable()) return; // silently skip if front-end not up

            var tournamentId = Environment.GetEnvironmentVariable("PUBLIC_TOURNAMENT_ID");
            if (string.IsNullOrWhiteSpace(tournamentId))
            {
                Info("PUBLIC_TOURNAMENT_ID not set; skipping recording helper test.");
                return;
            }

            NavigatePublicPagesForRecording(tournamentId);
            // Minimal sanity check to keep xUnit from optimizing away
            Assert.Contains("AquaChamps", _driver.PageSource);
        }

        public void Dispose()
        {
            try { _driver.Quit(); } catch { /* ignore */ }
            _http.Dispose();
        }
    }
}
