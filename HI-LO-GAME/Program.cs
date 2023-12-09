using Microsoft.Extensions.Configuration;

var settingsService = new SettingsService();

var interfaceService = new InterfaceService();

interfaceService.DisplayGameInfo(settingsService.RangeMin, settingsService.RangeMax);

interfaceService.DisplayContinueOption();

IEnumerable<PlayerGuess> playerList = interfaceService.PromptPlayerSelection();

interfaceService.GenerateSecretNumber(playerList, settingsService);

List<WallOfFame> wallOfFame = new List<WallOfFame>();

var roundCounter = 0;

while (true)
{
    roundCounter++;

    interfaceService.DisplayRount(roundCounter);

    interfaceService.PromptPlayerGuess(playerList);

    var winners = playerList.Where(g => g.SecretNumber == g.Guess)
                            .Select(p => p as PlayerInfo);

    if (winners.Any())
    {
        interfaceService.AddToWallOfFame(winners, wallOfFame, roundCounter);

        interfaceService.DisplayWinners(winners);

        interfaceService.DisplayContinueOption();

        interfaceService.DisplayWallOfFame(wallOfFame);

        bool restart = interfaceService.PromptRestartOption();

        if (restart)
        {
            interfaceService.GenerateSecretNumber(playerList, settingsService);

            roundCounter = 0;

            continue;
        }
        else
            break;
    }
    else
    {
        interfaceService.DisplayHits(playerList);

        interfaceService.DisplayContinueOption();
    }
}

#region Models

/// <summary>
/// Player Information
/// </summary>
internal class PlayerInfo
{
    /// <summary>
    /// Unique ID of the Player
    /// </summary>
    internal int ID { get; set; }
    /// <summary>
    /// The Name of the player
    /// </summary>
    internal string? Name { get; set; }
    /// <summary>
    /// Player info display
    /// </summary>
    internal string Display { get { return $"Player {ID} - {Name}"; } }
}

internal class PlayerGuess : PlayerInfo
{
    internal int SecretNumber { get; set; }
    internal int? Guess { get; set; }
}

internal class WallOfFame
{
    internal string PlayerName { get; set; }

    internal int NeededGuesses { get; set; }

    internal DateTime Date { get; set; }
}

#endregion

#region Sevices

internal class SettingsService
{
    private readonly int _rangeMin;
    private readonly int _rangeMax;

    public SettingsService()
    {
        var configurationBuilder = new ConfigurationBuilder().AddJsonFile($"settings.json");
        var settings = configurationBuilder.Build();
        _rangeMin = int.Parse(settings["RangeMin"]);
        _rangeMax = int.Parse(settings["RangeMax"]);
    }

    public int RangeMin => _rangeMin;
    public int RangeMax => _rangeMax;

    internal int GenerateSecretNumber()
    {
        return new Random().Next(_rangeMin, _rangeMax + 1);
    }
}

internal class InterfaceService
{
    internal string CollectInputString()
    {
        Console.Write(" -> ");
        return Console.ReadLine();
    }

    internal int CollectInputInt()
    {
        while (true)
        {
            Console.Write(" -> ");

            var input = Console.ReadLine();

            if (int.TryParse(input, out _))
            {
                return int.Parse(input);
            }
        }
    }

    internal void DisplayGameInfo(int rangeMin, int rangeMax)
    {
        Console.Clear();

        Console.WriteLine($"Welcome to HI-LO-GAME");
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine($"In this game you have to find a secret number that's in this range [{rangeMin}-{rangeMax}]");
    }

    internal IEnumerable<PlayerGuess> PromptPlayerSelection()
    {
        var response = new List<PlayerGuess>();

        Console.Clear();

        int auxCounter = 0;

        while (true)
        {
            auxCounter++;

            Console.Clear();
            Console.WriteLine($"Please type the name of Player {auxCounter}" +
                              ((auxCounter > 1) ? " (or press [ENTER] to start playing)" : ""));

            string input = CollectInputString();

            if (string.IsNullOrEmpty(input))
            {
                if (auxCounter == 1)
                {
                    auxCounter = 0;
                    continue;
                }
                else
                    break;
            }
            else
            {
                response.Add(new PlayerGuess() { ID = auxCounter, Name = input });
            }
        }

        Console.Clear();

        return response;
    }

    internal void DisplayRount(int roundNumber)
    {
        Console.Clear();
        Console.WriteLine($"Round {roundNumber} !");
    }

    internal void PromptPlayerGuess(IEnumerable<PlayerGuess> playerList)
    {
        Console.Clear();

        foreach (var player in playerList)
        {
            Console.WriteLine($"{player.Display}'s guess:");

            player.Guess = CollectInputInt();
        }

        Console.Clear();
    }

    internal void GenerateSecretNumber(IEnumerable<PlayerGuess> playerList, SettingsService settingsService)
    {
        foreach (var player in playerList)
        {
            player.SecretNumber = settingsService.GenerateSecretNumber();
        }
    }

    internal void DisplayWinners(IEnumerable<PlayerInfo> winners)
    {
        Console.Clear();

        foreach (var player in winners)
        {
            Console.WriteLine($"Congratulations [{player.Display}], you found your Secret Number!");
        }
    }

    internal bool PromptRestartOption()
    {
        Console.WriteLine(Environment.NewLine);

        Console.WriteLine($"Type [Y] to play again");

        return CollectInputString().ToUpper() == "Y";
    }

    internal void DisplayHits(IEnumerable<PlayerGuess> playerGuessList)
    {
        Console.Clear();

        foreach (var playerGuess in playerGuessList)
        {
            if (playerGuess.SecretNumber > playerGuess.Guess)
                Console.WriteLine($"{playerGuess.Display}: HI: the mystery number is > the player's guess");
            else
                Console.WriteLine($"{playerGuess.Display}: LO: the mystery number is < the player's guess");
        }
    }

    internal void DisplayContinueOption()
    {
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine($"Press [ENTER] to continue");
        Console.ReadLine();
        Console.Clear();
    }

    internal void AddToWallOfFame(IEnumerable<PlayerInfo> winners, List<WallOfFame> wallOfFameList, int roundNumber)
    {
        foreach (var winner in winners)
        {
            wallOfFameList.Add(new WallOfFame() { PlayerName = winner.Name, NeededGuesses = roundNumber, Date = DateTime.Now });
        }
    }

    internal void DisplayWallOfFame(IEnumerable<WallOfFame> wallOfFameList)
    {
        Console.Clear();

        Console.WriteLine("WALL OF FAME");

        Console.WriteLine(Environment.NewLine);

        foreach (var item in wallOfFameList.OrderBy(w => w.NeededGuesses)
                                           .ThenBy(w => w.Date)
                                           .Take(10))
        {
            Console.WriteLine($"{item.Date.ToShortDateString()} - {item.NeededGuesses} Guesses - {item.PlayerName}");
        }
    }
}

#endregion