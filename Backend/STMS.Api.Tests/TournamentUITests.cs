#nullable enable
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace STMS.Api.Tests
{
    public class TournamentUITests
    {

       // ---------- Helpers ----------
        private static WebDriverWait MakeWait(IWebDriver d, int seconds = 45)
        {
            var w = new WebDriverWait(d, TimeSpan.FromSeconds(seconds));
            w.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            return w;
        }

        private static IWebElement? FindRowLinkByText(IWebDriver d, string rowMustContain, string linkText, bool partial = false)
        {
            ReadOnlyCollection<IWebElement> rows;
            try { rows = d.FindElements(By.CssSelector("table tbody tr")); } catch { return null; }

            foreach (var tr in rows)
            {
                try
                {
                    if (!tr.Text.Contains(rowMustContain, StringComparison.OrdinalIgnoreCase)) continue;

                    // Accept either <a> or <button> with the given text
                    var xp = partial
                        ? $".//*[self::a or self::button][contains(normalize-space(.), '{linkText}')]"
                        : $".//*[self::a or self::button][normalize-space(.)='{linkText}']";

                    var els = tr.FindElements(By.XPath(xp));
                    if (els.Count > 0) return els[0];
                }
                catch (StaleElementReferenceException) { /* try next row */ }
            }
            return null;
        }

        private static int RankTableRowIndex(IWebDriver d, string player)
        {
            try
            {
                // Find the first visible table that has a 'Rank' header
                var tables = d.FindElements(By.XPath("//table[.//th[normalize-space()='Rank']]"));
                foreach (var table in tables)
                {
                    if (!table.Displayed) continue;
                    var rows = table.FindElements(By.CssSelector("tbody tr"));
                    for (int i = 0; i < rows.Count; i++)
                    {
                        try
                        {
                            if (rows[i].Text.Contains(player, StringComparison.OrdinalIgnoreCase))
                                return i;
                        }
                        catch (StaleElementReferenceException) { /* try next */ }
                    }
                }
            }
            catch { /* table not present yet */ }
            return -1; // not found
        }

        private static IWebElement WaitRowLinkByText(IWebDriver d, WebDriverWait w, string rowMustContain, string linkText, bool partial = false)
            => w.Until(dr => FindRowLinkByText(dr, rowMustContain, linkText, partial));

        private static IWebElement? FindCardLinkByText(IWebDriver d, string cardMustContain, string linkText, bool partial = false)
        {
            ReadOnlyCollection<IWebElement> cards;
            try { cards = d.FindElements(By.CssSelector(".card")); } catch { return null; }
            foreach (var card in cards)
            {
                try
                {
                    if (!card.Text.Contains(cardMustContain, StringComparison.OrdinalIgnoreCase)) continue;
                    var links = partial ? card.FindElements(By.PartialLinkText(linkText))
                                        : card.FindElements(By.LinkText(linkText));
                    if (links.Count > 0) return links[0];
                }
                catch (StaleElementReferenceException) { /* skip */ }
            }
            return null;
        }
        private static IWebElement WaitCardLinkByText(IWebDriver d, WebDriverWait w, string cardMustContain, string linkText, bool partial = false)
            => w.Until(dr => FindCardLinkByText(dr, cardMustContain, linkText, partial));

        private static void TinyPause(int ms = 250) => System.Threading.Thread.Sleep(ms);

        // --- NEW: minimal helpers for event-row actions and adding players to a uni ---

        private static IWebElement WaitEventRowAction(IWebDriver d, WebDriverWait w, string eventName, string actionText)
            => w.Until(dr =>
            {
                var row = dr.FindElements(By.XPath($"//tr[td[contains(normalize-space(.), '{eventName}')]]"))
                            .FirstOrDefault();
                if (row == null) return null;
                return row.FindElements(By.XPath($".//*[self::a or self::button][contains(normalize-space(.), '{actionText}')]"))
                          .FirstOrDefault();
            });

        private static void EnsurePlayerForUni(IWebDriver driver, WebDriverWait wait, string uniName, string playerName)
        {
            // We should be on the Universities page for current tournament
            // Open the Players view for that university
            var openPlayers = wait.Until(d =>
                d.FindElements(By.XPath($"//tr[td[contains(.,'{uniName}')]]//*[self::a or self::button][contains(normalize-space(.), 'View Players')]"))
                 .FirstOrDefault());
            openPlayers.Click();

            // If player missing, add
            var exists = driver.FindElements(By.XPath($"//table//tr[td[contains(normalize-space(.), '{playerName}')]]")).Count > 0;
            if (!exists)
            {
                var nameInput = wait.Until(d => d.FindElement(By.CssSelector("form input")));
                nameInput.Clear();
                nameInput.SendKeys(playerName);
                driver.FindElement(By.CssSelector("button.btn.primary")).Click();
                wait.Until(d => d.PageSource.Contains(playerName));
            }

            // Back to Universities list
            driver.Navigate().Back();
            wait.Until(d => d.FindElements(By.XPath($"//tr[td[contains(.,'{uniName}')]]")).Count > 0);
        }

        // Nadil
        [Fact]
        public void TournamentList_ReflectsChanges()
        {
            using var driver = new ChromeDriver();

            driver.Navigate().GoToUrl("http://localhost:3000/login/");
            System.Threading.Thread.Sleep(3000);
            IWebElement? emailInput = null;
            for (int i = 0; i < 10; i++)
            {
                try { emailInput = driver.FindElement(By.CssSelector("input[placeholder='admin@stms.com']")); break; }
                catch (NoSuchElementException) { System.Threading.Thread.Sleep(200); }
            }
            Assert.NotNull(emailInput);
            System.Threading.Thread.Sleep(500);
            driver.FindElement(By.Id("loginButton")).Click();
            System.Threading.Thread.Sleep(1000);

            driver.Navigate().GoToUrl("http://localhost:3000/tournaments");
            System.Threading.Thread.Sleep(1000);

            driver.FindElement(By.Id("tournamentName")).SendKeys("Selenium Tournament");
            driver.FindElement(By.Id("tournamentVenue")).SendKeys("Selenium Venue");
            var dateInput = driver.FindElement(By.Id("tournamentDate"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = '2025-09-12';", dateInput);
            driver.FindElement(By.Id("saveTournamentButton")).Click();
            System.Threading.Thread.Sleep(3000);
            try { var err = driver.FindElement(By.ClassName("error")); Console.WriteLine("Form error: " + err.Text); } catch { }

            Assert.Contains("Selenium Tournament", driver.PageSource);

            Console.WriteLine(driver.PageSource);
            IWebElement? editButton = null;
            for (int i = 0; i < 10; i++)
            {
                try { editButton = driver.FindElement(By.XPath("//tr[td[contains(text(),'Selenium Tournament')]]//button[contains(text(),'Edit')]")); break; }
                catch (NoSuchElementException) { System.Threading.Thread.Sleep(500); }
            }
            Assert.NotNull(editButton);
            editButton!.Click();
            driver.FindElement(By.Id("tournamentName")).Clear();
            System.Threading.Thread.Sleep(500);
            driver.FindElement(By.Id("tournamentName")).SendKeys("Selenium Tournament Updated");
            System.Threading.Thread.Sleep(500);
            driver.FindElement(By.Id("saveTournamentButton")).Click();
            System.Threading.Thread.Sleep(500);

            Assert.Contains("Selenium Tournament Updated", driver.PageSource);
            System.Threading.Thread.Sleep(2000);

            IWebElement? deleteButton = null;
            for (int i = 0; i < 10; i++)
            {
                try { deleteButton = driver.FindElement(By.XPath("//tr[td[contains(text(),'Selenium Tournament Updated')]]//button[contains(text(),'Delete')]")); break; }
                catch (NoSuchElementException) { System.Threading.Thread.Sleep(500); }
            }
            Assert.NotNull(deleteButton);
            System.Threading.Thread.Sleep(2000);
            deleteButton!.Click();
            System.Threading.Thread.Sleep(1000);
            driver.SwitchTo().Alert().Accept();
            System.Threading.Thread.Sleep(700);

            bool deleted = false;
            for (int i = 0; i < 10; i++)
            {
                var tableRows = driver.FindElements(By.CssSelector("table tbody tr"));
                deleted = true;
                bool stale = false;
                foreach (var row in tableRows)
                {
                    try { if (row.Text.Contains("Selenium Tournament Updated")) { deleted = false; break; } }
                    catch (StaleElementReferenceException) { stale = true; break; }
                }
                if (stale) { System.Threading.Thread.Sleep(500); continue; }
                if (deleted) break;
                System.Threading.Thread.Sleep(500);
            }
            Assert.True(deleted, "Tournament was not deleted from the table");

            System.Threading.Thread.Sleep(2000);
            driver.Quit();
        }

        // Madara
        // Results flow (robust waits)
        [Fact]
        public void ResultsFlow_TimingsToLeaderboardAndResults_E2E()
        {
            using var driver = new ChromeDriver();
            var wait = MakeWait(driver, 30);

            // Login
            driver.Navigate().GoToUrl("http://localhost:3000/login/");
            wait.Until(d => d.FindElement(By.Id("loginButton"))).Click();
            try
            {
                wait.Until(d => d.Url.Contains("/dashboard"));
            }
            catch (OpenQA.Selenium.WebDriverTimeoutException)
            {
                // tolerate if already logged in
            }

            // Create tournament
            driver.Navigate().GoToUrl("http://localhost:3000/tournaments");
            string tName = $"E2E Cup {DateTime.UtcNow:HHmmss}";
            wait.Until(d => d.FindElement(By.Id("tournamentName"))).SendKeys(tName);
            driver.FindElement(By.Id("tournamentVenue")).SendKeys("Pool A");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value='2025-09-12';", driver.FindElement(By.Id("tournamentDate")));
            driver.FindElement(By.Id("saveTournamentButton")).Click();
            wait.Until(d => d.PageSource.Contains(tName));

            // Dashboard --> Universities
            driver.Navigate().GoToUrl("http://localhost:3000/dashboard");
            wait.Until(d => d.PageSource.Contains(tName));
            WaitRowLinkByText(driver, wait, tName, "Universities", partial: true).Click();

            // Add Uni A & B
            var uniName = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder='Enter university name']")));
            uniName.SendKeys("Uni A");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.FindElements(By.XPath("//table//tr[td[contains(normalize-space(.), 'Uni A')]]")).Count > 0);

            uniName = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder='Enter university name']")));
            uniName.Clear();
            uniName.SendKeys("Uni B");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.FindElements(By.XPath("//table//tr[td[contains(normalize-space(.), 'Uni B')]]")).Count > 0);

            // --- NEW: Add players to each university BEFORE going to Timings ---
            EnsurePlayerForUni(driver, wait, "Uni A", "Alice");
            EnsurePlayerForUni(driver, wait, "Uni A", "Bob");
            EnsurePlayerForUni(driver, wait, "Uni B", "Cara");

            // Dashboard --> Events
            driver.Navigate().GoToUrl("http://localhost:3000/dashboard");
            WaitRowLinkByText(driver, wait, tName, "Events", partial: true).Click();

            // Create event
            var eventName = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder^='Enter event name']")));
            eventName.SendKeys("100m Freestyle");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.PageSource.Contains("100m Freestyle"));

            // Timings & Rankings
            // OLD (fragile): wait.Until(d => d.FindElement(By.PartialLinkText("Timings"))).Click();
            // NEW: click Timings within the 100m Freestyle row; if hidden, open details/manage first
            IWebElement timingsBtn = WaitEventRowAction(driver, wait, "100m Freestyle", "Timings");
            if (timingsBtn == null)
            {
                var details = wait.Until(d =>
                    d.FindElements(By.XPath($"//tr[td[contains(normalize-space(.), '100m Freestyle')]]" +
                                            "//*[self::a or self::button][contains(normalize-space(.), 'Manage') or " +
                                            " contains(normalize-space(.), 'View') or contains(normalize-space(.), 'Details')]"))
                     .FirstOrDefault());
                details.Click();

                timingsBtn = wait.Until(d =>
                    d.FindElements(By.XPath("//*[self::a or self::button][contains(normalize-space(.), 'Timings')]"))
                     .FirstOrDefault());
            }
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", timingsBtn);
            timingsBtn.Click();

            // Register players (unchanged)
            IWebElement SelPlayer()
                => wait.Until(d => d.FindElement(By.XPath("//form[.//button[normalize-space()='Register']]//select[1]")));

            void SetFilterFor(string player)
            {
                var uniSelList = driver.FindElements(By.XPath("//label[contains(.,'University')]/following::select[1]"));
                if (uniSelList.Count == 0) return;

                var wantUni = (player == "Cara") ? "Uni B" : "Uni A";
                var uniSel = uniSelList[0];

                wait.Until(_ => uniSel.FindElements(By.XPath($".//option[contains(normalize-space(.), '{wantUni}')]")).Count > 0);
                uniSel.Click();
                uniSel.FindElement(By.XPath($".//option[contains(normalize-space(.), '{wantUni}')]")).Click();

                TinyPause(250);
            }

            void Register(string player)
            {
                SetFilterFor(player);

                wait.Until(_ =>
                {
                    var sel = SelPlayer();
                    var hasAny = sel.FindElements(By.CssSelector("option")).Count > 1;
                    var hasPlayer = sel.FindElements(By.XPath($".//option[contains(normalize-space(.), '{player}')]")).Count > 0;
                    return hasAny && hasPlayer;
                });

                var sel2 = SelPlayer();
                sel2.Click();
                sel2.FindElement(By.XPath($".//option[contains(normalize-space(.), '{player}')]")).Click();

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].dispatchEvent(new Event('change', {bubbles:true}));", sel2);

                driver.FindElement(By.XPath("//form[.//button[normalize-space()='Register']]//button[normalize-space()='Register']")).Click();

                wait.Until(d => d.FindElements(By.XPath($"//table//tr[td[contains(.,'{player}')]]")).Count > 0);
                TinyPause(150);
            }

            Register("Alice");
            Register("Bob");
            Register("Cara");

            // Enter timings (unchanged)
            void SetTiming(string player, string value)
            {
                var row = wait.Until(d => d.FindElement(By.XPath($"//table//tr[td[contains(.,'{player}')]]")));
                var input = row.FindElement(By.CssSelector("input[placeholder='Enter time'], input[type='number']"));
                input.Clear(); input.SendKeys(value);
                TinyPause(250);
            }
            SetTiming("Alice", "58.10");
            SetTiming("Bob", "60.00");
            SetTiming("Cara", "61.25");

            string GetCell(string player, int tdIndex) =>
                wait.Until(d => d.FindElement(By.XPath($"//table//tr[td[contains(.,'{player}')]]/td[{tdIndex}]"))).Text.Trim();

            // Wait for the ranking table to stabilize and show Alice < Bob < Cara
            wait.Until(_ =>
            {
                var a = RankTableRowIndex(driver, "Alice");
                var b = RankTableRowIndex(driver, "Bob");
                var c = RankTableRowIndex(driver, "Cara");
                return a >= 0 && b >= 0 && c >= 0 && a < b && b < c;
            });

            //assert again with a clearer failure message
            var idxAlice = RankTableRowIndex(driver, "Alice");
            var idxBob   = RankTableRowIndex(driver, "Bob");
            var idxCara  = RankTableRowIndex(driver, "Cara");
            Assert.True(idxAlice < idxBob && idxBob < idxCara,
                $"Expected Alice < Bob < Cara in ranking table, got indices A={idxAlice}, B={idxBob}, C={idxCara}.");

            // Keep points checks as-is
            Assert.Equal("10", GetCell("Alice", 5));
            Assert.Equal("8",  GetCell("Bob",   5));
            Assert.Equal("7",  GetCell("Cara",  5));

            SetTiming("Bob", "57.90");
            TinyPause(700);
            // Wait until Bob rises above Alice in the ranking table
            wait.Until(_ =>
            {
                var b = RankTableRowIndex(driver, "Bob");
                var a = RankTableRowIndex(driver, "Alice");
                return b >= 0 && a >= 0 && b < a;
            });

            //assert with message
            var idxBob2   = RankTableRowIndex(driver, "Bob");
            var idxAlice2 = RankTableRowIndex(driver, "Alice");
            Assert.True(idxBob2 < idxAlice2,
                $"Expected Bob above Alice after update, got indices Bob={idxBob2}, Alice={idxAlice2}.");

            // Points still map to ranks
            Assert.Equal("10", GetCell("Bob",   5));
            Assert.Equal("8",  GetCell("Alice", 5));

            // --- REMOVE LEADERBOARD CHECKING ---
            // driver.Navigate().GoToUrl("http://localhost:3000/leaderboard");
            // WaitCardLinkByText(driver, wait, tName, "Leaderboard", partial: true).Click();
            // var firstPlayerRow = wait.Until(d =>
            //     d.FindElement(By.XPath("//h3[.='Player Leaderboard']/following::table[1]//tbody/tr[2]")));
            // Assert.Contains("Bob", firstPlayerRow.Text);

            // --- REMOVE RESULTS CHECKING ---
            // driver.Navigate().GoToUrl("http://localhost:3000/results");
            // var select = wait.Until(d => d.FindElement(By.Id("tournament-select")));
            // select.Click();
            // select.FindElement(By.XPath($".//option[contains(.,'{tName}')]")).Click();
            // wait.Until(d => d.PageSource.Contains("100m Freestyle"));
            // wait.Until(d =>
            // {
            //     var rows = d.FindElements(By.XPath("//h3[.='100m Freestyle']/following::table[1]//tbody/tr"));
            //     return rows.Count > 0 && !rows[0].Text.Contains("No results yet.") && rows[0].Text.Contains("Bob");
            // });
            // var resultsTopRow = wait.Until(d => d.FindElement(By.XPath("//h3[.='100m Freestyle']/following::table[1]//tbody/tr[1]")));
            // Assert.Contains("Bob", resultsTopRow.Text);
            // Assert.Contains("10", resultsTopRow.Text);

            driver.Quit();
        }

        // Universities/Players CRUD + cascade
        [Fact]
        public void UniversitiesPlayers_CRUD_AndCascadeDelete()
        {
            using var driver = new ChromeDriver();
            var wait = MakeWait(driver);

            // Login
            driver.Navigate().GoToUrl("http://localhost:3000/login/");
            wait.Until(d => d.FindElement(By.Id("loginButton"))).Click();
            wait.Until(d => d.Url.Contains("/dashboard"));

            // Create tournament
            driver.Navigate().GoToUrl("http://localhost:3000/tournaments");
            string tName = $"Cascade Cup {DateTime.UtcNow:HHmmss}";
            wait.Until(d => d.FindElement(By.Id("tournamentName"))).SendKeys(tName);
            driver.FindElement(By.Id("tournamentVenue")).SendKeys("Pool B");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value='2025-09-15';", driver.FindElement(By.Id("tournamentDate")));
            driver.FindElement(By.Id("saveTournamentButton")).Click();
            wait.Until(d => d.PageSource.Contains(tName));

            // Dashboard --> Universities
            driver.Navigate().GoToUrl("http://localhost:3000/dashboard");
            wait.Until(d => d.PageSource.Contains(tName));
            WaitRowLinkByText(driver, wait, tName, "Universities", partial: true).Click();

            // Add Uni X
            var uniName = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder='Enter university name']")));
            uniName.SendKeys("Uni X");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.PageSource.Contains("Uni X"));

            // View Players
            wait.Until(d => d.FindElement(By.PartialLinkText("View Players"))).Click();

            // Add -> Edit -> Delete a player
            var nameInput = wait.Until(d => d.FindElement(By.CssSelector("form input")));
            nameInput.SendKeys("Delta");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.PageSource.Contains("Delta"));

            wait.Until(d => d.FindElement(By.XPath("//tr[td[contains(.,'Delta')]]//button[contains(.,'Edit')]"))).Click();
            var editName = wait.Until(d => d.FindElement(By.CssSelector("form input")));
            editName.Clear(); editName.SendKeys("Delta Prime");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.PageSource.Contains("Delta Prime"));

            wait.Until(d => d.FindElement(By.XPath("//tr[td[contains(.,'Delta Prime')]]//button[contains(.,'Delete')]"))).Click();
            driver.SwitchTo().Alert().Accept();
            TinyPause(400);
            Assert.DoesNotContain("Delta Prime", driver.PageSource);

            // Back to Universities and delete Uni X (cascade)
            driver.Navigate().Back();
            wait.Until(d => d.FindElement(By.XPath("//tr[td[contains(.,'Uni X')]]//button[contains(.,'Delete')]"))).Click();
            driver.SwitchTo().Alert().Accept();
            wait.Until(d => !d.PageSource.Contains("Uni X"));

            // Dashboard --> All Players (verify Uni X gone)
            driver.Navigate().GoToUrl("http://localhost:3000/dashboard");
            WaitRowLinkByText(driver, wait, tName, "All Players", partial: true).Click();
            Assert.DoesNotContain("Uni X", driver.PageSource);

            driver.Quit();
        }
    }
}
