using System.Text.Json;

class Pokemon
{
    public int Id { get; set; }
    public Dictionary<string, string> Names { get; set; } = new();
    public Dictionary<string, List<FlavorText>> FlavorTexts { get; set; } = new();
}

class FlavorText
{
    public string Text { get; set; } = "";
    public string Version { get; set; } = "";
}

class Program
{
    private static List<Pokemon> pokemons = new();
    private static int currentIndex = 0;

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        await LoadPokemonData();
        
        while (true)
        {
            Console.Clear();
            DisplayCurrentPokemon();
            
            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    if (currentIndex > 0) currentIndex--;
                    break;
                case ConsoleKey.RightArrow:
                    if (currentIndex < pokemons.Count - 1) currentIndex++;
                    break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    static async Task LoadPokemonData()
    {
        for (int i = 1; i <= 151; i++)
        {
            var filePath = Path.Combine("..", "Data", $"{i}.json");
            if (!File.Exists(filePath)) continue;

            var jsonString = await File.ReadAllTextAsync(filePath);
            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            var pokemon = new Pokemon { Id = i };

            // Load names in different languages
            if (root.TryGetProperty("names", out var names))
            {
                foreach (var name in names.EnumerateArray())
                {
                    var language = name.GetProperty("language").GetProperty("name").GetString();
                    var pokemonName = name.GetProperty("name").GetString();
                    if (language != null && pokemonName != null)
                    {
                        pokemon.Names[language] = pokemonName;
                    }
                }
            }

            // Load flavor texts
            if (root.TryGetProperty("flavor_text_entries", out var flavorTexts))
            {
                foreach (var entry in flavorTexts.EnumerateArray())
                {
                    var language = entry.GetProperty("language").GetProperty("name").GetString();
                    if (language == null) continue;

                    if (!pokemon.FlavorTexts.ContainsKey(language))
                    {
                        pokemon.FlavorTexts[language] = new List<FlavorText>();
                    }

                    var version = entry.GetProperty("version").GetProperty("name").GetString() ?? "";
                    var text = entry.GetProperty("flavor_text").GetString() ?? "";
                    text = text.Replace("\n", " ").Replace("\f", " ").Trim();

                    pokemon.FlavorTexts[language].Add(new FlavorText
                    {
                        Text = text,
                        Version = version
                    });
                }
            }

            pokemons.Add(pokemon);
        }
    }

    static void DisplayCurrentPokemon()
    {
        var pokemon = pokemons[currentIndex];
        Console.WriteLine($"ID: {pokemon.Id}");
        
        // Display German name if available, fallback to English
        var name = pokemon.Names.GetValueOrDefault("de", pokemon.Names.GetValueOrDefault("en", "Unknown"));
        Console.WriteLine($"Name: {name}");

        // Display German flavor text if available, fallback to English
        var germanTexts = pokemon.FlavorTexts.GetValueOrDefault("de", new List<FlavorText>());
        var englishTexts = pokemon.FlavorTexts.GetValueOrDefault("en", new List<FlavorText>());
        
        var flavorText = germanTexts.FirstOrDefault()?.Text ?? englishTexts.FirstOrDefault()?.Text ?? "";
        Console.WriteLine(flavorText);
        Console.WriteLine();
        Console.WriteLine("Press Left/Right arrow keys to navigate, or Esc to exit.");
    }
}
