using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameTon
{
    // Классы для десериализации ответа
    public class ApiResponse
    {
        public int[] MapSize { get; set; }
        public int Turn { get; set; }
        public int NextTurnSec { get; set; }
        public int[] UsedIndexes { get; set; }
        public DateTime RoundEndsAt { get; set; }
        public int ShuffleLeft { get; set; }
        public string[] Words { get; set; }
    }
    public static class Words
    {
        public static async Task GetWordsInFile(string outputFile)
        {
            string apiUrl = "https://games-test.datsteam.dev/api/words";
            string authToken = "c2862f30-2848-48c5-ac44-e310b98bec6d";

            var client = new HttpClient();
            // Настройка клиента
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", authToken);

            try
            {
                // Отправка GET-запроса
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Настройки десериализации
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    // Десериализация
                    var result = JsonSerializer.Deserialize<ApiResponse>(responseBody, options);

                    using (StreamWriter writer = new StreamWriter(outputFile))
                    {
                        foreach (string word in result.Words)
                        {
                            await writer.WriteLineAsync(word);
                        }
                    }

                    // Вывод результата
                    //Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
                    //{
                    //    WriteIndented = true,
                    //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    //}));
                }
                else
                {
                    Console.WriteLine($"Ошибка: {response.StatusCode}");
                    Console.WriteLine(responseBody);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Ошибка запроса: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Ошибка парсинга JSON: {e.Message}");
            }
        }

        public static Dictionary<int, string> SearchTwoWordsForLetterInPosition(List<string> words,
            int firstIndex, int secondIndex, char firstLetter, char secondLetter)
        {
            return words
            .Select((word, index) => (word, index)) // Сохраняем индекс каждого слова
            .Where(item =>
                item.word.Length > Math.Max(firstIndex, secondIndex) && // Проверяем, что слово достаточно длинное
                char.ToLower(item.word[firstIndex]) == char.ToLower(firstLetter) && // Первая буква
                char.ToLower(item.word[secondIndex]) == char.ToLower(secondLetter))   // Вторая буква
            .ToDictionary(item => item.index, item => item.word);

        }

        public static Dictionary<int, string> SearchOneWordForLetterInPosition(List<string> words,
            int firstIndex, char firstLetter)
        {
            return words
            .Select((word, index) => (word, index)) // Сохраняем индекс каждого слова
            .Where(item =>
                item.word.Length == firstIndex + 1 &&
                char.ToLower(item.word[firstIndex]) == char.ToLower(firstLetter))
            .ToDictionary(item => item.index, item => item.word);

        }
    }
}
