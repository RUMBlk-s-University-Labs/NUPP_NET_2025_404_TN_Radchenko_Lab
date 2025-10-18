using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Polls.RandomNames
{
    public static class RandomPersonName
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly Random random = new Random();

        private static List<string>? names;
        private static readonly object _cacheLock = new object();

        public static string Get()
        {
            lock (_cacheLock)
            {
                if (names == null)
                {
                    Console.WriteLine("Завантаження списку імен... ");
                    try
                    {
                        string content = httpClient.GetStringAsync("https://gist.githubusercontent.com/sorokadima/a6ba9a0864ef7bbd1648e198c14ddfb6/raw/4bf08de5d79ac26b3dda1391383ddff5e2cee83f/%25D0%25A3%25D0%25BA%25D1%2580%25D0%25B0%25D1%2597%25D0%25BD%25D1%2581%25D1%258C%25D0%25BA%25D1%2596%2520%25D1%2596%25D0%25BC%25D0%25B5%25D0%25BD%25D0%25B0%2520%25D1%2581%25D0%25BF%25D0%25B8%25D1%2581%25D0%25BE%25D0%25BA.txt")
                            .GetAwaiter().GetResult();

                        names = content
                            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => line.Trim())
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .ToList();
                        Console.WriteLine("Успішно!");
                    }
                    catch
                    {
                        Console.WriteLine("Помилка завантаження списку імен.");
                    }
                }
            }

            if (names != null && names.Any())
            {
                int i = random.Next(names.Count);
                return names[i];
            }

            return "Іван";
        }
    }

    public static class RandomString
    {
        private static readonly Random random = new Random();
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string Get(int length)
        {
            if (length <= 0)
            {
                return string.Empty;
            }
            
            return new string(Enumerable.Repeat(Chars, length)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }
    }
}