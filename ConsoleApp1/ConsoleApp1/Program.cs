using System.Text.Json;

namespace ConsoleApp1
{
    namespace Tamagoji
    {
        public class Pet
        {
            public string Name { get; set; }
            public string Species { get; set; }
            public int Hunger { get; set; }
            public int Happiness { get; set; }
            public int Energy { get; set; }
            public int Age { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        public static class Logger
        {
            private static readonly string _logPath = "pet_log.txt";

            public static void Log(string message)
            {
                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                Console.WriteLine(entry);
                File.AppendAllText(_logPath, entry + Environment.NewLine);
            }
        }

        public class PetManager
        {
            private List<Pet> _pets;
            private string _filePath = "pets.json";
            private JsonSerializerOptions _options;

            public PetManager()
            {
                _options = new JsonSerializerOptions { WriteIndented = true };
                _pets = LoadPets();
            }

            private List<Pet> LoadPets()
            {
                if (File.Exists(_filePath))
                {
                    try
                    {
                        string json = File.ReadAllText(_filePath);
                        var pets = JsonSerializer.Deserialize<List<Pet>>(json, _options);
                        UpdateAges(pets);
                        return pets;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Ошибка загрузки: {ex.Message}");
                        return new List<Pet>();
                    }
                }
                return new List<Pet>();
            }

            private void UpdateAges(List<Pet> pets)
            {
                DateTime now = DateTime.Now;
                foreach (var pet in pets)
                {
                    int daysPassed = (now - pet.LastUpdated).Days;
                    if (daysPassed > 0)
                    {
                        pet.Age += daysPassed;
                        pet.LastUpdated = now;
                    }
                }
            }

            public void SavePets()
            {
                try
                {
                    foreach (var pet in _pets)
                        pet.LastUpdated = DateTime.Now;

                    string json = JsonSerializer.Serialize(_pets, _options);
                    File.WriteAllText(_filePath, json);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка сохранения: {ex.Message}");
                }
            }
            public void AddPet(Pet pet)
            {
                _pets.Add(pet);
                SavePets();
            }
            public List<Pet> GetPets() => _pets;

            public void Feed(Pet pet)
            {
                pet.Hunger = Math.Clamp(pet.Hunger - 25, 0, 100);
                pet.Happiness = Math.Clamp(pet.Happiness - 10, 0, 100);
                CheckStatus(pet);
                RandomEvent(pet);
                SavePets();
                Logger.Log($"{pet.Name} покормлен.");
            }

            public void Play(Pet pet)
            {
                pet.Happiness = Math.Clamp(pet.Happiness + 30, 0, 100);
                pet.Energy = Math.Clamp(pet.Energy - 20, 0, 100);
                CheckStatus(pet);
                RandomEvent(pet);
                SavePets();
                Logger.Log($"{pet.Name} играл.");
            }

            public void Sleep(Pet pet)
            {
                pet.Energy = Math.Clamp(pet.Energy + 50, 0, 100);
                pet.Hunger = Math.Clamp(pet.Hunger + 15, 0, 100);
                CheckStatus(pet);
                RandomEvent(pet);
                SavePets();
                Logger.Log($"{pet.Name} поспал.");
            }

            private void CheckStatus(Pet pet)
            {
                if (pet.Hunger >= 100)
                    Logger.Log($"{pet.Name} голодает! Срочно покормите!");

                if (pet.Happiness <= 0)
                    Logger.Log($"{pet.Name} в депрессии! Поиграйте с ним!");
            }
            private void RandomEvent(Pet pet)
            {
                Random rand = new Random();
                if (rand.Next(100) < 30)
                {
                    switch (rand.Next(3))
                    {
                        case 0:
                            pet.Energy -= 20;
                            Logger.Log($"{pet.Name} заболел(-а)!");
                            break;
                        case 1:
                            pet.Happiness += 25;
                            Logger.Log($"{pet.Name} нашёл(-ла) игрушку!");
                            break;
                        case 2:
                            pet.Age += 1;
                            Logger.Log($"{pet.Name} отмечает день рождения!");
                            break;
                    }
                }
            }
        }

        class Program
        {
            static void Main()
            {
                var manager = new PetManager();
                bool exit = false;

                while (!exit)
                {
                    Console.Clear();
                    Console.WriteLine("Virtual Pet Care\n");
                    Console.WriteLine("1. Создать питомца");
                    Console.WriteLine("2. Выбрать питомца");
                    Console.WriteLine("3. Выход");
                    Console.Write("> ");

                    switch (Console.ReadLine())
                    {
                        case "1":
                            CreatePet(manager);
                            break;
                        case "2":
                            SelectPet(manager);
                            break;
                        case "3":
                            exit = true;
                            break;
                    }
                }
            }
            static void CreatePet(PetManager manager)
            {
                Console.Write("Имя: ");
                string name = Console.ReadLine();
                Console.Write("Вид: ");
                string species = Console.ReadLine();

                manager.AddPet(new Pet
                {
                    Name = name,
                    Species = species,
                    Hunger = 50,
                    Happiness = 50,
                    Energy = 50,
                    Age = 0,
                    LastUpdated = DateTime.Now
                });

                Logger.Log($"Создан новый питомец: {name}");
            }

            static void SelectPet(PetManager manager)
            {
                var pets = manager.GetPets();
                if (pets.Count == 0)
                {
                    Console.WriteLine("Питомцев нет! Создайте нового.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("Выберите питомца:");
                for (int i = 0; i < pets.Count; i++)
                    Console.WriteLine($"{i + 1}. {pets[i].Name} ({pets[i].Species})");

                if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= pets.Count)
                    ManagePet(manager, pets[index - 1]);
            }

            static void ManagePet(PetManager manager, Pet pet)
            {
                bool back = false;
                while (!back)
                {
                    Console.Clear();
                    ShowStatus(pet);
                    Console.WriteLine("\n1. Кормить\n2. Играть\n3. Спать\n4. Назад");
                    Console.Write("> ");

                    switch (Console.ReadLine())
                    {
                        case "1":
                            manager.Feed(pet);
                            break;
                        case "2":
                            manager.Play(pet);
                            break;
                        case "3":
                            manager.Sleep(pet);
                            break;
                        case "4":
                            back = true;
                            break;
                    }
                    Console.ReadKey();
                }
            }

            static void ShowStatus(Pet pet)
            {
                Console.WriteLine($"{pet.Name} ({pet.Species})");
                Console.WriteLine($"Возраст: {pet.Age} дней");
                Console.WriteLine($"Голод:   {pet.Hunger}/100");
                Console.WriteLine($"Счастье: {pet.Happiness}/100");
                Console.WriteLine($"Энергия: {pet.Energy}/100");
            }
        }
    }
}
