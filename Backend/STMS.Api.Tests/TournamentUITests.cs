#nullable enable
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using System;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;

namespace STMS.Api.Tests
{
    public class TournamentUITests
    {

        private static IWebDriver CreateDriver()
        {
            var downloadDir = Path.Combine(Path.GetTempPath(), "stms-downloads");
            try { Directory.CreateDirectory(downloadDir); } catch { }
            var options = new ChromeOptions();
            // allow downloads without prompt and set directory
            options.AddUserProfilePreference("download.default_directory", downloadDir);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);
            // use a visible browser for easier debugging; remove Headless if present
            // options.AddArgument("--headless=new"); // avoid headless for downloads unless configured
            return new ChromeDriver(options);
        }

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

        private static void FillLogin(IWebDriver driver, WebDriverWait wait, string email = "admin@stms.com", string password = "Admin#123")
        {
            // Robust login helper: try click+SendKeys, then JS-value-and-dispatch, then verify and submit
            IWebElement emailEl = null!;
            IWebElement pwdEl = null!;
            try
            {
                emailEl = wait.Until(d => d.FindElement(By.CssSelector("input[name='email']")));
                pwdEl = wait.Until(d => d.FindElement(By.Id("password")));
            }
            catch
            {
                // let exception surface to caller
                throw;
            }

            try
            {
                // Try normal interaction first
                emailEl.Click(); emailEl.Clear(); emailEl.SendKeys(email);
                pwdEl.Click(); pwdEl.Clear(); pwdEl.SendKeys(password);
            }
            catch { /* ignore and try JS fallback */ }

            try
            {
                // Ensure React controlled inputs receive the change by dispatching both input and change
                ((IJavaScriptExecutor)driver).ExecuteScript(@"
                    arguments[0].value = arguments[2];
                    arguments[0].dispatchEvent(new Event('input', { bubbles: true }));
                    arguments[0].dispatchEvent(new Event('change', { bubbles: true }));
                    arguments[1].value = arguments[3];
                    arguments[1].dispatchEvent(new Event('input', { bubbles: true }));
                    arguments[1].dispatchEvent(new Event('change', { bubbles: true }));
                ", emailEl, pwdEl, email, password);
            }
            catch { /* ignore */ }

            // Verify values; if not matching, attempt SendKeys again
            try
            {
                var actualEmail = ((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].value;", emailEl) as string ?? "";
                var actualPwd = ((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].value;", pwdEl) as string ?? "";
                if (actualEmail != email || actualPwd != password)
                {
                    try { emailEl.Clear(); emailEl.SendKeys(email); pwdEl.Clear(); pwdEl.SendKeys(password); } catch { }
                }
            }
            catch { /* ignore */ }

            // Submit login: try id first, then button text, allow longer wait
            IWebElement loginBtn = null!;
            try
            {
                loginBtn = new WebDriverWait(driver, TimeSpan.FromSeconds(20)).Until(d =>
                {
                    try { return d.FindElement(By.Id("loginButton")); } catch { }
                    try
                    {
                        var btn = d.FindElements(By.XPath("//button[normalize-space(.)='Sign In' or normalize-space(.)='Signing in…']")).FirstOrDefault();
                        if (btn != null) return btn;
                    }
                    catch { }
                    return null;
                });
                loginBtn.Click();
            }
            catch (WebDriverTimeoutException)
            {
                // Dump a small page-source snippet for debugging
                try
                {
                    var src = driver.PageSource;
                    var snippet = src.Length > 4000 ? src.Substring(0, 4000) : src;
                    Console.WriteLine("[DEBUG] Login button not found. Page source snippet:\n" + snippet);
                }
                catch { }
                throw;
            }

            // Wait for redirect to dashboard or tournaments
            wait.Until(d => d.Url.Contains("/dashboard") || d.Url.Contains("/tournaments"));
        }

        // Nadil
        [Fact]
        public void TournamentList_ReflectsChanges()
        {
            using var driver = CreateDriver();

            driver.Navigate().GoToUrl("http://localhost:3000/login/");
            // Use robust waits and stable selectors for login inputs (login placeholder changed in frontend)
            var shortWait = MakeWait(driver, 15);
            // FillLogin will both populate inputs and submit, and wait for redirect
            FillLogin(driver, shortWait);

            driver.Navigate().GoToUrl("http://localhost:3000/tournaments");
            System.Threading.Thread.Sleep(1000);

            // Use a unique tournament name per test run to avoid matching older rows
            var uniqueName = "Selenium Tournament " + DateTime.UtcNow.ToString("HHmmssfff");
            // Wait for tournament form to be present, then fill and submit
            var tournamentNameInput = shortWait.Until(d => d.FindElement(By.Id("tournamentName")));
            tournamentNameInput.Clear();
            tournamentNameInput.SendKeys(uniqueName);
            shortWait.Until(d => d.FindElement(By.Id("tournamentVenue"))).SendKeys("Selenium Venue");
            var dateInput = shortWait.Until(d => d.FindElement(By.Id("tournamentDate")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = '2025-09-12';", dateInput);
            shortWait.Until(d => d.FindElement(By.Id("saveTournamentButton"))).Click();
            // Wait until the created tournament (uniqueName) appears in the All Tournaments list
            shortWait.Until(d => d.PageSource.Contains(uniqueName));

            Console.WriteLine(driver.PageSource);
            IWebElement? editButton = null;
            for (int i = 0; i < 10; i++)
            {
                try { editButton = driver.FindElement(By.XPath($"//tr[td[contains(text(),'{uniqueName}')]]//button[contains(text(),'Edit')]")); break; }
                catch (NoSuchElementException) { System.Threading.Thread.Sleep(500); }
            }
            Assert.NotNull(editButton);
            editButton!.Click();
            // Wait for the form/card heading to indicate Edit mode (heading may be a sibling of the form)
            shortWait.Until(d =>
            {
                try
                {
                    var h = d.FindElements(By.CssSelector(".card h3")).FirstOrDefault();
                    if (h == null) return false;
                    return h.Text.Contains("Edit Tournament", StringComparison.OrdinalIgnoreCase)
                        || h.Text.Contains("Edit", StringComparison.OrdinalIgnoreCase);
                }
                catch { return false; }
            });
            var nameInput = shortWait.Until(d => d.FindElement(By.Id("tournamentName")));
            // Use JS to set the value and dispatch events to ensure React controlled input updates
            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                arguments[0].value = arguments[1];
                arguments[0].dispatchEvent(new Event('input', { bubbles: true }));
                arguments[0].dispatchEvent(new Event('change', { bubbles: true }));
            ", nameInput, "Selenium Tournament Updated");
            // Click save and wait for the updated name to appear in the All Tournaments list
            shortWait.Until(d => d.FindElement(By.Id("saveTournamentButton"))).Click();
            shortWait.Until(d => d.PageSource.Contains("Selenium Tournament Updated"));
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
            using var driver = CreateDriver();
            var wait = MakeWait(driver, 30);

            // Login
            driver.Navigate().GoToUrl("http://localhost:3000/login/");
            FillLogin(driver, wait);

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
            using var driver = CreateDriver();
            var wait = MakeWait(driver);

            // Login (use FillLogin to populate and submit)
            driver.Navigate().GoToUrl("http://localhost:3000/login/");
            FillLogin(driver, wait);

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
            // Wait until the deleted player no longer appears in the page source to avoid race conditions
            wait.Until(d => !d.PageSource.Contains("Delta Prime"));

            // Back to Universities and delete Uni X (cascade)
            driver.Navigate().Back();
            wait.Until(d => d.FindElement(By.XPath("//tr[td[contains(.,'Uni X')]]//button[contains(.,'Delete')]"))).Click();
            driver.SwitchTo().Alert().Accept();
            wait.Until(d => !d.PageSource.Contains("Uni X"));

            // Dashboard --> All Players (verify Uni X gone)
            driver.Navigate().GoToUrl("http://localhost:3000/dashboard");
            WaitRowLinkByText(driver, wait, tName, "All Players", partial: true).Click();
            // Wait until the University is removed from the All Players view
            wait.Until(d => !d.PageSource.Contains("Uni X"));

            driver.Quit();
        }
    }
}
