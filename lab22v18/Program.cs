using System;
using System.Collections.Generic;

namespace lab22
{
    // 1: Порушення LSP (Погана архітектура)

    // Базовий клас
    public class CustomList
    {
        protected List<string> _items = new List<string>();

        public virtual void Add(string item)
        {
            _items.Add(item);
            Console.WriteLine($"[Bad] Додано: {item}");
        }

        public virtual IEnumerable<string> GetItems() => _items;
    }

    // Похідний клас, який порушує LSP
    public class ReadOnlyList : CustomList
    {
        public ReadOnlyList(IEnumerable<string> initialItems)
        {
            _items.AddRange(initialItems);
        }

        // Порушення контракту:
        // Базовий клас обіцяє, що метод Add() додасть елемент.
        // Похідний клас змінює цю поведінку і викидає виняток. 
        // Клієнтський код, який очікує CustomList, зламається.
        public override void Add(string item)
        {
            throw new NotSupportedException("Помилка: Неможливо додати елемент до ReadOnlyList.");
        }
    }
    // 2: Аналіз порушення LSP
    /*
     * Чому ця ієрархія порушує LSP?
     * Принцип підстановки Лісков (LSP) стверджує, що об'єкти базового класу повинні 
     * бути замінюваними на об'єкти похідних класів без порушення правильності програми.
     * * У нашому випадку клієнт (метод AddItemToBadList) приймає базовий тип `CustomList` 
     * і очікує, що метод `Add` виконається успішно. Але якщо ми передаємо йому 
     * `ReadOnlyList`, метод `Add` викидає NotSupportedException. Контракт базового 
     * класу (обіцянка, що елемент можна додати) розривається похідним класом.
     */

    // 3: Дотримання LSP (Рефакторинг)
    // Рішення: Використання спільних інтерфейсів. 
    // Розділяємо можливості "читання" і "запису".

    

    // 1. Інтерфейс тільки для читання (базовий контракт)
    public interface IReadableCollection
    {
        IEnumerable<string> GetItems();
    }

    // 2. Інтерфейс для зміни даних (розширений контракт)
    public interface IMutableCollection : IReadableCollection
    {
        void Add(string item);
    }

    // Правильний змінний список
    public class GoodCustomList : IMutableCollection
    {
        private List<string> _items = new List<string>();

        public void Add(string item)
        {
            _items.Add(item);
            Console.WriteLine($"[Good] Додано: {item}");
        }

        public IEnumerable<string> GetItems() => _items;
    }

    // Правильний список тільки для читання (не обіцяє те, чого не може зробити)
    public class GoodReadOnlyList : IReadableCollection
    {
        private List<string> _items;

        public GoodReadOnlyList(IEnumerable<string> initialItems)
        {
            _items = new List<string>(initialItems);
        }

        public IEnumerable<string> GetItems() => _items;
    }

    // 4: Демонстрація (Main)
    class Program
    {
        // "Клієнтський" метод для поганої архітектури
        static void AddItemToBadList(CustomList list, string item)
        {
            Console.WriteLine("Спроба додати елемент до CustomList...");
            list.Add(item);
        }

        // "Клієнтський" метод для хорошої архітектури
        // Тепер він чітко вимагає колекцію, яка ПІДТРИМУЄ зміну (IMutableCollection)
        static void AddItemToGoodList(IMutableCollection list, string item)
        {
            Console.WriteLine("Спроба додати елемент до IMutableCollection...");
            list.Add(item);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Демонстрація порушення LSP");
            CustomList regularList = new CustomList();
            CustomList readOnlyList = new ReadOnlyList(new[] { "Елемент 1", "Елемент 2" });

            // Це спрацює
            AddItemToBadList(regularList, "Новий елемент");

            // Це викличе виняток (Порушення LSP!)
            try
            {
                AddItemToBadList(readOnlyList, "Ще один елемент");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Виключення: {ex.Message}");
            }

            Console.WriteLine("\n Демонстрація дотримання LSP ");
            
            IMutableCollection goodList = new GoodCustomList();
            IReadableCollection goodReadOnly = new GoodReadOnlyList(new[] { "Елемент А", "Елемент Б" });

            // Це працює коректно
            AddItemToGoodList(goodList, "Новий елемент");

            // Наступний рядок коду навіть не скомпілюється, що є ознакою надійної архітектури!
            // Клієнт захищений на етапі компіляції.
            // AddItemToGoodList(goodReadOnly, "Ще один"); // Помилка компіляції: неможливо конвертувати IReadableCollection в IMutableCollection

            Console.WriteLine("Усі допустимі операції виконано успішно.");
            Console.ReadLine();
        }
    }
}