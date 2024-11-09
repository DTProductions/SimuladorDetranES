using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace DetranScraper {
    internal class Program {
        static void Main(string[] args) {
            var driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://e-detran.com.br/ES/Simulador_ES/");

            // Start test
            driver.FindElement(By.Id("txtNome")).SendKeys("João Silva");
            driver.FindElement(By.Id("txtCPF")).SendKeys("49223711096");
            driver.FindElement(By.Id("btnOk")).Click();

            List<Question> questions = new List<Question>();
            for(int i = 0; i < 29; i++) {
                Question question = new Question(ScrapeQuestionTitle(driver), ScrapeQuestionOptions(driver));

                driver.FindElement(By.Id("btnPular")).Click();

                var waitUntilNextQuestionIsLoaded = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                waitUntilNextQuestionIsLoaded.Until(driver => {
                        try {
                            ReadOnlyCollection<IWebElement> elements = driver.FindElements(By.Id("lblPergunta"));
                            return elements.Count > 0 && elements[0].Text != question.Title.Text;
                        }
                        catch (StaleElementReferenceException) {
                            return false;
                        }
                    });

                questions.Add(question);
            }

            SaveQuestions("", "questions.txt", questions);

            driver.Close();
            Console.WriteLine("CLOSED");
            Console.ReadLine();
        }

        static string? ExtractImageType(string dataUri) {
            string pattern = @"^data:image/([^;]+)";

            Match match = Regex.Match(dataUri, pattern);

            if (match.Success) {
                return match.Groups[1].Value;
            }
            else {
                return null;
            }
        }

        static string ExtractBase64Data(string dataUri) {
            string pattern = @"^data:image/.+;base64,(.+)$";
            Match match = Regex.Match(dataUri, pattern);

            if (match.Success) {
                return match.Groups[1].Value;
            }
            else {
                throw new ArgumentException("Invalid data URI format.");
            }
        }

        private static QuestionTitle ScrapeQuestionTitle(IWebDriver driver) {
            string titleText = driver.FindElement(By.Id("lblPergunta")).Text;
            string? imageType = null;
            string? imageBase64 = null;

            ReadOnlyCollection<IWebElement> questionImage = driver.FindElements(By.Id("imgPergunta"));
            if (questionImage.Count > 0) {
                imageType = ExtractImageType(questionImage[0].GetAttribute("src"));
                imageBase64 = ExtractBase64Data(questionImage[0].GetAttribute("src"));
            }

            return new QuestionTitle(titleText, imageBase64, imageType);
        }

        private static List<QuestionOption> ScrapeQuestionOptions(IWebDriver driver) {
            List <QuestionOption> options = new List<QuestionOption>();

            for (int j = 0; j < 4; j++) {
                string text = driver.FindElement(By.Id("lblResposta" + (j + 1))).Text;
                string? imageBase64 = null;
                string? imageType = null;

                ReadOnlyCollection<IWebElement> optionImage = driver.FindElements(By.Id("imgResposta" + (j + 1)));
                if (optionImage.Count > 0) {
                    imageType = ExtractImageType(optionImage[0].GetAttribute("src"));
                    imageBase64 = ExtractBase64Data(optionImage[0].GetAttribute("src"));
                }

                options.Add(new QuestionOption(text, imageBase64, imageType));
            }

            return options;
        }

        private static void SaveQuestions(string path, string fileName, List<Question> questions) {
            using (StreamWriter writer = new StreamWriter(path == "" ? fileName : path + "/" + fileName)) {
                for (int i = 0; i < questions.Count; i++) {
                    Question question = questions[i];

                    writer.WriteLine(question.Title.Text);
                    if (question.Title.ImageBase64 != null) {
                        string folder = (path == "" ? "Question_" : "/Question_") + (i + 1);
                        if (!Directory.Exists(folder)){
                            Directory.CreateDirectory(folder);
                        }

                        byte[] imageBytes = Convert.FromBase64String(question.Title.ImageBase64);
                        File.WriteAllBytes(folder + "/QuestionTitle." + question.Title.ImageType, imageBytes);
                    }

                    for(int j = 0; j < question.Options.Count; j++) {
                        QuestionOption option = question.Options[j];

                        writer.WriteLine((j + 1) + " " + option.Text);
                        if (option.ImageBase64 != null) {
                            string folder = (path == "" ? "Question_" : "/Question_") + (i + 1);
                            if (!Directory.Exists(folder)) {
                                Directory.CreateDirectory(folder);
                            }

                            byte[] imageBytes = Convert.FromBase64String(option.ImageBase64);
                            File.WriteAllBytes(folder + "/QuestionOption" + (j + 1) + "." + option.ImageType, imageBytes);
                        }
                    }

                    writer.WriteLine("");
                }
            }
        }
    }
}
