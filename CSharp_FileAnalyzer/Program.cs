using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharp_FileAnalyzer
{
    //Yeni dosya türleri eklendiğinde sistemi genişletmeyi sağlayan Arayüz
    public interface IFileReader
    {
        string ReadFile(string path);
    }

    //TXT dosyalarını okumaktan sorumlu sınıf
    public class TXTReader : IFileReader
    {
        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }
    }

    //Word(.docx) dosyalarını okumaktan sorumlu sınıf
    public class DocxReader : IFileReader
    {
        public string ReadFile(string path)
        {
            StringBuilder content = new StringBuilder();

            using (var wordFile = Xceed.Words.NET.DocX.Load(path))
            {
                foreach (var paragraph in wordFile.Paragraphs)
                {
                    content.AppendLine(paragraph.Text);
                }
            }

            return content.ToString();
        }
    }

    internal class Program
    {
        //Heaplamaya dahil edilmeyecek bağlaçların listesi
        private static readonly HashSet<string> Conjunctions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ve", "veya", "ile", "da", "de", "ki",
                "ama", "çünkü", "ise", "ancak", "zira", "madem"
            };

        //Windows dosya penceresinin konsolda kararlı çalışmasını sağlayan zorunlu ayar
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Dosya analizi başlatılıyor...");
            Console.WriteLine("Lütfen dosya seçiniz");

            //Dosya seçim penceresi
            OpenFileDialog fileSelector = new OpenFileDialog();
            fileSelector.Filter = "Desteklenen Dosyalar|*.txt;*.docx";
            fileSelector.Title = "Analiz edilecek dosyayı seçin";

            if (fileSelector.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = fileSelector.FileName;
                string extension = Path.GetExtension(selectedFilePath).ToLower();

                // Geçersiz veya var olmayan dosya yolu kontrolü
                if (!File.Exists(selectedFilePath))
                {
                    LogError("Geçersiz dosya yolu! Dosya bulunamadı: " + selectedFilePath);
                    return;
                }

                try
                {
                    IFileReader reader = null;

                    //Dosya uzantısına göre ilgili okuyucu sınıfı devreye giriyor
                    if (extension == ".txt")
                        reader = new TXTReader();
                    else if (extension == ".docx")
                        reader = new DocxReader();

                    if (reader != null)
                    {
                        string text = reader.ReadFile(selectedFilePath);

                        //Ekranı temizleyip düzenli yazdırdığımız kısım
                        Console.Clear();
                        Console.WriteLine("===Analiz Raporu===");
                        Console.WriteLine($"Dosya: {Path.GetFileName(selectedFilePath)}\n");

                        //Analizi yapan fonksiyonuçağıran kısım
                        AnalyzeAndReport(text);

                        //Başarılı olan işlemi kaydetirdiğimiz kısm
                        LogInfo($"Başarılı Analiz: {selectedFilePath}");
                    }
                    else
                    {
                        LogError("Desteklenmeyen dosya formatı.");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Dosya işlenirken hata oluştu: {ex.Message}");
                }
            }

            Console.WriteLine("\n Çıkmak için bir tuşa basınız.");
            Console.ReadKey();
        }

        public static void LogInfo(string message)
        {
            string logText = "[" + DateTime.Now + "] INFO: " + message + "\n";
            File.AppendAllText("uygulama_log.txt", logText);
        }

        // [Madde 7] Hataları hem ekrana basar hem de "uygulama_log.txt" dosyasına kaydeder
        public static void LogError(string message)
        {
            Console.WriteLine("\nHata: " + message);
            string logText = "[" + DateTime.Now + "] ERROR: " + message + "\n";
            File.AppendAllText("uygulama_log.txt", logText);
        }

        public static void AnalyzeAndReport(string text)
        {
            // 1. ADIM: Bilgisayara Türkçe dil kurallarını tanımlıyoruz
            System.Globalization.CultureInfo turkishCulture =
                new System.Globalization.CultureInfo("tr-TR");

            // Noktalama işaretlerini sayıyoruz
            int punctuationCount = text.Count(char.IsPunctuation);
            Console.WriteLine("[Noktalama İşareti Sayısı]: " + punctuationCount);

            // 2. ADIM: Tüm noktalama işaretlerini tek tek boşluğa çeviriyoruz
            string cleanedText = new string(
                text.Select(c => char.IsPunctuation(c) ? ' ' : c).ToArray()
            );

            // Metni sadece boşluklara, satır başlarına göre temiz kelimelere bölüyoruz
            string[] allWords = cleanedText.Split(
                new[] { ' ', '\r', '\n', '\t' },
                StringSplitOptions.RemoveEmptyEntries
            );

            // 3. ADIM: Sözlüğü (Dictionary) Türkçe büyük/küçük harfe duyarsız yapıyoruz
            Dictionary<string, int> wordCounts =
                new Dictionary<string, int>(
                    StringComparer.Create(turkishCulture, true)
                );

            int totalDifferentWords = 0;

            foreach (string word in allWords)
            {
                // Kelimeyi Türkçe kurallarına göre tamamen küçük harfe çeviriyoruz
                string lowercaseWord = word.ToLower(turkishCulture);

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
                "[Toplam Farklı Kelime Sayısı]: " + totalDifferentWords + "\n"
            );

            Console.WriteLine("--- En Çok Tekrar Eden Kelimeler (Sıralı) ---");

            var sortedWords = wordCounts.OrderByDescending(x => x.Value);

            foreach (var item in sortedWords)
            {
                Console.WriteLine(item.Key + ": " + item.Value + " kez");
            }
        }
    }
}




