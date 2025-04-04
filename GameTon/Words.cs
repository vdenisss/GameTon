using System;
using System.Collections.Generic;
using System.IO;
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
        private static readonly HttpClient _client = new HttpClient();

        public static async Task GetWords()
        {
            string apiUrl = "https://games-test.datsteam.dev/api/words";
            string authToken = "c2862f30-2848-48c5-ac44-e310b98bec6d";
            string outputFile = "C:\\Users\\denis\\source\\repos\\GameTon\\GameTon\\words.txt";

            // Настройка клиента
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("X-Auth-Token", authToken);

            try
            {
                // Отправка GET-запроса
                HttpResponseMessage response = await _client.GetAsync(apiUrl);
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
                    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
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
    }
}
