using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
//using SeleniumExtras.WaitHelpers;
using Xunit;

namespace STMS.Api.Tests
{
    public class TournamentUITests
    {
        [Fact]
        public void TournamentList_ReflectsChanges()
        {
            using var driver = new ChromeDriver();

            // 1. Go to the login page and log in as admin
            // driver.Navigate().GoToUrl("http://localhost:3000/login");
            // driver.FindElement(By.Id("email")).SendKeys("admin@stms.com");
            // driver.FindElement(By.Id("password")).SendKeys("Admin#123");
            // driver.FindElement(By.Id("loginButton")).Click();

            // 2. Go to tournaments page
            driver.Navigate().GoToUrl("http://localhost:3000/tournaments");

            // 3. Create a tournament
            // No separate create button, just fill the form and click save
            // driver.FindElement(By.Id("createTournamentButton")).Click();
            driver.FindElement(By.Id("tournamentName")).SendKeys("Selenium Tournament");
            driver.FindElement(By.Id("tournamentVenue")).SendKeys("Selenium Venue");
            var dateInput = driver.FindElement(By.Id("tournamentDate"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = '2025-09-12';", dateInput);
            driver.FindElement(By.Id("saveTournamentButton")).Click();
            System.Threading.Thread.Sleep(2000); // Wait longer for table to update
            // Print error message from form if present
            try
            {
                var errorDiv = driver.FindElement(By.ClassName("error"));
                Console.WriteLine("Form error: " + errorDiv.Text);
            }
            catch (NoSuchElementException) { }

            // 4. Verify tournament appears in list
            Assert.Contains("Selenium Tournament", driver.PageSource);

            // 5. Update tournament
                // 5. Update tournament (debug page source and retry loop for robustness)
                Console.WriteLine(driver.PageSource); // Debug: print HTML before searching for Edit button
                IWebElement? editButton = null;
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        editButton = driver.FindElement(By.XPath("//tr[td[contains(text(),'Selenium Tournament')]]//button[contains(text(),'Edit')]"));
                        break;
                    }
                    catch (NoSuchElementException)
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                }
                Assert.NotNull(editButton);
                editButton.Click();
            driver.FindElement(By.Id("tournamentName")).Clear();
            driver.FindElement(By.Id("tournamentName")).SendKeys("Selenium Tournament Updated");
            driver.FindElement(By.Id("saveTournamentButton")).Click();

            // 6. Verify update
            Assert.Contains("Selenium Tournament Updated", driver.PageSource);

            // 7. Delete tournament (retry loop for robustness)
            IWebElement? deleteButton = null;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    deleteButton = driver.FindElement(By.XPath("//tr[td[contains(text(),'Selenium Tournament Updated')]]//button[contains(text(),'Delete')]"));
                    break;
                }
                catch (NoSuchElementException)
                {
                    System.Threading.Thread.Sleep(500);
                }
            }
            Assert.NotNull(deleteButton);
            deleteButton.Click();
            // Accept the confirmation alert
            driver.SwitchTo().Alert().Accept();

            // 8. Verify deletion: wait for tournament to disappear from table
            bool deleted = false;
            for (int i = 0; i < 10; i++)
            {
                var tableRows = driver.FindElements(By.CssSelector("table tbody tr"));
                deleted = true;
                bool stale = false;
                foreach (var row in tableRows)
                {
                    try
                    {
                        if (row.Text.Contains("Selenium Tournament Updated"))
                        {
                            deleted = false;
                            break;
                        }
                    }
                    catch (OpenQA.Selenium.StaleElementReferenceException)
                    {
                        stale = true;
                        break;
                    }
                }
                if (stale) {
                    System.Threading.Thread.Sleep(500);
                    continue;
                }
                if (deleted) break;
                System.Threading.Thread.Sleep(500);
            }
            Assert.True(deleted, "Tournament was not deleted from the table");

            driver.Quit();
        }
    }
}
