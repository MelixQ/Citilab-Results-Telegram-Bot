using Citilab_Results_Telegram_Bot.TestClientData;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Citilab_Results_Telegram_Bot.WebDriver;

public class WebDriver
{
    public static async Task<string> GetResultsFromCitilabAsync(ClientBaseRecord client)
    {
        await Task.Run(() =>
        {
            var service = ChromeDriverService.CreateDefaultService();
            var driver = new ChromeDriver(service);
            driver.Navigate().GoToUrl("https://my.citilab.ru/VlprtH8uK0NaR5a2CwmC_view/");

            var citySelectElement = driver.FindElement(By.Id("ctl00_MainContentHolder_laboratory_ddl"));
            var citySelect = new SelectElement(citySelectElement);
            citySelect.SelectByValue(client.Lab);

            var requestIdElement = driver.FindElement(By.Id("ctl00_MainContentHolder_requestId_tbx"));
            requestIdElement.SendKeys(client.RequestId);

            var lastNameElement = driver.FindElement(By.Id("ctl00_MainContentHolder_lastName_tbx"));
            lastNameElement.SendKeys(client.LastName);

            var birthDayElement = driver.FindElement(By.Id("ctl00_MainContentHolder_birthDay_tbx"));
            var birthMonthElement = driver.FindElement(By.Id("ctl00_MainContentHolder_birthMonth_tbx"));
            var birthYearElement = driver.FindElement(By.Id("ctl00_MainContentHolder_birthYear_tbx"));
            birthDayElement.SendKeys(client.BirthDay);
            birthMonthElement.SendKeys(client.BirthMonth);
            birthYearElement.SendKeys(client.BirthYear);

            var checkBoxElement = driver.FindElement(By.Id("soglCheck"));
            checkBoxElement.Click();

            var submitButtonElement = driver.FindElement(By.Id("ctl00_MainContentHolder_results_btn"));
            submitButtonElement.Click();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            var fileLinkElement = driver.FindElement(By.Id("ctl00_MainContentHolder_downloadPDF_link_1"));

            fileLinkElement.Click();

            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                       @$"\Downloads\{client.RequestId}.pdf";

            new WebDriverWait(driver, TimeSpan.FromSeconds(60))
                .Until(d => File.Exists(path));

            driver.Quit();
        });
        
        var downloadFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                              @$"\Downloads\{client.RequestId}.pdf";

        Console.WriteLine(downloadFilePath + "\nFILE PATH DOWNLOADED");
        return downloadFilePath;
    }
}