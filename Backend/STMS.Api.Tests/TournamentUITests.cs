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
        private static WebDriverWait MakeWait(IWebDriver d, int seconds = 20)
            => new WebDriverWait(d, TimeSpan.FromSeconds(seconds));

        private static IWebElement? FindRowLinkByText(IWebDriver d, string rowMustContain, string linkText, bool partial = false)
        {
            ReadOnlyCollection<IWebElement> rows;
            try { rows = d.FindElements(By.CssSelector("table tbody tr")); } catch { return null; }
            foreach (var tr in rows)
            {
                try
                {
                    if (!tr.Text.Contains(rowMustContain)) continue;
                    var links = partial ? tr.FindElements(By.PartialLinkText(linkText))
                                        : tr.FindElements(By.LinkText(linkText));
                    if (links.Count > 0) return links[0];
                }
                catch (StaleElementReferenceException) { /* try next tick */ }
            }
            return null;
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
                    if (!card.Text.Contains(cardMustContain)) continue;
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
            var wait = MakeWait(driver);

            // Login
            driver.Navigate().GoToUrl("http://localhost:3000/login/");
            wait.Until(d => d.FindElement(By.Id("loginButton"))).Click();
            wait.Until(d => d.Url.Contains("/dashboard"));

            // Create tournament
            driver.Navigate().GoToUrl("http://localhost:3000/tournaments");
            string tName = $"E2E Cup {DateTime.UtcNow:HHmmss}";
            wait.Until(d => d.FindElement(By.Id("tournamentName"))).SendKeys(tName);
            driver.FindElement(By.Id("tournamentVenue")).SendKeys("Pool A");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value='2025-09-12';", driver.FindElement(By.Id("tournamentDate")));
            driver.FindElement(By.Id("saveTournamentButton")).Click();
            wait.Until(d => d.PageSource.Contains(tName));

            // Dashboard → Universities
            driver.Navigate().GoToUrl("http://localhost:3000/dashboard");
            WaitRowLinkByText(driver, wait, tName, "Universities", partial: true).Click();

            // Add Uni A & B (placeholder from Universities.jsx)
            var uniName = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder='Enter university name']")));
            uniName.SendKeys("Uni A");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.PageSource.Contains("Uni A"));
            uniName = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder='Enter university name']")));
            uniName.Clear(); uniName.SendKeys("Uni B");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.PageSource.Contains("Uni B"));

            // Add players for Uni A
            wait.Until(d => d.FindElement(By.PartialLinkText("View Players"))).Click();
            var nameInput = wait.Until(d => d.FindElement(By.CssSelector("form input")));
            nameInput.SendKeys("Alice");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.PageSource.Contains("Alice"));
            nameInput = wait.Until(d => d.FindElement(By.CssSelector("form input")));
            nameInput.Clear(); nameInput.SendKeys("Bob");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.PageSource.Contains("Bob"));

            // Add player for Uni B
            driver.Navigate().Back();
            wait.Until(d => d.FindElements(By.PartialLinkText("View Players")).Count >= 2);
            var viewPlayersButtons = driver.FindElements(By.PartialLinkText("View Players"));
            viewPlayersButtons.Last().Click();
            nameInput = wait.Until(d => d.FindElement(By.CssSelector("form input")));
            nameInput.SendKeys("Cara");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.PageSource.Contains("Cara"));

            // Dashboard --> Events
            driver.Navigate().GoToUrl("http://localhost:3000/dashboard");
            WaitRowLinkByText(driver, wait, tName, "Events", partial: true).Click();

            // Create event (placeholder from Events.jsx)
            var eventName = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder^='Enter event name']")));
            eventName.SendKeys("100m Freestyle");
            driver.FindElement(By.CssSelector("button.btn.primary")).Click();
            wait.Until(d => d.PageSource.Contains("100m Freestyle"));

            // Timings & Rankings
            wait.Until(d => d.FindElement(By.PartialLinkText("Timings"))).Click();

            // Register players 
            IWebElement SelPlayer() => wait.Until(d => d.FindElement(By.XPath("//label[normalize-space()='Select Player']/following::select[1]")));
            void Register(string player)
            {
                var sel = SelPlayer();
                sel.Click();
                sel.FindElement(By.XPath($".//option[contains(., '{player}')]")).Click();
                driver.FindElement(By.XPath("//button[normalize-space()='Register']")).Click();
                TinyPause(200);
            }
            Register("Alice");
            Register("Bob");
            Register("Cara");

            // Enter timings
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

            Assert.Equal("1", GetCell("Alice", 1));
            Assert.Equal("10", GetCell("Alice", 5));
            Assert.Equal("2", GetCell("Bob", 1));
            Assert.Equal("8", GetCell("Bob", 5));
            Assert.Equal("3", GetCell("Cara", 1));
            Assert.Equal("7", GetCell("Cara", 5));

            SetTiming("Bob", "57.90");
            TinyPause(700);
            Assert.Equal("1", GetCell("Bob", 1));
            Assert.Equal("10", GetCell("Bob", 5));
            Assert.Equal("2", GetCell("Alice", 1));
            Assert.Equal("8", GetCell("Alice", 5));

            // Leaderboard
            driver.Navigate().GoToUrl("http://localhost:3000/leaderboard");
            WaitCardLinkByText(driver, wait, tName, "Leaderboard", partial: true).Click();
            var firstPlayerRow = wait.Until(d =>
                d.FindElement(By.XPath("//h3[.='Player Leaderboard']/following::table[1]//tbody/tr[1]")));
            Assert.Contains("Bob", firstPlayerRow.Text);

            // Results
            driver.Navigate().GoToUrl("http://localhost:3000/results");
            var select = wait.Until(d => d.FindElement(By.Id("tournament-select")));
            select.Click();
            select.FindElement(By.XPath($".//option[contains(.,'{tName}')]")).Click();
            wait.Until(d => d.PageSource.Contains("100m Freestyle"));
            var resultsTopRow = wait.Until(d => d.FindElement(By.XPath("//h3[.='100m Freestyle']/following::table[1]//tbody/tr[1]")));
            Assert.Contains("Bob", resultsTopRow.Text);
            Assert.Contains("10", resultsTopRow.Text);

            driver.Quit();
        }

        
        // NEW TEST 2: Universities/Players CRUD + cascade
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
