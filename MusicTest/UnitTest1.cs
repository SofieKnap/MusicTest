using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace MusicTest
{
    public class UnitTest1
    {
        private const string FrontendUrl = "http://127.0.0.1:5500/index.html";

        [Fact]
        public void Frontend_ViserEtKendtAlbum()
        {
            // Arrange: Opret en ny browser, så testen starter uden gemte favoritter.
            using var browser = OpretBrowser();

            // Act: Åbn frontend, og vent på at musikken er hentet fra Azure.
            AabnFrontend(browser);
            var album = browser.FindElements(By.CssSelector(".record-card"))
                .Single(kort => kort.FindElement(By.CssSelector(".record-title")).Text == "lowkey");

            // Assert: Titel og kunstner skal stå på det samme kort.
            Assert.Equal("lowkey", album.FindElement(By.CssSelector(".record-title")).Text);
            Assert.Equal("NIKI", album.FindElement(By.CssSelector(".record-artist")).Text);
        }

        // Samme test køres med titel, kunstner og begge dele sammen.
        [Theory]
        [InlineData("low", "")]
        [InlineData("", "niki")]
        [InlineData("low", "niki")]
        public void Frontend_KanSoegePaaTitelOgKunstner(string titel, string kunstner)
        {
            // Arrange
            using var browser = OpretBrowser();
            var wait = AabnFrontend(browser);

            // Act: Indtast filtre, og tryk på søgeknappen.
            browser.FindElement(By.Id("title")).SendKeys(titel);
            browser.FindElement(By.Id("artist")).SendKeys(kunstner);
            browser.FindElement(By.CssSelector(".search-form button[type='submit']")).Click();
            wait.Until(driver => driver.FindElement(By.CssSelector(".result-count")).Text == "1 nummer");

            // Assert: Kun lowkey af NIKI må være tilbage.
            var album = Assert.Single(browser.FindElements(By.CssSelector(".record-card")));
            Assert.Equal("lowkey", album.FindElement(By.CssSelector(".record-title")).Text);
            Assert.Equal("NIKI", album.FindElement(By.CssSelector(".record-artist")).Text);
        }

        [Fact]
        public void Frontend_ViserBeskedNaarSoegningenIkkeMatcher()
        {
            // Arrange
            using var browser = OpretBrowser();
            var wait = AabnFrontend(browser);

            // Act
            browser.FindElement(By.Id("title")).SendKeys("zzzz-findes-ikke-12345");
            browser.FindElement(By.CssSelector(".search-form button[type='submit']")).Click();
            wait.Until(driver => driver.FindElement(By.CssSelector(".result-count")).Text == "0 numre");

            // Assert
            Assert.Empty(browser.FindElements(By.CssSelector(".record-card")));
            Assert.Equal("Der var ikke noget, der passede.",
                browser.FindElement(By.CssSelector(".message h3")).Text);
        }

        [Fact]
        public void Frontend_KanGemmeOgFjerneFavorit()
        {
            // Arrange
            using var browser = OpretBrowser();
            var wait = AabnFrontend(browser);

            // Act: Gem lowkey, og genindlæs for at kontrollere localStorage.
            browser.FindElement(By.CssSelector("[aria-label='Gem favorit: lowkey']")).Click();
            browser.Navigate().Refresh();
            VentPaaMusikken(wait);

            // Assert: Hjertet skal stadig være markeret efter genindlæsning.
            Assert.Equal("true", browser.FindElement(
                By.CssSelector("[aria-label='Fjern favorit: lowkey']")).GetAttribute("aria-pressed"));

            // Act: Åbn favoritter, og fjern nummeret igen.
            browser.FindElement(By.CssSelector(".page-header nav button:last-child")).Click();
            wait.Until(driver => driver.FindElement(By.Id("collection-title")).Text == "Dine favoritter");
            var favorit = Assert.Single(browser.FindElements(By.CssSelector(".record-card")));
            Assert.Equal("lowkey", favorit.FindElement(By.CssSelector(".record-title")).Text);
            browser.FindElement(By.CssSelector("[aria-label='Fjern favorit: lowkey']")).Click();
            wait.Until(driver => driver.FindElements(By.CssSelector(".record-card")).Count == 0);

            // Assert: Favoritlisten skal nu være tom.
            Assert.Equal("Dine favoritter får plads her.",
                browser.FindElement(By.CssSelector(".message h3")).Text);
            Assert.Equal("0", browser.FindElement(By.CssSelector(".nav-count")).Text);
        }

        [Fact]
        public void Frontend_KanSortereNyesteFoerst()
        {
            // Arrange
            using var browser = OpretBrowser();
            var wait = AabnFrontend(browser);

            // Act
            var sortering = new SelectElement(browser.FindElement(By.Id("sort")));
            sortering.SelectByValue("newest");
            wait.Until(driver => driver.FindElement(By.CssSelector(".record-title")).Text == "Gabriela");

            // Assert: Alle årstal skal stå i faldende rækkefølge.
            var aarstal = browser.FindElements(By.CssSelector(".record-duration"))
                .Select(element => int.Parse(element.Text.Split(' ')[0])).ToList();
            Assert.Equal(aarstal.OrderByDescending(aar => aar), aarstal);
        }

        [Fact]
        public void Frontend_KanAabneOgLukkeDetaljer()
        {
            // Arrange
            using var browser = OpretBrowser();
            var wait = AabnFrontend(browser);

            // Act
            browser.FindElement(By.CssSelector("[aria-label='Vis detaljer om lowkey']")).Click();
            wait.Until(driver => driver.FindElement(By.CssSelector("dialog")).Displayed);

            // Assert: Dialogen skal vise oplysninger om det valgte nummer.
            Assert.Equal("lowkey", browser.FindElement(By.Id("dialog-title")).Text);
            Assert.Equal("NIKI", browser.FindElement(By.CssSelector(".dialog-artist")).Text);
            var detaljer = browser.FindElements(By.CssSelector("dialog dd"));
            Assert.Equal("2019", detaljer[0].Text);
            Assert.Equal("3:05", detaljer[1].Text);

            // Act og Assert: Luk dialogen, og kontrollér at den er skjult.
            browser.FindElement(By.CssSelector("[aria-label='Luk detaljer']")).Click();
            wait.Until(driver => !driver.FindElement(By.CssSelector("dialog")).Displayed);
            Assert.False(browser.FindElement(By.CssSelector("dialog")).Displayed);
        }

        // Små hjælpemetoder samler den opsætning, som alle testene bruger.
        private static ChromeDriver OpretBrowser()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new"); // Fjern linjen for at se Chrome under testen.
            options.AddArgument("--window-size=1440,1200");
            return new ChromeDriver(options);
        }

        private static WebDriverWait AabnFrontend(IWebDriver browser)
        {
            browser.Navigate().GoToUrl(FrontendUrl);
            var wait = new WebDriverWait(browser, TimeSpan.FromSeconds(30));
            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
            VentPaaMusikken(wait);
            return wait;
        }

        private static void VentPaaMusikken(WebDriverWait wait)
        {
            // Vue og API'et er asynkrone, så vi venter på synlige kort og et aktivt søgefelt.
            wait.Until(driver =>
                driver.FindElements(By.CssSelector(".record-card")).Any(kort => kort.Displayed)
                && driver.FindElement(By.Id("title")).Enabled);
        }
    }
}
