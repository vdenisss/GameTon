using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GameTon
{
    internal class Words
    {
        public void GetWords()
        {
            string apiUrl = "https://api.example.com/protected-data";
            string authToken = "c2862f30-2848-48c5-ac44-e310b98bec6d";

            using (var client = new WebClient())
            {
                // Добавляем заголовок авторизации
                client.Headers[HttpRequestHeader.Authorization] = $"Bearer {authToken}";

                // Добавляем заголовок для указания типа контента (если нужно)
                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                try
                {
                    string response = client.DownloadString(apiUrl);
                    Console.WriteLine(response);
                }
                catch (WebException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    if (ex.Response != null)
                    {
                        using (var stream = ex.Response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            Console.WriteLine(reader.ReadToEnd());
                        }
                    }
                }
            }
        }
    }
}
