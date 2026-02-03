using System;
using System.Collections.Generic;

namespace Lab21
{
    // 1. Інтерфейс Стратегії (Абстракція)
    // Цей інтерфейс дозволяє DeliveryService не знати про деталі розрахунків.
    public interface IShippingStrategy
    {
        decimal CalculateCost(decimal distance, decimal weight);
    }

    // 2. Реалізації Стратегій

    // Стандартна доставка
    public class StandardShippingStrategy : IShippingStrategy
    {
        public decimal CalculateCost(decimal distance, decimal weight)
        {
            // distance * 1.5 + weight * 0.5
            return (distance * 1.5m) + (weight * 0.5m);
        }
    }

    // Експрес доставка
    public class ExpressShippingStrategy : IShippingStrategy
    {
        public decimal CalculateCost(decimal distance, decimal weight)
        {
            // (distance * 2.5 + weight * 1.0) + 50 фіксована доплата
            return (distance * 2.5m) + (weight * 1.0m) + 50m;
        }
    }

    // Міжнародна доставка
    public class InternationalShippingStrategy : IShippingStrategy
    {
        public decimal CalculateCost(decimal distance, decimal weight)
        {
            // distance * 5.0 + weight * 2.0 + 15% податок
            decimal baseCost = (distance * 5.0m) + (weight * 2.0m);
            return baseCost * 1.15m; // +15%
        }
    }

    // --- Демонстрація OCP: Додаємо новий клас без зміни існуючої логіки розрахунку ---
    // Нічна доставка
    public class NightShippingStrategy : IShippingStrategy
    {
        public decimal CalculateCost(decimal distance, decimal weight)
        {
            // Логіка як у стандартної, але + фіксована націнка за нічний час (наприклад, 20 грн)
            return (distance * 1.5m) + (weight * 0.5m) + 20m;
        }
    }

    // 3. Фабрика (Factory Method)
    // Відповідає за створення об'єктів. Це єдине місце, яке доведеться змінити при додаванні нової стратегії.
    public static class ShippingStrategyFactory
    {
        public static IShippingStrategy CreateStrategy(string deliveryType)
        {
            // Приводимо до нижнього регістру для зручності
            switch (deliveryType.ToLower())
            {
                case "standard":
                    return new StandardShippingStrategy();
                case "express":
                    return new ExpressShippingStrategy();
                case "international":
                    return new InternationalShippingStrategy();
                // Додана підтримка нової стратегії
                case "night":
                    return new NightShippingStrategy(); 
                default:
                    throw new ArgumentException("Невідомий тип доставки");
            }
        }
    }

    // 4. Сервіс Доставки (Context)
    // Цей клас закритий для модифікації. Ми не змінюємо його код, коли додаємо нові типи доставки.
    public class DeliveryService
    {
        // Метод приймає абстракцію (IShippingStrategy), а не конкретний клас
        public decimal CalculateDeliveryCost(decimal distance, decimal weight, IShippingStrategy strategy)
        {
            if (strategy == null)
            {
                throw new ArgumentNullException(nameof(strategy));
            }
            
            return strategy.CalculateCost(distance, weight);
        }
    }

    // 5. Головний метод (User Interface)
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; // Для коректного відображення кирилиці
            DeliveryService deliveryService = new DeliveryService();

            while (true)
            {
                Console.WriteLine("\n--- Розрахунок вартості доставки ---");
                Console.WriteLine("Оберіть тип доставки: Standard, Express, International, Night (або 'exit' для виходу)");
                Console.Write("Ваш вибір: ");
                string typeInput = Console.ReadLine();

                if (typeInput?.ToLower() == "exit") break;

                try
                {
                    // 1. Отримуємо стратегію через фабрику
                    IShippingStrategy strategy = ShippingStrategyFactory.CreateStrategy(typeInput);

                    // 2. Вводимо дані
                    Console.Write("Введіть відстань (км): ");
                    if (!decimal.TryParse(Console.ReadLine(), out decimal distance))
                    {
                        Console.WriteLine("Помилка: Некоректне число для відстані.");
                        continue;
                    }

                    Console.Write("Введіть вагу (кг): ");
                    if (!decimal.TryParse(Console.ReadLine(), out decimal weight))
                    {
                        Console.WriteLine("Помилка: Некоректне число для ваги.");
                        continue;
                    }

                    // 3. Розрахунок через сервіс (Сервіс не знає, яка саме це стратегія, він просто викликає CalculateCost)
                    decimal cost = deliveryService.CalculateDeliveryCost(distance, weight, strategy);

                    Console.WriteLine($"-----------------------------------");
                    Console.WriteLine($"Тип доставки: {typeInput}");
                    Console.WriteLine($"Вартість: {cost:F2} грн");
                    Console.WriteLine($"-----------------------------------");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Помилка: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Критична помилка: {ex.Message}");
                }
            }
        }
    }
}
