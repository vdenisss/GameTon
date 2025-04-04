using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class WordTowerGame
{
    private readonly HttpClient _client;
    private readonly string _token;
    private readonly bool _useTestServer;

    public WordTowerGame(string token, bool useTestServer = false)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        _token = token.Trim();
        _useTestServer = useTestServer;

        _client = new HttpClient();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
    }

    private string BaseUrl => _useTestServer ?
        "https://games-test.datsteam.dev/api" :
        "https://games.datsteam.dev/api";

    public async Task PlayGame()
    {
        try
        {
            Console.WriteLine($"Starting game with token: {_token.Substring(0, 5)}...");

            // Первоначальная проверка токена
            var testResponse = await _client.GetAsync($"{BaseUrl}/towers");
            if (!testResponse.IsSuccessStatusCode)
            {
                var error = await testResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Authorization failed: {error}");
                return;
            }

            while (true)
            {
                var wordsResponse = await GetWords();
                if (wordsResponse?.Words == null || wordsResponse.Words.Count == 0)
                {
                    Console.WriteLine("No words received, ending game.");
                    break;
                }

                Console.WriteLine($"Received {wordsResponse.Words.Count} words");

                // Основная игровая логика
                await ProcessGameTurn(wordsResponse);

                if (wordsResponse.NextTurnSec <= 0)
                {
                    Console.WriteLine("Turn time ended");
                    break;
                }

                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Game finished.");
        }
    }

    private async Task ProcessGameTurn(WordsResponse wordsResponse)
    {
        var towers = await GetTowers();
        if (towers?.CurrentTower == null)
        {
            Console.WriteLine("Failed to get current tower");
            return;
        }

        if (towers.CurrentTower.Words == null || towers.CurrentTower.Words.Count == 0)
        {
            Console.WriteLine("Starting new tower");
            await StartNewTower(wordsResponse);
        }
        else
        {
            Console.WriteLine($"Continuing tower (height: {towers.CurrentTower.Words.Count})");
            await ContinueBuildingTower(wordsResponse, towers.CurrentTower);
        }
    }

    private async Task<WordsResponse> GetWords()
    {
        try
        {
            var response = await _client.GetAsync($"{BaseUrl}/words");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Words API error: {response.StatusCode} - {error}");
                return null;
            }
            return JsonConvert.DeserializeObject<WordsResponse>(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetWords exception: {ex.Message}");
            return null;
        }
    }

    private async Task<TowersResponse> GetTowers()
    {
        try
        {
            var response = await _client.GetAsync($"{BaseUrl}/towers");
            return response.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<TowersResponse>(await response.Content.ReadAsStringAsync())
                : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetTowers exception: {ex.Message}");
            return null;
        }
    }

    private async Task<bool> StartNewTower(WordsResponse wordsResponse)
    {
        try
        {
            var availableWords = wordsResponse.Words
                .Where(w => !wordsResponse.UsedIndexes.Contains(w.Id))
                .OrderByDescending(w => w.Text.Length)
                .Take(2)
                .ToList();

            if (availableWords.Count < 2)
            {
                Console.WriteLine("Not enough words to start tower");
                return false;
            }

            var buildWords = new List<BuildWord>
            {
                new BuildWord { Id = availableWords[0].Id, Dir = 2, Pos = new[] { 0, 0, 0 } },
                new BuildWord { Id = availableWords[1].Id, Dir = 3, Pos = new[] { 0, 0, 0 } }
            };

            return await BuildTower(buildWords);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"StartNewTower exception: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ContinueBuildingTower(WordsResponse wordsResponse, Tower tower)
    {
        try
        {
            var newWord = wordsResponse.Words
                .Where(w => !wordsResponse.UsedIndexes.Contains(w.Id))
                .OrderByDescending(w => w.Text.Length)
                .FirstOrDefault();

            if (newWord == null)
            {
                Console.WriteLine("No available words to continue");
                return await BuildTower(new List<BuildWord>(), true);
            }

            var lastZ = tower.Words.Min(w => w.Pos[2]) - 1;
            return await BuildTower(new List<BuildWord>
            {
                new BuildWord
                {
                    Id = newWord.Id,
                    Dir = tower.Words.Last().Dir == 2 ? 3 : 2, // Alternate direction
                    Pos = new[] { 0, 0, lastZ }
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ContinueBuildingTower exception: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> BuildTower(List<BuildWord> words, bool done = false)
    {
        try
        {
            var request = new BuildRequest { Words = words, Done = done };
            var response = await _client.PostAsync(
                $"{BaseUrl}/build",
                new StringContent(JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Build error: {response.StatusCode} - {error}");
                return false;
            }

            Console.WriteLine($"Successfully {(done ? "completed" : "built")} tower");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BuildTower exception: {ex.Message}");
            return false;
        }
    }
}

// Data models
public class WordsResponse
{
    [JsonProperty("words")]
    public List<Word> Words { get; set; } = new List<Word>();
    [JsonProperty("usedIndexes")]
    public List<int> UsedIndexes { get; set; } = new List<int>();
    [JsonProperty("nextTurnSec")]
    public int NextTurnSec { get; set; }
}

public class Word
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("word")]
    public string Text { get; set; } = string.Empty;
}

public class TowersResponse
{
    [JsonProperty("current")]
    public Tower CurrentTower { get; set; } = new Tower();
    [JsonProperty("completed")]
    public List<Tower> CompletedTowers { get; set; } = new List<Tower>();
}

public class Tower
{
    [JsonProperty("words")]
    public List<TowerWord> Words { get; set; } = new List<TowerWord>();
    [JsonProperty("score")]
    public int Score { get; set; }
}

public class TowerWord
{
    [JsonProperty("pos")]
    public int[] Pos { get; set; } = new int[3];
    [JsonProperty("dir")]
    public int Dir { get; set; }
}

public class BuildRequest
{
    [JsonProperty("words")]
    public List<BuildWord> Words { get; set; } = new List<BuildWord>();
    [JsonProperty("done")]
    public bool Done { get; set; }
}

public class BuildWord
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("dir")]
    public int Dir { get; set; }
    [JsonProperty("pos")]
    public int[] Pos { get; set; } = new int[3];
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter your game token:");
        var token = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("Invalid token");
            return;
        }

        Console.WriteLine("Use test server? (y/n)");
        var useTest = Console.ReadLine()?.ToLower() == "y";

        var game = new WordTowerGame(token, useTest);
        await game.PlayGame();
    }
}