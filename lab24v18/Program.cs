using System;
using System.Collections.Generic;

namespace lab24
{
    // 1. Патерн Strategy (Стратегія)

    public interface INumericOperationStrategy
    {
        double Execute(double value);
    }

    public class SquareOperationStrategy : INumericOperationStrategy
    {
        public double Execute(double value) => value * value;
    }

    public class CubeOperationStrategy : INumericOperationStrategy
    {
        public double Execute(double value) => Math.Pow(value, 3);
    }

    public class SquareRootOperationStrategy : INumericOperationStrategy
    {
        public double Execute(double value)
        {
            if (value < 0) throw new ArgumentException("Неможливо обчислити корінь з від'ємного числа.");
            return Math.Sqrt(value);
        }
    }

    public class NumericProcessor
    {
        private INumericOperationStrategy _strategy;

        public NumericProcessor(INumericOperationStrategy strategy)
        {
            _strategy = strategy;
        }

        public void SetStrategy(INumericOperationStrategy strategy)
        {
            _strategy = strategy;
        }

        public double Process(double input)
        {
            return _strategy.Execute(input);
        }
    }

    // 2. Патерн Observer (Спостерігач)

    public class ResultPublisher
    {
        public event Action<double, string>? ResultCalculated;

        public void PublishResult(double result, string operationName)
        {
            ResultCalculated?.Invoke(result, operationName);
        }
    }

    public class ConsoleLoggerObserver
    {
        public void OnResultCalculated(double result, string operationName)
        {
            Console.WriteLine($"[ConsoleLogger] Операція: '{operationName}'. Результат: {result}");
        }
    }

    public class HistoryLoggerObserver
    {
        public List<string> History { get; } = new List<string>();

        public void OnResultCalculated(double result, string operationName)
        {
            History.Add($"{DateTime.Now:HH:mm:ss} | {operationName}: {result}");
        }

        public void PrintHistory()
        {
            Console.WriteLine("\n Історія обчислень");
            foreach (var item in History)
            {
                Console.WriteLine(item);
            }
        }
    }

    public class ThresholdNotifierObserver
    {
        private readonly double _threshold;

        public ThresholdNotifierObserver(double threshold)
        {
            _threshold = threshold;
        }

        public void OnResultCalculated(double result, string operationName)
        {
            if (result > _threshold)
            {
                Console.WriteLine($"[Увага! ThresholdNotifier] Результат {result} перевищує встановлений поріг {_threshold}!");
            }
        }
    }

    
    // 3. Точка входу (Main)
    
    class Program
    {
        static void Main(string[] args)
        {
            // Щоб українські літери нормально відображалися в консолі
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine(" Демонстрація LAB 24");

            // 1. Ініціалізація
            var publisher = new ResultPublisher();
            var processor = new NumericProcessor(new SquareOperationStrategy());

            // 2. Створення спостерігачів (поріг для сповіщень = 100)
            var consoleLogger = new ConsoleLoggerObserver();
            var historyLogger = new HistoryLoggerObserver();
            var thresholdNotifier = new ThresholdNotifierObserver(100.0);

            // 3. Підписка на подію
            publisher.ResultCalculated += consoleLogger.OnResultCalculated;
            publisher.ResultCalculated += historyLogger.OnResultCalculated;
            publisher.ResultCalculated += thresholdNotifier.OnResultCalculated;

            // Виконання операцій

            double num1 = 10;
            double res1 = processor.Process(num1);
            publisher.PublishResult(res1, $"Квадрат числа {num1}");

            processor.SetStrategy(new CubeOperationStrategy());
            double num2 = 5;
            double res2 = processor.Process(num2);
            publisher.PublishResult(res2, $"Куб числа {num2}");

            processor.SetStrategy(new SquareRootOperationStrategy());
            double num3 = 22500;
            double res3 = processor.Process(num3);
            publisher.PublishResult(res3, $"Квадратний корінь числа {num3}");

            // Вивід історії 
            historyLogger.PrintHistory();

            Console.WriteLine("Готово! Натисніть Enter для виходу.");
            Console.ReadLine();
        }
    }
}