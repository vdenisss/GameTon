using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class CrosswordGenerator
{
    private readonly int _rows;
    private readonly int _cols;
    private readonly char[,] _grid;
    private readonly Random _random = new Random();

    public CrosswordGenerator(int rows, int cols)
    {
        _rows = rows;
        _cols = cols;
        _grid = new char[rows, cols];
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                _grid[i, j] = ' ';
            }
        }
    }

    public void Generate(List<string> words)
    {
        var sortedWords = words.OrderByDescending(w => w.Length).ToList();

        foreach (var word in sortedWords)
        {
            if (!TryPlaceWord(word))
            {
                Console.WriteLine($"Не удалось разместить слово: {word}");
            }
        }

        FillEmptySpaces();
    }

    private bool TryPlaceWord(string word)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            bool horizontal = _random.Next(2) == 0;

            if (horizontal)
            {
                int row = _random.Next(_rows);
                int col = _random.Next(_cols - word.Length + 1);

                if (CanPlaceWordHorizontally(word, row, col))
                {
                    PlaceWordHorizontally(word, row, col);
                    return true;
                }
            }
            else
            {
                int row = _random.Next(_rows - word.Length + 1);
                int col = _random.Next(_cols);

                if (CanPlaceWordVertically(word, row, col))
                {
                    PlaceWordVertically(word, row, col);
                    return true;
                }
            }
        }

        return false;
    }

    private bool CanPlaceWordHorizontally(string word, int row, int col)
    {
        if (col + word.Length > _cols) return false;

        for (int i = 0; i < word.Length; i++)
        {
            char gridChar = _grid[row, col + i];
            if (gridChar != ' ' && gridChar != word[i])
            {
                return false;
            }
        }

        return true;
    }

    private void PlaceWordHorizontally(string word, int row, int col)
    {
        for (int i = 0; i < word.Length; i++)
        {
            _grid[row, col + i] = word[i];
        }
    }

    private bool CanPlaceWordVertically(string word, int row, int col)
    {
        if (row + word.Length > _rows) return false;

        for (int i = 0; i < word.Length; i++)
        {
            char gridChar = _grid[row + i, col];
            if (gridChar != ' ' && gridChar != word[i])
            {
                return false;
            }
        }

        return true;
    }

    private void PlaceWordVertically(string word, int row, int col)
    {
        for (int i = 0; i < word.Length; i++)
        {
            _grid[row + i, col] = word[i];
        }
    }

    private void FillEmptySpaces()
    {
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                if (_grid[i, j] == ' ')
                {
                    _grid[i, j] = GetRandomLetter();
                }
            }
        }
    }

    private char GetRandomLetter()
    {
        return (char)('А' + _random.Next(32));
    }

    public void PrintGrid()
    {
        Console.WriteLine("Сканворд:");
        Console.WriteLine(new string('-', _cols * 2 + 1));

        for (int i = 0; i < _rows; i++)
        {
            Console.Write("|");
            for (int j = 0; j < _cols; j++)
            {
                Console.Write(_grid[i, j] + "|");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', _cols * 2 + 1));
        }
    }

    // Новый метод для загрузки слов из файла
    public static List<string> LoadWordsFromFile(string filePath)
    {
        List<string> words = new List<string>();

        try
        {
            string[] lines = File.ReadAllLines("C:\\Users\\denis\\source\\repos\\GameTon\\GameTon\\words.txt", Encoding.UTF8);
            foreach (string line in lines)
            {
                // Разбиваем строку на слова (разделители: пробел, запятая, точка с запятой)
                string[] lineWords = line.Split(new[] { ' ', ',', ';' },
                                              StringSplitOptions.RemoveEmptyEntries);

                //foreach (string word in lineWords)
                //{
                //    // Удаляем лишние символы и приводим к верхнему регистру
                //    string cleanedWord = new string(word.Where(c => char.IsLetter(c)).ToUpper();
                //    if (!string.IsNullOrEmpty(cleanedWord))
                //    {
                //        words.Add(cleanedWord);
                //    }
                //}
            }

            Console.WriteLine($"Загружено {words.Count} слов из файла.");
            return words;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            return words;
        }
    }
}