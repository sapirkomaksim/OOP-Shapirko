using System;
using System.Collections.Generic;

namespace IndependentWork16
{
    // 1. "поганий" клас (Порушує SRP)
    public class BadEmailCampaignSender
    {
        public void RunCampaign(string campaignName)
        {
            Console.WriteLine("Отримання списку отримувачів з бази даних...");
            var recipients = new List<string> { "user1@example.com", "user2@example.com" };
            string emailBody = $"<h1>Вітаємо!</h1><p>Це лист з кампанії {campaignName}.</p>";

            foreach (var email in recipients)
            {
                Console.WriteLine($"Відправка листа на {email} через SMTP...");
            }
            Console.WriteLine($"[LOG]: Кампанія '{campaignName}' успішно завершена.");
        }
    }
    // 2. Інтерфейси та їх реалізація (Після рефакторингу)
    public interface IRecipientProvider { List<string> GetRecipients(); }
    public interface IEmailContentBuilder { string BuildContent(string campaignName); }
    public interface IEmailSender { void Send(string email, string content); }
    public interface ILogger { void Log(string message); }

    public class MockRecipientProvider : IRecipientProvider
    {
        public List<string> GetRecipients() => new List<string> { "alice@example.com", "bob@example.com" };
    }

    public class HtmlEmailContentBuilder : IEmailContentBuilder
    {
        public string BuildContent(string campaignName) => $"<h1>Новини: {campaignName}</h1><p>Спеціальна пропозиція для вас!</p>";
    }

    public class MockSmtpEmailSender : IEmailSender
    {
        public void Send(string email, string content) => Console.WriteLine($"[SMTP] Відправлено на {email}. Довжина листа: {content.Length} символів.");
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(string message) => Console.WriteLine($"[LOG {DateTime.Now:HH:mm:ss}]: {message}");
    }
    // 3. Новий сервіс (використовує DIP)
    public class EmailCampaignService
    {
        private readonly IRecipientProvider _recipientProvider;
        private readonly IEmailContentBuilder _contentBuilder;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public EmailCampaignService(
            IRecipientProvider recipientProvider,
            IEmailContentBuilder contentBuilder,
            IEmailSender emailSender,
            ILogger logger)
        {
            _recipientProvider = recipientProvider;
            _contentBuilder = contentBuilder;
            _emailSender = emailSender;
            _logger = logger;
        }

        public void ExecuteCampaign(string campaignName)
        {
            _logger.Log($"Початок кампанії: {campaignName}");
            var recipients = _recipientProvider.GetRecipients();
            var content = _contentBuilder.BuildContent(campaignName);

            foreach (var email in recipients)
            {
                _emailSender.Send(email, content);
            }
            _logger.Log($"Кампанія '{campaignName}' успішно надіслана {recipients.Count} отримувачам.");
        }
    }
    // 4. Точка входу
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(" До рефакторингу");
            var badSender = new BadEmailCampaignSender();
            badSender.RunCampaign("Літній Розпродаж");

            Console.WriteLine("\n Після рефакторингу (SRP & DIP)");
            var campaignService = new EmailCampaignService(
                new MockRecipientProvider(),
                new HtmlEmailContentBuilder(),
                new MockSmtpEmailSender(),
                new ConsoleLogger()
            );
            
            campaignService.ExecuteCampaign("Осіння Знижка");
            
            Console.ReadLine();
        }
    }
}