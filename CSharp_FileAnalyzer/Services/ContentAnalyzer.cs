using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;


namespace CSharp_FileAnalyzer.Services
{
    public class ContentAnalyzer
    {
        private static readonly HashSet<string> Conjunctions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ve", "veya", "ile", "da", "de", "ki",
                "ama", "çünkü", "ise", "ancak", "zira", "madem"
            };

        public void Analyze(string text)
        {
            CultureInfo turkishCulture =
                new CultureInfo("tr-TR");

            int punctuationCount = text.Count(char.IsPunctuation);

            Console.WriteLine("[Noktalama İşareti Sayısı]: " + punctuationCount);

            string cleanedText = new string(
                text.Select(c => char.IsPunctuation(c) ? ' ' : c).ToArray()
            );

            string[] allWords = cleanedText.Split(
                new[] { ' ', '\r', '\n', '\t' },
                StringSplitOptions.RemoveEmptyEntries
            );

            Dictionary<string, int> wordCounts =
                new Dictionary<string, int>(
                    StringComparer.Create(turkishCulture, true)
                );

            int totalDifferentWords = 0;

            foreach (string word in allWords)
            {
                string lowercaseWord =
                    word.ToLower(turkishCulture);

                if (string.IsNullOrEmpty(lowercaseWord) ||
                    double.TryParse(lowercaseWord, out _) ||
                    Conjunctions.Contains(lowercaseWord))
                {
                    continue;
                }

                if (wordCounts.ContainsKey(lowercaseWord))
                {
                    wordCounts[lowercaseWord]++;
                }
                else
                {
                    wordCounts[lowercaseWord] = 1;
                    totalDifferentWords++;
                }
            }

            Console.WriteLine(
                "[Toplam Farklı Kelime Sayısı]: " +
                totalDifferentWords + "\n"
            );

            Console.WriteLine(
                "--- En Çok Tekrar Eden Kelimeler (Sıralı) ---"
            );

            var sortedWords =
                wordCounts.OrderByDescending(x => x.Value);

            foreach (var item in sortedWords)
            {
                Console.WriteLine(
                    item.Key + ": " + item.Value + " kez"
                );
            }
        }
    }
}
