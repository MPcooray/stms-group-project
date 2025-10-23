using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Linq;
using System.Threading;

class Program
{
    static int Main(string[] args)
    {
        var tournamentId = args.Length > 0 ? args[0] : "1"; // default to 1
        var frontendBase = "http://localhost:3000";
        var downloadDir = Path.Combine(Path.GetTempPath(), "stms-public-downloads");
        Directory.CreateDirectory(downloadDir);
        var artifactsDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "tests", "artifacts");
        Directory.CreateDirectory(artifactsDir);

        var options = new ChromeOptions();
        options.AddUserProfilePreference("download.default_directory", downloadDir);
        options.AddUserProfilePreference("download.prompt_for_download", false);
        options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
        options.AddArgument("--window-size=1400,1000");

        using var driver = new ChromeDriver(options);
        try
        {
            Console.WriteLine($"Opening leaderboard for tournament {tournamentId}...");
            driver.Navigate().GoToUrl($"{frontendBase}/public/tournaments/{tournamentId}/leaderboard");
            Thread.Sleep(1500);
            // capture screenshot for debugging
            try
            {
                var ss = ((ITakesScreenshot)driver).GetScreenshot();
                var lbShot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "tests", "artifacts", $"leaderboard_{tournamentId}.png");
                ss.SaveAsFile(lbShot, ScreenshotImageFormat.Png);
                Console.WriteLine("Saved leaderboard screenshot: " + lbShot);
            }
            catch (Exception ex) { Console.WriteLine("Screenshot failed: " + ex.Message); }

            // Helper to click export and move PDF to artifacts with a suffix
            int exportCount = 0;
            void ClickExportAndCollect(string suffix)
            {
                // remove any existing pdfs
                foreach (var f in Directory.GetFiles(downloadDir, "*.pdf")) File.Delete(f);
                var exportBtn = driver.FindElements(By.XPath("//button[contains(., 'Export') or contains(., 'Download') or contains(., 'PDF')]")).FirstOrDefault();
                if (exportBtn == null)
                {
                    Console.WriteLine($"Export button not found for view {suffix}.");
                    return;
                }
                Console.WriteLine($"Clicking export for {suffix}...");
                exportBtn.Click();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (sw.Elapsed.TotalSeconds < 30)
                {
                    var files = Directory.GetFiles(downloadDir, "*.pdf");
                    if (files.Length > 0)
                    {
                        var src = files[0];
                        var dest = Path.Combine(artifactsDir, Path.GetFileNameWithoutExtension(src) + $"_{suffix}.pdf");
                        File.Copy(src, dest, overwrite: true);
                        Console.WriteLine("Collected PDF: " + dest);
                        exportCount++;
                        return;
                    }
                    Thread.Sleep(500);
                }
                Console.WriteLine($"No PDF detected within timeout for {suffix}.");
            }

            // Try player view first (default)
            ClickExportAndCollect("players");

            // Try toggles for Male/Female if present
            var toggleBtns = driver.FindElements(By.CssSelector(".view-toggle .toggle-btn"));
            if (toggleBtns.Count > 0)
            {
                foreach (var tb in toggleBtns)
                {
                    var txt = tb.Text.Trim();
                    if (string.IsNullOrEmpty(txt)) continue;
                    try
                    {
                        tb.Click();
                        Thread.Sleep(800);
                        // Export per toggle
                        ClickExportAndCollect(txt.Replace(' ', '_'));
                    }
                    catch (Exception ex) { Console.WriteLine("Toggle click failed: " + ex.Message); }
                }
            }

            // Now go to Results page and attempt an export there as well
            Console.WriteLine($"Opening results page for tournament {tournamentId}...");
            driver.Navigate().GoToUrl($"{frontendBase}/public/tournaments/{tournamentId}/results");
            Thread.Sleep(1500);
            try
            {
                var ss2 = ((ITakesScreenshot)driver).GetScreenshot();
                var resShot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "tests", "artifacts", $"results_{tournamentId}.png");
                ss2.SaveAsFile(resShot, ScreenshotImageFormat.Png);
                Console.WriteLine("Saved results screenshot: " + resShot);
            }
            catch (Exception ex) { Console.WriteLine("Screenshot failed: " + ex.Message); }
            ClickExportAndCollect("results");

            Console.WriteLine($"Exports collected: {exportCount}");
            return exportCount > 0 ? 0 : 5;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex);
            return 4;
        }
        finally
        {
            try { driver.Quit(); } catch { }
        }
    }
}
