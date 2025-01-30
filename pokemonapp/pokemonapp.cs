using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main(string[] args)
    {
        string dataDirectory = "../Data";
        var jsonFiles = Directory.GetFiles(dataDirectory, "*.json").ToList();
        int currentIndex = 0;

        while (true)
        {
            if (currentIndex < 0) currentIndex = 0;
            if (currentIndex >= jsonFiles.Count) currentIndex = jsonFiles.Count - 1;

            var file = jsonFiles[currentIndex];
            var jsonData = File.ReadAllText(file);
            var jsonObject = JObject.Parse(jsonData);

            var id = jsonObject["id"]?.ToString();
            var name = jsonObject["name"]?.ToString();
            var flavorText = jsonObject["flavor_text_entries"]
                ?.FirstOrDefault(entry => entry["version"]?.ToString() == "red")?["flavor_text"]?.ToString();

            if (id != null && name != null && flavorText != null)
            {
                Console.Clear();
                Console.WriteLine($"ID: {id}");
                Console.WriteLine($"Name: {name}");
                Console.WriteLine($"Flavor Text (Red): {flavorText}");
                Console.WriteLine(new string('-', 20));
                Console.WriteLine("Use left/right arrow keys to navigate through Pokémon. Press 'q' to quit.");

                var key = Console.ReadKey().Key;
                if (key == ConsoleKey.RightArrow)
                {
                    currentIndex++;
                }
                else if (key == ConsoleKey.LeftArrow)
                {
                    currentIndex--;
                }
                else if (key == ConsoleKey.Q)
                {
                    break;
                }
            }
        }
    }
}