using System;
using System.Collections.Generic;
using System.Text;

namespace Lab3_Inheritance
{
    // 🔹 Базовий клас, який описує часовий інтервал (початок і кінець).
    // Містить метод для обчислення тривалості інтервалу.
    class TimeSpanBase
    {
        // Властивості з модифікаторами доступу public 
        // (доступні і в похідних класах, і зовні).
        public DateTime Start { get; set; }   // час початку
        public DateTime End { get; set; }     // час завершення

        // Конструктор базового класу
        public TimeSpanBase(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        // Віртуальний метод (може бути перевизначений у похідних класах).
        public virtual double GetDuration()
        {
            return (End - Start).TotalMinutes; // повертає тривалість у хвилинах
        }

        // Перевизначення ToString() для виводу інформації про інтервал
        public override string ToString()
        {
            return $"{Start:HH:mm} - {End:HH:mm} ({GetDuration()} хв)";
        }
    }

    // 🔹 Похідний клас, який представляє заняття.
    // Наслідує TimeSpanBase і додає власну властивість Subject.
    class LessonTime : TimeSpanBase
    {
        public string Subject { get; set; }   // назва предмету

        // Використання ключового слова base для виклику конструктора базового класу
        public LessonTime(DateTime start, DateTime end, string subject)
            : base(start, end)
        {
            Subject = subject;
        }

        // Поліморфізм: перевизначення ToString() 
        public override string ToString()
        {
            return $"Заняття: {Subject} {base.ToString()}";
        }
    }

    // 🔹 Похідний клас, який представляє перерву між заняттями.
    class BreakTime : TimeSpanBase
    {
        // Конструктор лише викликає базовий
        public BreakTime(DateTime start, DateTime end)
            : base(start, end)
        {
        }

        // Перевизначення ToString()
        public override string ToString()
        {
            return $"Перерва {base.ToString()}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Забезпечуємо підтримку українських символів у консолі
            Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;

            // 🔹 Поліморфізм:
            // ми створюємо список базового типу TimeSpanBase,
            // але зберігаємо в ньому об'єкти похідних класів (LessonTime і BreakTime).
            List<TimeSpanBase> schedule = new List<TimeSpanBase>
            {
                new LessonTime(new DateTime(2025, 9, 29, 8, 30, 0), new DateTime(2025, 9, 29, 9, 15, 0), "Математика"),
                new BreakTime(new DateTime(2025, 9, 29, 9, 15, 0), new DateTime(2025, 9, 29, 9, 30, 0)),
                new LessonTime(new DateTime(2025, 9, 29, 9, 30, 0), new DateTime(2025, 9, 29, 10, 15, 0), "Програмування"),
                new BreakTime(new DateTime(2025, 9, 29, 10, 15, 0), new DateTime(2025, 9, 29, 10, 30, 0)),
                new LessonTime(new DateTime(2025, 9, 29, 10, 30, 0), new DateTime(2025, 9, 29, 11, 15, 0), "Фізика")
            };

            double totalLessons = 0; // загальна тривалість занять
            double totalBreaks = 0;  // загальна тривалість перерв

            Console.WriteLine(" Розклад дня:");
            foreach (var item in schedule)
            {
                // Викликається ToString() для відповідного класу (поліморфізм)
                Console.WriteLine(item);

                // Перевіряємо тип об'єкта за допомогою is
                if (item is LessonTime)
                    totalLessons += item.GetDuration();
                else if (item is BreakTime)
                    totalBreaks += item.GetDuration();
            }

            // Вивід результатів
            Console.WriteLine($"\n Загальна тривалість занять: {totalLessons} хв");
            Console.WriteLine($" Загальна тривалість перерв: {totalBreaks} хв");
            Console.WriteLine($" Повна тривалість дня: {totalLessons + totalBreaks} хв");
        }
    }
}
