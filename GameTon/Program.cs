using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GameTon;
using System.Text.Json;

class Program
{
    public static string fileWithWords = "C:\\Users\\denis\\source\\repos\\GameTon\\GameTon\\words.txt";

    static void Main(string[] args)
    {
        Words.GetWordsInFile(fileWithWords).Wait();
        var allWords = File.ReadLines(fileWithWords).ToList();

        /// Получим первое горизонтальное слово.
        var wordsInfo = new List<WordInfo>();
        var firstWords = allWords
            .Select((word, index) => new { Word = word, Index = index })
            .Where(x => x.Word.Length > 8);

        var indexInFiestWords = 0;
        var firstWord = firstWords.ElementAt(indexInFiestWords);

        var targetFirstWords = new Dictionary<int, string>();
        var targetSecondWords = new Dictionary<int, string>();

        while (!targetFirstWords.Any() || !targetSecondWords.Any())
        {
            /// Получим все слова, которые можно поставить для горизонтального слова по индексу 2.
            targetFirstWords = Words.SearchOneWordForLetterInPosition(allWords, 8, firstWord.Word[2]).Where(x => x.Value.Length >= 8).ToDictionary(x => x.Key, x => x.Value);
            /// Получим все слова, которые можно поставить для горизонтального слова по индексу 6.
            targetSecondWords = Words.SearchOneWordForLetterInPosition(allWords, 8, firstWord.Word[6]).Where(x => x.Value.Length >= 8).ToDictionary(x => x.Key, x => x.Value);

            if (!targetFirstWords.Any() || !targetSecondWords.Any())
            {
                indexInFiestWords++;
                firstWord = firstWords.ElementAt(indexInFiestWords);
            }
        }
        var firstVerticalWord = targetFirstWords.ElementAt(0);
        var secondVerticalWord = targetSecondWords.ElementAt(0);

        var minHeightWord = Math.Min(firstVerticalWord.Value.Length, secondVerticalWord.Value.Length) - 1;

        var secondHorizontalWords = new Dictionary<int, string>();
        var indexHeight = minHeightWord;
        for (var i = 1; i < minHeightWord - 1; i++)
        {
            var tempSecondHorizontalWords = Words.SearchTwoWordsForLetterInPosition(allWords, 1, 5, firstVerticalWord.Value[i], secondVerticalWord.Value[i]);
            if (tempSecondHorizontalWords != null && tempSecondHorizontalWords.Values.Count > 0
                && !(tempSecondHorizontalWords.Values.Count == 1 && tempSecondHorizontalWords.ContainsValue(firstWord.Word)))
            {
                secondHorizontalWords.Add(tempSecondHorizontalWords.FirstOrDefault().Key, tempSecondHorizontalWords.FirstOrDefault().Value);
                indexHeight = minHeightWord - i;
                break;
            }
        }

        var secondHorizontalWord = secondHorizontalWords.FirstOrDefault();

        Console.WriteLine($"ПЕРВОЕ ГОРИЗОНТАЛЬНОЕ СЛОВО: Слово: {firstWord.Word}, Index: {firstWord.Index}");
        Console.WriteLine($"ПЕРВОЕ ВЕРТИКАЛЬНОЕ СЛОВО: Слово: {firstVerticalWord.Value}, Index: {firstVerticalWord.Key}");
        Console.WriteLine($"ВТОРОЕ ВЕРТИКАЛЬНОЕ СЛОВО: Слово: {secondVerticalWord.Value}, Index: {secondVerticalWord.Key}");
        Console.WriteLine($"ВТОРОЕ ГОРИЗОНТАЛЬНОЕ СЛОВО: Слово: {secondHorizontalWord.Value}, Index: {secondHorizontalWord.Key}");

        /// Добавим его в объект запроса.
        var listWordsRequest = new List<WordInfo>()
        {
            new WordInfo() { Direction = 2, Id = firstWord.Index, Position = { 5, 5, 0 } },
            new WordInfo() { Direction = 1, Id = firstVerticalWord.Key, Position = { 7, 5, firstVerticalWord.Value.Length - 1 } },
            new WordInfo() { Direction = 1, Id = secondVerticalWord.Key, Position = { 11, 5, firstVerticalWord.Value.Length - 1, } },
            new WordInfo() { Direction = 2, Id = secondHorizontalWord.Key, Position = { 6, 5, indexHeight } }
        };

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var wordsRequest = new WordsResponse { Done = true, Words = listWordsRequest };

        string jsonRequest = JsonSerializer.Serialize<WordsResponse>(wordsRequest, options);
        Console.WriteLine(jsonRequest);
    }
}