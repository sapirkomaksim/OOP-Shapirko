using System;
using System.IO;

namespace lab25
{
    // ====================================================================
    // 1. Патерн Factory Method (Фабричний метод)
    // ====================================================================
    

    public interface ILogger
    {
        void Log(string message);
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[ConsoleLogger] {message}");
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string _filePath = "log.txt";

        public void Log(string message)
        {
            // Записуємо у файл і дублюємо в консоль для наочності в лабораторній
            File.AppendAllText(_filePath, $"{DateTime.Now}: {message}\n");
            Console.WriteLine($"[FileLogger] (записано у файл {_filePath}): {message}");
        }
    }

    public abstract class LoggerFactory
    {
        public abstract ILogger CreateLogger();
    }

    public class ConsoleLoggerFactory : LoggerFactory
    {
        public override ILogger CreateLogger() => new ConsoleLogger();
    }

    public class FileLoggerFactory : LoggerFactory
    {
        public override ILogger CreateLogger() => new FileLogger();
    }

    // ====================================================================
    // 2. Патерн Singleton (Одинак)
    // ====================================================================
    

    public class LoggerManager
    {
        private static LoggerManager? _instance;
        private LoggerFactory? _factory;

        // Приватний конструктор, щоб не можна було створити об'єкт через new
        private LoggerManager() { }

        public static LoggerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoggerManager();
                }
                return _instance;
            }
        }

        // Метод для динамічної зміни фабрики логерів
        public void SetFactory(LoggerFactory factory)
        {
            _factory = factory;
        }

        public void LogMessage(string message)
        {
            if (_factory == null)
            {
                Console.WriteLine("[Попередження] Фабрика логерів не встановлена!");
                return;
            }

            ILogger logger = _factory.CreateLogger();
            logger.Log(message);
        }
    }

    // ====================================================================
    // 3. Патерн Strategy (Стратегія)
    // ====================================================================
    

    public interface IDataProcessorStrategy
    {
        string Process(string data);
    }

    public class EncryptDataStrategy : IDataProcessorStrategy
    {
        public string Process(string data)
        {
            return $"***ENCRYPTED({data})***";
        }
    }

    public class CompressDataStrategy : IDataProcessorStrategy
    {
        public string Process(string data)
        {
            return $"zip[{data}]";
        }
    }

    public class DataContext
    {
        private IDataProcessorStrategy _strategy;

        public DataContext(IDataProcessorStrategy strategy)
        {
            _strategy = strategy;
        }

        public void SetStrategy(IDataProcessorStrategy strategy)
        {
            _strategy = strategy;
        }

        public string ExecuteStrategy(string data)
        {
            return _strategy.Process(data);
        }
    }

    // ====================================================================
    // 4. Патерн Observer (Спостерігач)
    // ====================================================================
    

    public class DataPublisher
    {
        public event Action<string>? DataProcessed;

        public void PublishDataProcessed(string processedData)
        {
            DataProcessed?.Invoke(processedData);
        }
    }

    public class ProcessingLoggerObserver
    {
        // Цей метод підписується на подію і викликає Singleton для логування
        public void OnDataProcessed(string processedData)
        {
            // Використовуємо глобальну точку доступу до логера
            LoggerManager.Instance.LogMessage($"Отримано оброблені дані: {processedData}");
        }
    }

    // ====================================================================
    // Точка входу (Main)
    // ====================================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=================================================");
            Console.WriteLine("Сценарій 1: Повна інтеграція (ConsoleLogger + Encrypt)");
            Console.WriteLine("=================================================");

            // 1. Ініціалізуємо LoggerManager (Singleton) з ConsoleLoggerFactory (Factory)
            LoggerManager.Instance.SetFactory(new ConsoleLoggerFactory());

            // 2. Створюємо DataContext зі стратегією шифрування (Strategy)
            var dataContext = new DataContext(new EncryptDataStrategy());

            // 3. Створюємо Publisher та Observer (Observer)
            var publisher = new DataPublisher();
            var observer = new ProcessingLoggerObserver();

            // 4. Підписуємо Observer на подію від Publisher
            publisher.DataProcessed += observer.OnDataProcessed;

            // 5. Виконуємо обробку та публікацію даних
            string rawData1 = "СекретнийПароль";
            string processedData1 = dataContext.ExecuteStrategy(rawData1);
            publisher.PublishDataProcessed(processedData1);


            Console.WriteLine("\n=================================================");
            Console.WriteLine("Сценарій 2: Динамічна зміна логера (на FileLogger)");
            Console.WriteLine("=================================================");

            // Змінюємо фабрику в існуючому Singleton об'єкті
            LoggerManager.Instance.SetFactory(new FileLoggerFactory());

            // Обробляємо і публікуємо нові дані
            string rawData2 = "ФінансовийЗвіт";
            string processedData2 = dataContext.ExecuteStrategy(rawData2);
            publisher.PublishDataProcessed(processedData2);


            Console.WriteLine("\n=================================================");
            Console.WriteLine("Сценарій 3: Динамічна зміна стратегії (на Compress)");
            Console.WriteLine("=================================================");

            // Змінюємо стратегію обробки даних
            dataContext.SetStrategy(new CompressDataStrategy());
            
            // Повертаємо консольний логер для наочності, хоча можемо залишити й файловий
            LoggerManager.Instance.SetFactory(new ConsoleLoggerFactory());

            // Обробляємо і публікуємо дані новою стратегією
            string rawData3 = "ВеличезнийМасивДаних";
            string processedData3 = dataContext.ExecuteStrategy(rawData3);
            publisher.PublishDataProcessed(processedData3);

            Console.WriteLine("\n=================================================");
            Console.WriteLine("Всі сценарії успішно виконані. Натисніть Enter...");
            Console.ReadLine();
        }
    }
}