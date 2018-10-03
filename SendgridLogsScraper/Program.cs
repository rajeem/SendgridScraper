using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SendgridLogsScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            using (IWebDriver driver = new FirefoxDriver())
            {
                var js = (IJavaScriptExecutor)driver;
                var timeout = new TimeSpan(0, 1, 0);

                driver.Navigate().GoToUrl("https://app.sendgrid.com/login?redirect_to=%2Femail_activity");
                driver.FindElement(By.Id("usernameContainer-input-id")).SendKeys(ConfigurationManager.AppSettings["sendgridUsername"]);
                driver.FindElement(By.Id("passwordContainer-input-id")).SendKeys(ConfigurationManager.AppSettings["sendgridPassword"]);
                driver.FindElement(By.XPath("//button[@data-role='login-btn']")).Click();

                new WebDriverWait(driver, timeout).Until(ExpectedConditions.ElementExists(By.XPath("//div[contains(@class, 'filter-search-basic')]//button[@data-role='searchButton']")));
                js.ExecuteScript(@"
                            document.querySelector('.filter-search-basic button[data-role=""searchButton""').click();
                        ");

                new WebDriverWait(driver, timeout).Until(x => (bool)((IJavaScriptExecutor)x).ExecuteScript("return jQuery.active == 0"));

                var htmlResult = driver.FindElement(By.Id("email_stats")).GetAttribute("outerHTML");
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlResult);

                var s = "";
                foreach (var tr in htmlDoc.DocumentNode.SelectNodes("//tbody/tr"))
                {
                    for (int i = 0; i < tr.ChildNodes.Count; i++)
                    {
                        s += tr.ChildNodes[i].InnerText.Trim() + "\r\n";
                    }
                    s += "-------------------------------------------------------\r\n";
                }

                File.WriteAllText("sendgridlogs" +  DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt", s);
            }
        }
    }
}
