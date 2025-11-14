using System;
using System.Collections.Generic;
using System.Linq; // Необхідно для LINQ

namespace lab6_vN // Замініть N на ваш номер варіанту
{
    // Клас, що описує автомобіль (за вимогою)
    public class Car
    {
        public string Model { get; set; }
        public int Mileage { get; set; } // Пробіг у км
        public double FuelConsumption { get; set; } // Витрата пального (л/100км)

        public Car(string model, int mileage, double fuelConsumption)
        {
            Model = model;
            Mileage = mileage;
            FuelConsumption = fuelConsumption;
        }

        // Перевизначення для зручного виводу в консоль
        public override string ToString()
        {
            return $"{Model} (Пробіг: {Mileage} км, Витрата: {FuelConsumption:F1} л/100км)";
        }
    }

    class Program
    {
        // 1. Оголошення власного делегату для арифметичних операцій
        // Цей делегат може вказувати на будь-який метод,
        // що приймає два параметри double і повертає double.
        public delegate double ArithmeticOperation(double a, double b);

        static void Main(string[] args)
        {
            // Встановлюємо кодування для коректного відображення української мови
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("--- Лабораторна робота 6: Делегати, Лямбда та LINQ ---");

            // 2. Приклади використання анонімних методів (старий синтаксис C# 2.0)
            Console.WriteLine("\n## 2. Анонімні методи (з власним делегатом) ##");

            // Використання 'delegate' для створення екземпляра делегата "на місці"
            ArithmeticOperation add = delegate(double x, double y)
            {
                return x + y;
            };
            Console.WriteLine($"Додавання (анонімний метод): 10 + 5 = {add(10, 5)}");

            // 3. Приклади використання лямбда-виразів (сучасний синтаксис C# 3.0+)
            Console.WriteLine("\n## 3. Лямбда-вирази (з власним делегатом) ##");

            // Лямбда-вираз '=>' є скороченим записом анонімного методу
            ArithmeticOperation multiply = (x, y) => x * y;
            Console.WriteLine($"Множення (лямбда): 10 * 5 = {multiply(10, 5)}");

            // Лямбда-вираз з тілом (кількома операторами)
            ArithmeticOperation divide = (x, y) =>
            {
                if (y == 0)
                {
                    Console.WriteLine("Ділення на нуль!");
                    return 0;
                }
                return x / y;
            };
            Console.WriteLine($"Ділення (лямбда): 10 / 5 = {divide(10, 5)}");

            // 4. Застосування вбудованих делегатів (Func, Action, Predicate)
            Console.WriteLine("\n## 4. Вбудовані делегати ##");

            // Action<>: вказує на метод, що нічого не повертає (void).
            // Приймає 1 параметр string.
            Action<string> printMessage = message => Console.WriteLine($"[Action]: {message}");
            printMessage("Це повідомлення виведено за допомогою Action<string>.");

            // Func<>: вказує на метод, що повертає значення.
            // Останній тип (double) - це тип повернення. Перші два (int, int) - параметри.
            Func<int, int, double> calculateAverage = (a, b) => (a + b) / 2.0;
            Console.WriteLine($"[Func]: Середнє арифметичне 7 і 8 = {calculateAverage(7, 8)}");

            // Predicate<>: спеціалізований Func<T, bool>, завжди повертає bool.
            // Використовується для перевірки умови.
            Predicate<int> isPositive = number => number > 0;
            Console.WriteLine($"[Predicate]: Число 150 додатнє? {isPositive(150)}");
            Console.WriteLine($"[Predicate]: Число -10 додатнє? {isPositive(-10)}");

            // 5. Обробка колекції об’єктів (List<Car>) за допомогою LINQ
            Console.WriteLine("\n## 5. LINQ та операції з колекцією Car ##");

            List<Car> carPark = new List<Car>
            {
                new Car("Tesla Model S", 80000, 0.0), // Електро
                new Car("Toyota Camry", 120000, 8.5),
                new Car("BMW X5", 45000, 11.2),
                new Car("Ford Focus", 150000, 7.1),
                new Car("VW Golf", 95000, 6.8),
                new Car("Audi A6", 105000, 9.0)
            };

            Console.WriteLine("Початковий список автомобілів:");
            // Використання методу ForEach, який приймає Action<Car>
            carPark.ForEach(car => Console.WriteLine(car));

            // Завдання 1: Відбір за пробігом > 100 000 км (Where)
            // Where використовує Predicate<Car> (або, точніше, Func<Car, bool>)
            Console.WriteLine("\n-> Автомобілі з пробігом > 100 000 км (Where):");
            var highMileageCars = carPark.Where(car => car.Mileage > 100000);
            foreach (var car in highMileageCars)
            {
                Console.WriteLine(car);
            }

            // Завдання 2: Розрахунок середньої витрати пального (Average)
            // Виключаємо електромобілі (де витрата = 0) з розрахунку
            double averageConsumption = carPark
                .Where(c => c.FuelConsumption > 0) // Фільтрація
                .Average(c => c.FuelConsumption); // Лямбда для вибору поля
            Console.WriteLine($"\n-> Середня витрата пального (для авто з ДВЗ): {averageConsumption:F2} л/100км");

            // Завдання 3: Пошук автомобіля з мінімальною витратою (OrderBy + FirstOrDefault)
            // (Знову ж, ігноруємо 0.0 для електро)
            var carWithMinConsumption = carPark
                .Where(c => c.FuelConsumption > 0)
                .OrderBy(c => c.FuelConsumption) // Сортування за зростанням
                .FirstOrDefault(); // Беремо перший елемент
            Console.WriteLine("\n-> Автомобіль з мінімальною витратою пального (крім електро):");
            Console.WriteLine(carWithMinConsumption);

            // Додатково: Проєкція (Select) та сортування (OrderByDescending)
            Console.WriteLine("\n-> Моделі авто, відсортовані за спаданням пробігу (Select + OrderBy):");
            var sortedModels = carPark
                .OrderByDescending(c => c.Mileage)
                .Select(c => $"Модель: {c.Model} (Пробіг: {c.Mileage} км)"); // Проєкція в новий тип (рядок)

            foreach (var modelInfo in sortedModels)
            {
                Console.WriteLine(modelInfo);
            }

            // Додатково: Aggregate для розрахунку загального пробігу
            int totalMileage = carPark.Aggregate(0, (total, nextCar) => total + nextCar.Mileage);
            Console.WriteLine($"\n-> Загальний пробіг автопарку (Aggregate): {totalMileage} км");

            Console.WriteLine("\n--- Завершення роботи ---");
        }
    }
}
