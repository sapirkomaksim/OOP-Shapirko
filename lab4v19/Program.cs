using System;
using System.IO;
using System.Text;

namespace FileStorageExample
{
    // Інтерфейс, який визначає базові операції з файлами
    interface IFileOperation
    {
        void Execute(string path, string content = null); // Метод виконання операції
    }

    // Абстрактний клас, який реалізує спільні властивості для всіх операцій
    abstract class FileOperationBase : IFileOperation
    {
        protected static int saveCount = 0;   // Лічильник збережень
        protected static int loadCount = 0;   // Лічильник завантажень

        // Абстрактний метод, який мають реалізувати нащадки
        public abstract void Execute(string path, string content = null);

        // Метод для виведення статистики
        public static void PrintStatistics()
        {
            Console.WriteLine($"\n Статистика операцій:");
            Console.WriteLine($"Збережено файлів: {saveCount}");
            Console.WriteLine($"Завантажено файлів: {loadCount}");
        }
    }

    // Реалізація для збереження файлу
    class FileSave : FileOperationBase
    {
        public override void Execute(string path, string content = null)
        {
            // Якщо контент відсутній — повідомляємо користувача
            if (content == null)
            {
                Console.WriteLine("❗ Відсутній текст для збереження.");
                return;
            }

            File.WriteAllText(path, content, Encoding.UTF8);
            saveCount++; // Збільшуємо лічильник збережень
            Console.WriteLine($" Файл успішно збережено: {path}");
        }
    }

    // Реалізація для завантаження файлу
    class FileLoad : FileOperationBase
    {
        public override void Execute(string path, string content = null)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"❌ Файл не знайдено: {path}");
                return;
            }

            string data = File.ReadAllText(path, Encoding.UTF8);
            loadCount++; // Збільшуємо лічильник завантажень
            Console.WriteLine($" Вміст файлу '{path}':");
            Console.WriteLine(data);
        }
    }

    // Демонстрація роботи програми
    class Program
    {
        static void Main()
        {
            Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;

            IFileOperation saver = new FileSave();
            IFileOperation loader = new FileLoad();

            string filePath = "test.txt";

            // Збереження файлу
            saver.Execute(filePath, "Привіт, світ! Це тестовий файл.");

            // Завантаження файлу
            loader.Execute(filePath);

            // Виведення статистики
            FileOperationBase.PrintStatistics();

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
