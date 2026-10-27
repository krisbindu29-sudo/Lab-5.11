using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lab5_Task1
{
    // Власне виключення для некоректного введення даних з клавіатури
    public class InvalidInputDataException : Exception
    {
        public InvalidInputDataException(string message) : base(message) { }
    }

    // Базовий клас "Лікар"
    public abstract class 
    {
        public string LastName { get; set; }
        public int Experience { get; set; }

        public Doctor() { }

        public Doctor(string lastName, int experience)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new InvalidInputDataException("Прізвище лікаря не може бути порожнім!");
            if (experience < 0)
                throw new InvalidInputDataException("Стаж лікаря не може бути від'ємним!");

            LastName = lastName;
            Experience = experience;
        }

        // Віртуальний метод для отримання короткої інформації
        public virtual string GetInfo()
        {
            return $"Лікар: {LastName}, Стаж: {Experience} років";
        }
    }

    // Похідний клас "Прийом"
    public class Appointment : Doctor
    {
        public string Day { get; set; }
        public int VisitorsCount { get; set; }
        public string Comment { get; set; }

        public Appointment() { }

        public Appointment(string lastName, int experience, string day, int visitorsCount, string comment)
            : base(lastName, experience)
        {
            if (string.IsNullOrWhiteSpace(day))
                throw new InvalidInputDataException("День прийому не може бути порожнім!");
            if (visitorsCount < 0)
                throw new InvalidInputDataException("Кількість відвідувачів не може бути від'ємною!");

            Day = day;
            VisitorsCount = visitorsCount;
            Comment = comment ?? "";
        }

        // Перевизначення віртуального методу (override)
        public override string GetInfo()
        {
            return $"{base.GetInfo()} | День: {Day}, Відвідувачів: {VisitorsCount}, Коментар: \"{Comment}\"";
        }

        // Форматування для збереження у текстовий файл
        public string ToFileString()
        {
            return $"{LastName};{Experience};{Day};{VisitorsCount};{Comment}";
        }

        // Парсинг із рядка файлу
        public static Appointment FromFileString(string line)
        {
            var parts = line.Split(';');
            if (parts.Length < 5) return null;

            return new Appointment(
                parts[0],
                int.Parse(parts[1]),
                parts[2],
                int.Parse(parts[3]),
                parts[4]
            );
        }
    }

    // Менеджер роботи з базою даних та обчисленнями
    public class DatabaseManager
    {
        private readonly string _filePath;
        public List<Appointment> Appointments { get; private set; }

        public DatabaseManager(string filePath)
        {
            _filePath = filePath;
            Appointments = new List<Appointment>();
            EnsureInitialData();
            LoadFromFile();
        }

        // Створення файлу з початковими 5 записами, якщо він відсутній
        private void EnsureInitialData()
        {
            if (!File.Exists(_filePath))
            {
                var initialRecords = new List<string>
                {
                    "Ковальчук;12;Понеділок;15;Первинний огляд пацієнтів",
                    "Мельник;8;Вівторок;7;Консультація за результатами аналізів",
                    "Шевченко;20;Середа;22;Плановий профілактичний огляд",
                    "Ковальчук;12;Четвер;5;Короткий прийом для виписки довідок",
                    "Бойко;5;П'ятниця;11;Повторний прийом після курсу лікування"
                };
                File.WriteAllLines(_filePath, initialRecords);
            }
        }

        public void LoadFromFile()
        {
            Appointments.Clear();
            if (File.Exists(_filePath))
            {
                var lines = File.ReadAllLines(_filePath);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var app = Appointment.FromFileString(line);
                        if (app != null) Appointments.Add(app);
                    }
                }
            }
        }

        public void SaveToFile()
        {
            var lines = Appointments.Select(a => a.ToFileString());
            File.WriteAllLines(_filePath, lines);
        }

        public void AddRecord(Appointment app)
        {
            Appointments.Add(app);
            SaveToFile();
        }

        public void RemoveRecord(int index)
        {
            if (index >= 0 && index < Appointments.Count)
            {
                Appointments.RemoveAt(index);
                SaveToFile();
            }
        }

        public void EditRecord(int index, Appointment updatedApp)
        {
            if (index >= 0 && index < Appointments.Count)
            {
                Appointments[index] = updatedApp;
                SaveToFile();
            }
        }

        // ІНДИВІДУАЛЬНІ ОБЧИСЛЕННЯ ВАРІАНТУ 9
        public double GetAverageVisitors()
        {
            if (Appointments.Count == 0) return 0;
            return Appointments.Average(a => a.VisitorsCount);
        }

        public Appointment GetAppointmentWithMinVisitors()
        {
            if (Appointments.Count == 0) return null;
            return Appointments.OrderBy(a => a.VisitorsCount).FirstOrDefault();
        }

        public Appointment GetAppointmentWithLongestComment()
        {
            if (Appointments.Count == 0) return null;
            return Appointments.OrderByDescending(a => a.Comment.Length).FirstOrDefault();
        }
    }

    // Головний клас з методом Main (розміщений у самому кінці)
    class Program
    {
        static void DisplayMenu()
        {
           
            Console.WriteLine("  БАЗА ДАНИХ ПРИЙОМІВ ЛІКАРІВ   ");
            
            Console.WriteLine(" 1 або 'v' - Вивести всі записи з файлу");
            Console.WriteLine(" 2 або 'a' - Додати новий запис");
            Console.WriteLine(" 3 або 'e' - Редагувати запис");
            Console.WriteLine(" 4 або 'd' - Видалити запис");
            Console.WriteLine(" 5 або 'c' - Обчислити результати за варіантом");
            Console.WriteLine(" 0 або Enter - Вийти з програми");
            Console.Write("\nВаш вибір: ");
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var db = new DatabaseManager("database_task1.txt");

            while (true)
            {
                DisplayMenu();
                string input = Console.ReadLine()?.Trim();

                // Вихід за кнопкою Enter або введенням "0"
                if (string.IsNullOrEmpty(input) || input == "0")
                {
                    Console.WriteLine("\nЗавершення роботи програми.");
                    break;
                }

                try
                {
                    switch (input.ToLower())
                    {
                        case "1":
                        case "v":
                            Console.WriteLine("\n--- Список записів у базі даних ---");
                            for (int i = 0; i < db.Appointments.Count; i++)
                            {
                                Console.WriteLine($"[{i}] {db.Appointments[i].GetInfo()}");
                            }
                            break;

                        case "2":
                        case "a":
                            Console.WriteLine("\n--- Додавання нового запису ---");
                            Console.Write("Прізвище лікаря: ");
                            string name = Console.ReadLine();
                            Console.Write("Стаж (років): ");
                            int exp = int.Parse(Console.ReadLine());
                            Console.Write("День прийому: ");
                            string day = Console.ReadLine();
                            Console.Write("Кількість відвідувачів: ");
                            int count = int.Parse(Console.ReadLine());
                            Console.Write("Коментар: ");
                            string comm = Console.ReadLine();

                            var newApp = new Appointment(name, exp, day, count, comm);
                            db.AddRecord(newApp);
                            Console.WriteLine("\nЗапис успішно додано та збережено у файл!");
                            break;

                        case "3":
                        case "e":
                            Console.Write("\nВведіть індекс запису для редагування: ");
                            int editIdx = int.Parse(Console.ReadLine());
                            if (editIdx < 0 || editIdx >= db.Appointments.Count)
                            {
                                Console.WriteLine("Некоректний індекс!");
                                break;
                            }

                            Console.Write("Нове прізвище лікаря: ");
                            string eName = Console.ReadLine();
                            Console.Write("Новий стаж: ");
                            int eExp = int.Parse(Console.ReadLine());
                            Console.Write("Новий день прийому: ");
                            string eDay = Console.ReadLine();
                            Console.Write("Нова кількість відвідувачів: ");
                            int eCount = int.Parse(Console.ReadLine());
                            Console.Write("Новий коментар: ");
                            string eComm = Console.ReadLine();

                            db.EditRecord(editIdx, new Appointment(eName, eExp, eDay, eCount, eComm));
                            Console.WriteLine("\nЗапис успішно відредаговано!");
                            break;

                        case "4":
                        case "d":
                            Console.Write("\nВведіть індекс запису для видалення: ");
                            int delIdx = int.Parse(Console.ReadLine());
                            db.RemoveRecord(delIdx);
                            Console.WriteLine("\nЗапис успішно видалено!");
                            break;

                        case "5":
                        case "c":
                           
                            Console.WriteLine("  РЕЗУЛЬТАТИ ОБЧИСЛЕНЬ (ІНДИВІДУАЛЬНЕ ЗАВДАННЯ)");
                           
                            Console.WriteLine($"1) Середня кількість відвідувачів: {db.GetAverageVisitors():F2}");

                            var minApp = db.GetAppointmentWithMinVisitors();
                            if (minApp != null)
                                Console.WriteLine($"2) Прийом з мін. відвідувачів ({minApp.VisitorsCount}): {minApp.GetInfo()}");

                            var maxCommApp = db.GetAppointmentWithLongestComment();
                            if (maxCommApp != null)
                                Console.WriteLine($"3) Прийом з найдовшим коментарем: {maxCommApp.GetInfo()}");
                            break;

                        default:
                            Console.WriteLine("\nНевідома команда! Спробуйте ще раз.");
                            break;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("\n[ПОМИЛКА ВВЕДЕННЯ] Помилка формату числового значення!");
                }
                catch (InvalidInputDataException ex)
                {
                    Console.WriteLine($"\n[СПЕЦІАЛЬНЕ ВИКЛЮЧЕННЯ] {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[ПОМИЛКА] {ex.Message}");
                }
            }
        }
    }
}