using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

// --- 1. Власні Винятки (Custom Exceptions) ---
// Демонстрація УСпадкування (Inheritance):
// Кожен клас винятку наслідує базовий клас 'Exception',
// що дозволяє створювати власні, специфічні типи помилок (Поліморфізм).

/// <summary>
/// Власний тип винятку. Демонструє **Успадкування**:
/// цей клас наслідує базовий клас `Exception` для створення
/// специфічної помилки предметної області.
/// </summary>
public class InvalidDimensionsException : Exception
{
    public InvalidDimensionsException(string message) : base(message) { }
}

/// <summary>
/// Демонструє **Успадкування** від `Exception`.
/// </summary>
public class ItemTooLargeException : Exception
{
    public ItemTooLargeException(string message) : base(message) { }
}

/// <summary>
/// Демонструє **Успадкування** від `Exception`.
/// </summary>
public class NoSpaceException : Exception
{
    public NoSpaceException(string message) : base(message) { }
}


// --- 2. Сутність: Предмет (BoxItem) ---

/// <summary>
/// Сутність 'Предмет'. Цей клас є моделлю даних.
/// Демонструє **Інкапсуляцію**: дані (Name, Volume) захищені
/// (доступні лише для читання після створення).
/// Демонструє **Абстракцію**: реалізує інтерфейс `IComparable<T>`.
/// </summary>
public class BoxItem : IComparable<BoxItem>
{
    // ІНКАПСУЛЯЦІЯ: Властивості 'get-only' гарантують незмінність (immutability)
    // стану об'єкта після його створення.
    public string Name { get; }
    public double Volume { get; }

    public BoxItem(string name, double volume)
    {
        // ІНКАПСУЛЯЦІЯ: Конструктор контролює вхідні дані
        // і гарантує, що об'єкт не буде створено у невалідному стані.
        if (volume <= 0)
        {
            throw new InvalidDimensionsException($"Об'єм предмета '{name}' має бути додатним.");
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Назва предмета не може бути порожньою.");
        }
        Name = name;
        Volume = volume;
    }

    /// <summary>
    /// Реалізація методу з інтерфейсу `IComparable<T>`.
    /// Це приклад **Поліморфізму** (через інтерфейс):
    /// `SortedSet` (наша PriorityQueue) буде викликати *саме* цю
    // реалізацію для сортування об'єктів BoxItem.
    /// </summary>
    public int CompareTo(BoxItem? other)
    {
        if (other == null) return -1;
        
        // Логіка порівняння для пріоритетної черги
        int volumeComparison = other.Volume.CompareTo(this.Volume);
        if (volumeComparison != 0)
        {
            return volumeComparison;
        }
        return string.Compare(this.Name, other.Name, StringComparison.Ordinal);
    }

    /// <summary>
    /// Перевизначення методу `ToString()`.
    /// Приклад **Поліморфізму** (ad-hoc / overriding):
    /// Будь-який код, що викликає `ToString()` для `BoxItem` (напр., `Console.WriteLine`),
    /// буде використовувати цю кастомну реалізацію, а не базову з `object`.
    /// </summary>
    public override string ToString() => $"{Name} ({Volume} м³)";
}


// --- 3. Сутність: Коробка (Box) ---

/// <summary>
/// Сутність 'Коробка'.
/// Демонструє **Інкапсуляцію**: клас приховує свій внутрішній стан
/// (список `_items`) і надає контрольований доступ до нього через
/// публічні методи (`TryAddItem`) та властивості (`CurrentVolume`, `RemainingSpace`).
/// </summary>
public class Box
{
    // ІНКАПСУЛЯЦІЯ: Властивості лише для читання.
    public string Id { get; }
    public double Capacity { get; }
    
    // ІНКАПСУЛЯЦІЯ: Це поле є *приватним*.
    // Світ ззовні не може напряму маніпулювати цим списком.
    private readonly List<BoxItem> _items = new List<BoxItem>();
    
    // АГРЕГАЦІЯ (Aggregation): Коробка "має" (агрегує) список предметів.
    // Це слабкий зв'язок "has-a", оскільки предмети (`BoxItem`)
    // створюються ззовні і можуть існувати незалежно від Коробки.
    public IReadOnlyCollection<BoxItem> PackedItems => _items.AsReadOnly();

    public Box(string id, double capacity)
    {
        // ІНКАПСУЛЯЦІЯ: Контроль валідності стану в конструкторі.
        if (capacity <= 0)
        {
            throw new InvalidDimensionsException($"Місткість коробки '{id}' має бути додатною.");
        }
        Id = id;
        Capacity = capacity;
    }

    // --- Обчислення з колекціями (Поведінка класу) ---

    // ІНКАПСУЛЯЦІЯ: Обчислювані властивості, які приховують логіку
    // обчислень від клієнта. Клієнту не потрібно знати, як
    // розраховується об'єм (через Sum() чи інакше).
    
    /// <summary>
    /// Обчислювана властивість. Приклад **Інкапсуляції**.
    /// </summary>
    public double CurrentVolume => _items.Sum(item => item.Volume);

    /// <summary>
    /// Обчислювана властивість. Приклад **Інкапсуляції**.
    /// </summary>
    public double RemainingSpace => Capacity - CurrentVolume;

    /// <summary>
    /// Обчислювана властивість. Приклад **Інкапсуляції**.
    /// </summary>
    public double FillPercentage => (CurrentVolume / Capacity) * 100.0;

    /// <summary>
    /// Публічний метод, що є частиною **інтерфейсу** класу.
    /// Це єдиний спосіб додати предмет, що гарантує
    /// дотримання бізнес-правил (перевірка `RemainingSpace`).
    /// </summary>
    public bool TryAddItem(BoxItem item)
    {
        // ІНКАПСУЛЯЦІЯ: Логіка перевірки прихована всередині методу.
        if (item.Volume <= RemainingSpace)
        {
            _items.Add(item);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Демонстрація **Поліморфізму** (Overriding).
    /// </summary>
    public override string ToString() => 
        $"Коробка '{Id}' ({CurrentVolume:F2} / {Capacity:F2} м³) - Заповненість: {FillPercentage:F1}%";
}


// --- 4. Головна сутність: Планувальник пакування (PackPlanner) ---

/// <summary>
/// Керуючий клас.
/// Демонструє **Композицію** (Composition): `PackPlanner` *володіє*
/// списками `_boxes` та `_itemQueue`. Їхній життєвий цикл керується
/// цим класом.
/// Демонструє **Абстракцію**: `PackPlanner` *абстрагує* (приховує)
/// складний процес пакування за простими методами (`PackItemsGreedy`).
/// </summary>
public class PackPlanner
{
    // КОМПОЗИЦІЯ (Composition): `PackPlanner` "володіє" цими колекціями.
    // Це сильний зв'язок "has-a".
    // ІНКАПСУЛЯЦІЯ: Колекції приватні.
    private readonly List<Box> _boxes = new List<Box>();

    // УЗАГАЛЬНЕННЯ (Generics):
    // Використання узагальненого компоненту `SortedSet<T>`.
    // Ми типізуємо його `BoxItem` (T = BoxItem),
    // що дає нам безпеку типів під час компіляції.
    private readonly SortedSet<BoxItem> _itemQueue = new SortedSet<BoxItem>();

    // ІНКАПСУЛЯЦІЯ: Надання доступу "лише для читання"
    // до внутрішніх колекцій.
    public IReadOnlyCollection<Box> Boxes => _boxes.AsReadOnly();
    public IReadOnlyCollection<BoxItem> ItemQueue => _itemQueue;

    /// <summary>
    /// Публічний метод (**Інтерфейс** класу).
    /// </summary>
    public void AddBox(Box box)
    {
        _boxes.Add(box);
    }

    /// <summary>
    /// Публічний метод (**Інтерфейс** класу).
    /// Приховує логіку валідації (Інкапсуляція).
    /// </summary>
    public void AddItemToQueue(BoxItem item)
    {
        // ІНКАПСУЛЯЦІЯ: Перевірка бізнес-логіки прихована від клієнта.
        if (_boxes.Any() && _boxes.All(box => item.Volume > box.Capacity))
        {
            throw new ItemTooLargeException(
                $"Предмет '{item.Name}' ({item.Volume} м³) завеликий для БУДЬ-ЯКОЇ коробки."
            );
        }
        
        _itemQueue.Add(item);
    }

    /// <summary>
    /// Метод, що реалізує *поведінку* класу.
    /// Демонструє **Абстракцію**: клієнт (Program.cs) просто викликає
    /// `PackItemsGreedy()`, не знаючи про складність алгоритму,
    /// що реалізований всередині.
    /// </summary>
    public void PackItemsGreedy()
    {
        Console.WriteLine("\n--- 📦 Початок 'жадібного' пакування ---");
        if (!_boxes.Any())
        {
            Console.WriteLine("Помилка: Немає коробок для пакування.");
            return;
        }

        var itemsToRepack = new List<BoxItem>();

        while (_itemQueue.Count > 0)
        {
            // Використання поведінки, наданої `IComparable` (Поліморфізм)
            BoxItem currentItem = _itemQueue.First(); 
            _itemQueue.Remove(currentItem);

            bool isPacked = false;
            
            // Взаємодія між об'єктами (Planner -> Box)
            foreach (var box in _boxes.OrderBy(b => b.RemainingSpace))
            {
                // Звернення до публічного інтерфейсу об'єкта 'Box'
                if (box.TryAddItem(currentItem))
                {
                    Console.WriteLine($"✔ Предмет '{currentItem.Name}' запаковано в коробку '{box.Id}'.");
                    isPacked = true;
                    break; 
                }
            }

            if (!isPacked)
            {
                Console.WriteLine($"⚠ Увага! Для предмета '{currentItem.Name}' не знайдено місця.");
                itemsToRepack.Add(currentItem);
            }
        }

        if (itemsToRepack.Any())
        {
            foreach (var item in itemsToRepack)
            {
                _itemQueue.Add(item);
            }
            
            throw new NoSpaceException("Не вдалося запакувати всі предмети. Деякі повернуто в чергу.");
        }
        
        Console.WriteLine("--- ✅ Пакування завершено ---");
    }

    /// <summary>
    /// Метод-звіт, що демонструє обчислення (Поведінка).
    /// </summary>
    public void PrintPackingReport()
    {
        Console.WriteLine("\n--- 📊 Звіт пакування ---");
        if (!_boxes.Any())
        {
            Console.WriteLine("Немає коробок для звіту.");
            return;
        }
        
        // Тут ми використовуємо ПОЛІМОРФІЗМ:
        // `Console.WriteLine(box)` автоматично викликає
        // перевизначений нами `box.ToString()`.
        foreach (var box in _boxes)
        {
            Console.WriteLine(box); // Виклик box.ToString()
            if (box.PackedItems.Any())
            {
                Console.WriteLine("   Вміст:");
                foreach (var item in box.PackedItems)
                {
                    Console.WriteLine($"   - {item}"); // Виклик item.ToString()
                }
            }
        }

        // Обчислення на основі стану інкапсульованих об'єктів
        double totalCapacity = _boxes.Sum(b => b.Capacity);
        double totalUsedVolume = _boxes.Sum(b => b.CurrentVolume);
        
        double overallFillPercentage = totalCapacity > 0 
            ? (totalUsedVolume / totalCapacity) * 100.0 
            : 0;

        Console.WriteLine("----------------------------------");
        Console.WriteLine($"Загальна місткість:  {totalCapacity:F2} м³");
        Console.WriteLine($"Загальний використаний об'єм: {totalUsedVolume:F2} м³");
        Console.WriteLine($"Загальна заповненість: {overallFillPercentage:F1}%");

        if (_itemQueue.Any())
        {
            Console.WriteLine("\nПредмети, що очікують пакування:");
            foreach (var item in _itemQueue)
            {
                Console.WriteLine($"- {item}"); // Виклик item.ToString()
            }
        }
    }
}


// --- 5. Демонстрація (Program.cs) ---

/// <summary>
/// Клієнтський код (Client).
/// Цей клас нічого не знає про *внутрішню* реалізацію `PackPlanner` чи `Box`.
/// Він працює виключно з їхніми **публічними інтерфейсами** (методами
/// та властивостями), що є ключовим принципом **Інкапсуляції**.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Лабораторна робота 5 (Варіант 19): Пакування на складі ===");

        // Створення "кореневого" об'єкта (див. Композиція)
        var planner = new PackPlanner();

        // --- 1. Налаштування (Створення коробок) ---
        try
        {
            // Використання публічного інтерфейсу `PackPlanner`
            planner.AddBox(new Box("A-Мала", 10.0));
            planner.AddBox(new Box("B-Середня", 25.0));
            planner.AddBox(new Box("C-Велика", 50.0));
        }
        // Обробка винятків (використання Успадкування:
        // ми ловимо наш кастомний виняток)
        catch (InvalidDimensionsException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nПОМИЛКА ІНІЦІАЛІЗАЦІЇ: {ex.Message}");
            Console.ResetColor();
        }

        // --- 2. Демонстрація обробки винятків ---
        Console.WriteLine("\n--- Демонстрація обробки винятків ---");

        // Блок Try-Catch 1: Обробка кастомного винятку
        try
        {
            var invalidItem = new BoxItem("Брак", -100.0);
            planner.AddItemToQueue(invalidItem);
        }
        catch (InvalidDimensionsException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[ПЕРЕХОПЛЕНО] {ex.Message}");
            Console.ResetColor();
        }

        // Блок Try-Catch 2: Обробка кастомного винятку
        try
        {
            var hugeItem = new BoxItem("Слон", 100.0);
            planner.AddItemToQueue(hugeItem);
        }
        catch (ItemTooLargeException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[ПЕРЕХОПЛЕНО] {ex.Message}");
            Console.ResetColor();
        }

        // --- 3. Додавання коректних предметів ---
        try
        {
            // Використання публічного інтерфейсу `PackPlanner`
            planner.AddItemToQueue(new BoxItem("Телевізор", 20.0));
            planner.AddItemToQueue(new BoxItem("Мікрохвильовка", 8.0));
            planner.AddItemToQueue(new BoxItem("Крісло", 22.0));
            planner.AddItemToQueue(new BoxItem("Комп'ютер", 7.0));
            planner.AddItemToQueue(new BoxItem("Принтер", 6.0));
            planner.AddItemToQueue(new BoxItem("Шафа", 45.0)); 
            planner.AddItemToQueue(new BoxItem("Диван", 30.0)); // Цей предмет не має влізти
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Неочікувана помилка при додаванні: {ex.Message}");
        }

        // --- 4. Виконання пакування (виклик Абстракції) ---
        // Блок Try-Catch 3: Обробка кастомного винятку
        try
        {
            Console.WriteLine("\nЧерга перед пакуванням (пріоритетна):");
            // Звернення до інкапсульованої, але доступної "лише для читання" колекції
            foreach (var item in planner.ItemQueue) Console.WriteLine($"- {item}");
            
            // АБСТРАКЦІЯ: Ми просто кажемо "пакуй",
            // не знаючи, як складно це відбувається всередині.
            planner.PackItemsGreedy();
        }
        catch (NoSpaceException ex)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n[ІНФО] {ex.Message}");
            Console.ResetColor();
        }
        
        // --- 5. Фінальний звіт ---
        // Виклик іншого "абстрактного" методу
        planner.PrintPackingReport();
    }
}