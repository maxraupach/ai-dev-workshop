using System.ComponentModel;
using System.Text.Json;
using System.Windows.Input;
using System.IO;

namespace GuiApp
{
    public class PokemonViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string FlavorText { get; set; } = "";
        public string ImagePath { get; set; } = "";
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly List<PokemonViewModel> _pokemons = new();
        private int _currentIndex;

        public PokemonViewModel CurrentPokemon => _pokemons[_currentIndex];

        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            LoadPokemonData();

            PreviousCommand = new RelayCommand(
                () => 
                {
                    if (_currentIndex > 0)
                    {
                        _currentIndex--;
                        OnPropertyChanged(nameof(CurrentPokemon));
                    }
                },
                () => _currentIndex > 0
            );

            NextCommand = new RelayCommand(
                () =>
                {
                    if (_currentIndex < _pokemons.Count - 1)
                    {
                        _currentIndex++;
                        OnPropertyChanged(nameof(CurrentPokemon));
                    }
                },
                () => _currentIndex < _pokemons.Count - 1
            );
        }

        private void LoadPokemonData()
        {
            for (int i = 1; i <= 151; i++)
            {
                var filePath = Path.Combine("..", "Data", $"{i}.json");
                if (!File.Exists(filePath)) continue;

                var jsonString = File.ReadAllText(filePath);
                var jsonDoc = JsonDocument.Parse(jsonString);
                var root = jsonDoc.RootElement;

                var pokemon = new PokemonViewModel { Id = i };

                // Load German name, fallback to English
                if (root.TryGetProperty("names", out var names))
                {
                    var germanName = names.EnumerateArray()
                        .FirstOrDefault(n => n.GetProperty("language").GetProperty("name").GetString() == "de");
                    
                    if (germanName.ValueKind != JsonValueKind.Undefined)
                    {
                        pokemon.Name = germanName.GetProperty("name").GetString() ?? "Unknown";
                    }
                    else
                    {
                        var englishName = names.EnumerateArray()
                            .FirstOrDefault(n => n.GetProperty("language").GetProperty("name").GetString() == "en");
                        pokemon.Name = englishName.ValueKind != JsonValueKind.Undefined 
                            ? englishName.GetProperty("name").GetString() ?? "Unknown"
                            : "Unknown";
                    }
                }

                // Load German flavor text, fallback to English
                if (root.TryGetProperty("flavor_text_entries", out var flavorTexts))
                {
                    var germanText = flavorTexts.EnumerateArray()
                        .FirstOrDefault(f => f.GetProperty("language").GetProperty("name").GetString() == "de");

                    if (germanText.ValueKind != JsonValueKind.Undefined)
                    {
                        pokemon.FlavorText = germanText.GetProperty("flavor_text").GetString() ?? "";
                    }
                    else
                    {
                        var englishText = flavorTexts.EnumerateArray()
                            .FirstOrDefault(f => f.GetProperty("language").GetProperty("name").GetString() == "en");
                        pokemon.FlavorText = englishText.ValueKind != JsonValueKind.Undefined
                            ? englishText.GetProperty("flavor_text").GetString() ?? ""
                            : "";
                    }

                    pokemon.FlavorText = pokemon.FlavorText.Replace("\n", " ").Replace("\f", " ").Trim();
                }

                // Set sprite image path
                pokemon.ImagePath = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{i}.png";

                _pokemons.Add(pokemon);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
