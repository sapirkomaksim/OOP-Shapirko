using System;

namespace lab23_Payroll
{
    // 1: Порушення ISP та DIP (Початкова структура)
    // Порушення ISP: "Товстий" інтерфейс, який змушує експортер реалізовувати 
    // зайві методи.
    public interface IReportExporter
    {
        void ExportToPdf(string data);
        void ExportToExcel(string data);
    }

    // Клас PdfExporter змушений реалізовувати ExportToExcel, хоча не вміє цього робити.
    public class PdfExporter : IReportExporter
    {
        public void ExportToPdf(string data)
        {
            Console.WriteLine($"[PdfExporter] Згенеровано PDF з даними: {data}");
        }

        public void ExportToExcel(string data)
        {
            // Порушення ISP призводить до "костилів" у вигляді винятків
            throw new NotImplementedException("Цей експортер підтримує лише PDF.");
        }
    }

    // Низькорівневий модуль для роботи з БД
    public class SqlDatabase
    {
        public void SaveReport(string reportData)
        {
            Console.WriteLine($"[SqlDatabase] Звіт збережено у SQL базу: {reportData}");
        }
    }

    // Модуль вищого рівня
    public class PayrollSystem
    {
        // Порушення DIP: Жорстка прив'язка до конкретних класів замість абстракцій.
        private PdfExporter _pdfExporter;
        private SqlDatabase _sqlDatabase;

        public PayrollSystem()
        {
            // Система сама створює свої залежності. Її неможливо протестувати ізольовано.
            _pdfExporter = new PdfExporter();
            _sqlDatabase = new SqlDatabase();
        }

        public void ProcessPayroll(string employeeData)
        {
            Console.WriteLine(" Запуск розрахунку (Погана архітектура)");
            string calculatedData = employeeData + " | Зарплата: 1500$";
            
            _pdfExporter.ExportToPdf(calculatedData);
            _sqlDatabase.SaveReport(calculatedData);
        }
    }
    // 2: Рефакторинг - дотримання ISP та DIP

    

    // 1. Виправлення ISP: Робимо інтерфейс вузькоспеціалізованим. 
    // Замість прив'язки до формату, робимо загальний метод експорту.
    public interface IExporter
    {
        void Export(string data);
    }

    // Тепер PdfExporter реалізує лише те, що дійсно вміє.
    public class GoodPdfExporter : IExporter
    {
        public void Export(string data)
        {
            Console.WriteLine($"[GoodPdfExporter] Згенеровано PDF з даними: {data}");
        }
    }

    // 2. Виправлення DIP: Виділяємо абстракцію для бази даних.
    public interface IDatabase
    {
        void Save(string data);
    }

    public class GoodSqlDatabase : IDatabase
    {
        public void Save(string data)
        {
            Console.WriteLine($"[GoodSqlDatabase] Звіт збережено у SQL базу: {data}");
        }
    }

    

    // 3. Виправлення DIP: Ін'єкція залежностей (Dependency Injection).
    // PayrollSystem тепер залежить від абстракцій (IExporter, IDatabase), 
    // а не від конкретних реалізацій.
    public class GoodPayrollSystem
    {
        private readonly IExporter _exporter;
        private readonly IDatabase _database;

        // Залежності передаються через конструктор
        public GoodPayrollSystem(IExporter exporter, IDatabase database)
        {
            _exporter = exporter;
            _database = database;
        }

        public void ProcessPayroll(string employeeData)
        {
            Console.WriteLine("Запуск розрахунку (Хороша архітектура)");
            string calculatedData = employeeData + " | Зарплата: 2000$";
            
            _exporter.Export(calculatedData);
            _database.Save(calculatedData);
        }
    }
    // 3: Демонстрація (Main)
    class Program
    {
        static void Main(string[] args)
        {
            string empData = "Працівник: Іван Іванов";

            // Демонстрація до рефакторингу
            Console.WriteLine("До рефакторингу");
            var badPayroll = new PayrollSystem();
            badPayroll.ProcessPayroll(empData);

            Console.WriteLine("\n Після рефакторингу");
            
            // Створюємо залежності ззовні (це може робити DI-контейнер у реальних проєктах)
            IExporter pdfExporter = new GoodPdfExporter();
            IDatabase sqlDb = new GoodSqlDatabase();
            
            // Передаємо залежності у систему (Constructor Injection)
            var goodPayroll = new GoodPayrollSystem(pdfExporter, sqlDb);
            goodPayroll.ProcessPayroll(empData);

            Console.ReadLine();
        }
    }
}