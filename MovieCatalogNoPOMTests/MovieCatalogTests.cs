using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium;

namespace MovieCatalogNoPOMTests
{
    public class MovieCatalogTests
    {
        private readonly static string BaseUrl = "http://moviecatalog-env.eba-ubyppecf.eu-north-1.elasticbeanstalk.com";
        private WebDriver driver;
        private Actions actions;
        private string? lastCreatedMovieTitle;
        private string? lastCreatedMovieDescription;

        [OneTimeSetUp]
        public void Setup()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("profile.password_manager_enable", false);
            chromeOptions.AddArgument("--disable-search-engine-choice-screen");

            driver = new ChromeDriver(chromeOptions);
            actions = new Actions(driver);
            driver.Navigate().GoToUrl(BaseUrl);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            driver.Navigate().GoToUrl($"{BaseUrl}/User/Login");

            driver.FindElement(By.XPath("//input[@name='Email']")).SendKeys("petqtest@test.bg");
            driver.FindElement(By.XPath("//input[@name='Password']")).SendKeys("123456");

            driver.FindElement(By.XPath("//button[text()='Login']")).Click();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }

        [Test, Order(1)]
        public void CreateMovieWithInvalidDataTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/Add#add");

            var titleInput = driver.FindElement(By.XPath("//input[@name='Title']"));
            titleInput.Clear();
            titleInput.SendKeys("");

            var descriptionInput = driver.FindElement(By.XPath("//textarea[@name='Description']"));
            descriptionInput.Clear();
            descriptionInput.SendKeys("");

            var formMovie = driver.FindElement(By.XPath("//div[@class='pt-1 mb-4']"));
            actions.ScrollToElement(formMovie).Perform();

            driver.FindElement(By.XPath("//button[text()='Add']")).Click();

            var errorMessage = driver.FindElement(By.XPath("//div[@class='toast-message']"));
            Assert.That(errorMessage.Text, Is.EqualTo("The Title field is required."), "The error message for invalid data when creating Movie is not there");
        }

        [Test, Order(2)]
        public void CreateMovieWithoutDescriptionTest()
        {
            lastCreatedMovieTitle = GenerateRandomTitle();
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/Add#add");

            var titleInput = driver.FindElement(By.XPath("//input[@name='Title']"));
            titleInput.Clear();
            titleInput.SendKeys(lastCreatedMovieTitle);

            var descriptionInput = driver.FindElement(By.XPath("//textarea[@name='Description']"));
            descriptionInput.Clear();
            descriptionInput.SendKeys("");

            var formMovie = driver.FindElement(By.XPath("//div[@class='pt-1 mb-4']"));
            actions.ScrollToElement(formMovie).Perform();

            driver.FindElement(By.XPath("//button[text()='Add']")).Click();

            var errorMessage = driver.FindElement(By.XPath("//div[@class='toast-message']"));
            Assert.That(errorMessage.Text, Is.EqualTo("The Description field is required."), "The error message for invalid description when creating Movie is not there");
        }

        [Test, Order(3)]
        public void AddMovieWithRandomTitleTest()
        {
            lastCreatedMovieTitle = GenerateRandomTitle();
            lastCreatedMovieDescription = GenerateRandomDescription();
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/Add#add");

            var titleInput = driver.FindElement(By.XPath("//input[@name='Title']"));
            titleInput.Clear();
            titleInput.SendKeys(lastCreatedMovieTitle);

            var descriptionInput = driver.FindElement(By.XPath("//textarea[@name='Description']"));
            descriptionInput.Clear();
            descriptionInput.SendKeys(lastCreatedMovieDescription);

            var formMovie = driver.FindElement(By.XPath("//div[@class='pt-1 mb-4']"));
            actions.ScrollToElement(formMovie).Perform();

            driver.FindElement(By.XPath("//button[text()='Add']")).Click();

            var pagesLinks = driver.FindElements(By.XPath("//a[@class='page-link']"));
            pagesLinks.Last().Click();

            var moviesCollection = driver.FindElements(By.XPath("//div[@class='col-lg-4']"));
            var lastMovieTitle = moviesCollection.Last().FindElement(By.XPath(".//h2")).Text;

            Assert.That(lastMovieTitle, Is.EqualTo(lastCreatedMovieTitle), "The title is not as expected");
        }

        [Test, Order(4)]
        public void EditLastCreatedMovieTest()
        {
            lastCreatedMovieTitle = GenerateRandomTitle() + "EDITED";
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/All#all");

            var pagesLinks = driver.FindElements(By.XPath("//a[@class='page-link']"));
            pagesLinks.Last().Click();

            var moviesCollection = driver.FindElements(By.XPath("//div[@class='col-lg-4']"));
            var lastMovieEditButton = moviesCollection.Last().FindElement(By.XPath(".//a[@class='btn btn-outline-success']"));
            lastMovieEditButton.Click();

            var titleInput = driver.FindElement(By.XPath("//input[@name='Title']"));
            titleInput.Clear();
            titleInput.SendKeys(lastCreatedMovieTitle);

            var formMovie = driver.FindElement(By.XPath("//div[@class='pt-1 mb-4']"));
            actions.ScrollToElement(formMovie).Perform();

            driver.FindElement(By.XPath("//button[text()='Edit']")).Click();

            var editMessage = driver.FindElement(By.XPath("//div[@class='toast-message']"));
            Assert.That(editMessage.Text, Is.EqualTo("The Movie is edited successfully!"), "The Movie was not edit.");

        }

        [Test, Order(5)]
        public void MarkLastAddedMovieAsWatchedTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/All#all");

            var pagesLinks = driver.FindElements(By.XPath("//a[@class='page-link']"));
            pagesLinks.Last().Click();

            var moviesCollection = driver.FindElements(By.XPath("//div[@class='col-lg-4']"));
            var lastMovieMarkAsWatchedButton = moviesCollection.Last().FindElement(By.XPath(".//a[@class='btn btn-info']"));
            lastMovieMarkAsWatchedButton.Click();

            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/Watched#watched");

            pagesLinks = driver.FindElements(By.XPath("//a[@class='page-link']"));
            pagesLinks.Last().Click();

            moviesCollection = driver.FindElements(By.XPath("//div[@class='col-lg-4']"));
            var lastMovieWatchedTitle = moviesCollection.Last().FindElement(By.XPath(".//h2")).Text;

            Assert.That(lastMovieWatchedTitle, Is.EqualTo(lastCreatedMovieTitle), "The Movie was not mark as watched");
        }

        [Test, Order(6)]
        public void DeleteLastCreatedMovieTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/All#all");

            var pagesLinks = driver.FindElements(By.XPath("//a[@class='page-link']"));
            pagesLinks.Last().Click();

            var moviesCollection = driver.FindElements(By.XPath("//div[@class='col-lg-4']"));
            var lastMovieDeleteButton = moviesCollection.Last().FindElement(By.XPath(".//a[@class='btn btn-danger']"));
            lastMovieDeleteButton.Click();

            driver.FindElement(By.XPath("//button[@class='btn warning']")).Click();

            var deleteMessage = driver.FindElement(By.XPath("//div[@class='toast-message']"));
            Assert.That(deleteMessage.Text, Is.EqualTo("The Movie is deleted successfully!"), "The Movie was not deleted.");

        }

        public string GenerateRandomTitle()
        {
            var random = new Random();
            return "TITLE: " + random.Next(1000, 10000);
        }

        public string GenerateRandomDescription()
        {
            var random = new Random();
            return "DESCRIPTION: " + random.Next(1000, 10000);
        }
    }
}