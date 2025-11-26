using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace Lab7Variant
{
    // === Принцип ООП: Абстракція та Повторне використання коду ===
    // Цей клас реалізує патерн "Retry" (Повторна спроба).
    // Він є статичним (static), оскільки не зберігає власного стану,
    // а лише надає функціонал для інших частин програми.
    public static class RetryHelper
    {
        // === Принцип ООП: Узагальнення (Generics) ===
        // Метод є узагальненим <T>, що дозволяє використовувати його 
        // для будь-якого типу даних, який повертає операція (int, string, List тощо).
        // Це приклад поліморфізму на рівні компіляції.
        public static T ExecuteWithRetry<T>(
            Func<T> operation,               // Делегат: абстракція виконуваної дії
            int retryCount = 3,              // Параметр за замовчуванням
            TimeSpan initialDelay = default, 
            Func<Exception, bool> shouldRetry = null) // Делегат: абстракція логіки обробки помилок
        {
            if (initialDelay == default)
            {
                initialDelay = TimeSpan.FromSeconds(1);
            }

            for (int attempt = 0; attempt <= retryCount; attempt++)
            {
                try
                {
                    // Виклик делегата (виконання переданого методу)
                    return operation();
                }
                catch (Exception ex)
                {
                    // Логіка перевірки: чи варто повторювати спробу?
                    // Якщо це остання спроба АБО передана умова shouldRetry повернула false
                    if (attempt == retryCount || (shouldRetry != null && !shouldRetry(ex)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"ERROR: Операція не вдалася остаточно. Тип помилки: {ex.GetType().Name}. Повідомлення: {ex.Message}");
                        Console.ResetColor();
                        
                        // Прокидання винятку далі (re-throw) для обробки на рівні вище
                        throw;
                    }

                    // Реалізація експоненційної затримки (Exponential Backoff)
                    // Формула: затримка * 2 в степені спроби
                    var delay = TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * Math.Pow(2, attempt));

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"WARNING: Спроба {attempt + 1} завершилася помилкою ({ex.GetType().Name}). Повтор через {delay.TotalSeconds:F1} сек...");
                    Console.ResetColor();

                    // Призупинення виконання потоку
                    Thread.Sleep(delay);
                }
            }

            // Цей рядок коду технічно недосяжний, оскільки цикл або поверне значення, або викине помилку
            return default;
        }
    }

    // === Принцип ООП: Інкапсуляція та Моделювання поведінки ===
    // Клас імітує роботу з файловою системою.
    public class FileProcessor
    {
        // === Інкапсуляція ===
        // Приватне поле _callCount приховане від зовнішнього світу.
        // Змінювати його може лише сам клас.
        private int _callCount = 0;

        public List<string> ReadDatabaseDump(string path)
        {
            _callCount++;
            Console.WriteLine($"INFO: FileProcessor звертається до файлу (Виклик номер {_callCount})");

            // Імітація бізнес-логіки: помилка перші 2 рази
            if (_callCount <= 2)
            {
                throw new FileNotFoundException($"Файл '{path}' не знайдено.");
            }

            // Успішне повернення даних
            return new List<string> { "INSERT INTO Users VALUES (1, 'Admin');", "INSERT INTO Users VALUES (2, 'Guest');" };
        }
    }

    // === Принцип ООП: Інкапсуляція та Моделювання поведінки ===
    // Клас імітує роботу з мережею.
    public class NetworkClient
    {
        // Прихований стан лічильника викликів
        private int _callCount = 0;

        public List<string> QueryDatabase(string connectionString, string query)
        {
            _callCount++;
            Console.WriteLine($"INFO: NetworkClient відправляє запит (Виклик номер {_callCount})");

            // Імітація тимчасової мережевої помилки (перші 3 рази)
            if (_callCount <= 3)
            {
                throw new HttpRequestException("Сервер недоступний (код 503).");
            }

            return new List<string> { "User: Admin", "User: Guest" };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Налаштування кодування консолі
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== LAB 7: DEMONSTRATION OF RETRY PATTERN ===\n");

            // Створення екземплярів класів (instantiation)
            var fileProcessor = new FileProcessor();
            var networkClient = new NetworkClient();

            // === Використання делегатів (Lambda Expressions) ===
            // Визначаємо логіку фільтрації винятків.
            // Повертає true тільки для FileNotFoundException або HttpRequestException.
            Func<Exception, bool> retryLogic = (ex) => 
                ex is FileNotFoundException || ex is HttpRequestException;

            // --- Сценарій 1: Робота з FileProcessor ---
            Console.WriteLine("--- SCENARIO 1: File Processing ---");
            try
            {
                // Виклик методу через RetryHelper
                // Ми передаємо метод як анонімну функцію: () => object.Method()
                List<string> fileData = RetryHelper.ExecuteWithRetry(
                    operation: () => fileProcessor.ReadDatabaseDump("data.sql"),
                    retryCount: 3,
                    initialDelay: TimeSpan.FromSeconds(0.5),
                    shouldRetry: retryLogic
                );

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("SUCCESS: Дані успішно прочитано з файлу:");
                foreach (var line in fileData)
                {
                    Console.WriteLine($"  DATA: {line}");
                }
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                // Обробка критичної помилки, якщо всі спроби вичерпано
                Console.WriteLine($"FATAL: Не вдалося виконати операцію з файлом. {ex.Message}");
            }

            Console.WriteLine(new string('-', 40));

            // --- Сценарій 2: Робота з NetworkClient ---
            Console.WriteLine("\n--- SCENARIO 2: Network Query ---");
            try
            {
                // Тут ми ставимо retryCount = 4, оскільки NetworkClient симулює 3 помилки поспіль.
                // 3 помилки -> 4-та спроба буде успішною.
                List<string> dbData = RetryHelper.ExecuteWithRetry(
                    operation: () => networkClient.QueryDatabase("Server=SQL;", "SELECT * FROM Users"),
                    retryCount: 4, 
                    initialDelay: TimeSpan.FromSeconds(0.5),
                    shouldRetry: retryLogic
                );

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("SUCCESS: Дані успішно отримано з БД:");
                foreach (var line in dbData)
                {
                    Console.WriteLine($"  DATA: {line}");
                }
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL: Не вдалося виконати запит до мережі. {ex.Message}");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для завершення...");
            Console.ReadKey();
        }
    }
}