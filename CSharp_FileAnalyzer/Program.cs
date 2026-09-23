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
    public interface IDosyaokuyucu
    {
        string Dosyaoku(string yol);

    }
    //TXT dosyalarını okumaktan sorumlu sınıf
    public class TXTOkuyucu : IDosyaokuyucu
    {
        public string Dosyaoku(string yol)
        {
            return File.ReadAllText(yol);
        }
    }
    //Word(.docx) dosyalarını okumaktan sorumlu sınıf
    public class DocxOkuyucu : IDosyaokuyucu
    {
        public string Dosyaoku(string yol)
        {
            System.Text.StringBuilder icerik = new System.Text.StringBuilder();
            using (var wordDosyasi = Xceed.Words.NET.DocX.Load(yol))
            {
                foreach (var paragraf in wordDosyasi.Paragraphs)
                {
                    icerik.AppendLine(paragraf.Text);
                }
            }
            return icerik.ToString();
        }
    }
    internal class Program
    {
        //Heaplamaya dahil edilmeyecek bağlaçların listesi
        private static readonly HashSet<string> Baglaclar = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ve","veya","ile","da","de","ki","ama","çünkü","ise","ancak","zira","madem"
        };

        //Windows dosya penceresinin konsolda kararlı çalışmasını sağlayan zorunlu ayar
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Dosya analizi başlatılıyor...");
            Console.WriteLine("Lütfen dosya seçiniz");
            //Dosya seçim penceresi
            OpenFileDialog dosyaSecici = new OpenFileDialog();
            dosyaSecici.Filter = "Desteklenen Dosyalar|*.txt;*.docx";
            dosyaSecici.Title = "Analiz edilecek dosyayı seçin";
            if (dosyaSecici.ShowDialog() == DialogResult.OK)
            {
                string secilenDosyaYolu = dosyaSecici.FileName;
                string uzanti = Path.GetExtension(secilenDosyaYolu).ToLower();
                // Geçersiz veya var olmayan dosya yolu kontrolü
                if (!File.Exists(secilenDosyaYolu))
                {
                    HataLogla("Geçersiz dosya yolu! Dosya bulunamadı: " + secilenDosyaYolu);
                    return;
                }
                try
                {
                    IDosyaokuyucu okuyucu = null;
                    //Dosya uzantısına göre ilgili okuyucu sınıfı devreye giriyor 
                    if (uzanti == ".txt") okuyucu = new TXTOkuyucu();
                    else if (uzanti == ".docx") okuyucu = new DocxOkuyucu();
                    if(okuyucu != null)
                    {
                        string metin = okuyucu.Dosyaoku(secilenDosyaYolu);

                        //Ekranı temizleyip düzenli yazdırdığımız kısım
                        Console.Clear();
                        Console.WriteLine("===Analiz Raporu===");
                        Console.WriteLine($"Dosya: {Path.GetFileName(secilenDosyaYolu)}\n");
                        //Analizi yapan fonksiyonuçağıran kısım
                        AnalizEtVeRaporla(metin);
                        //Başarılı olan işlemi kaydetirdiğimiz kısm
                        Logla($"Başarılı Analiz: {secilenDosyaYolu}");

                    }
                    else
                    {
                        HataLogla("Desteklenmeyen dosya formatı.");
                    }
                }
                catch(Exception ex)
                {
                    HataLogla($"Dosya işlenirken hata oluştu: {ex.Message}");
                }
            }
            Console.WriteLine("\n Çıkmak için bir tuşa basınız.");
            Console.ReadKey();
        }
        public static void Logla(string mesaj)
        {
            string logMetni = "[" + DateTime.Now + "] INFO: " + mesaj + "\n";
            File.AppendAllText("uygulama_log.txt", logMetni);
        }

        // [Madde 7] Hataları hem ekrana basar hem de "uygulama_log.txt" dosyasına kaydeder
        public static void HataLogla(string mesaj)
        {
            Console.WriteLine("\nHata: " + mesaj);
            string logMetni = "[" + DateTime.Now + "] ERROR: " + mesaj + "\n";
            File.AppendAllText("uygulama_log.txt", logMetni);
        }
        public static void AnalizEtVeRaporla(string metin)
        {
            // 1. ADIM: Bilgisayara Türkçe dil kurallarını tanımlıyoruz
            System.Globalization.CultureInfo turkceKultur = new System.Globalization.CultureInfo("tr-TR");

            // Noktalama işaretlerini sayıyoruz
            int noktalamaSayisi = metin.Count(char.IsPunctuation);
            Console.WriteLine("[Noktalama İşareti Sayısı]: " + noktalamaSayisi);

            // 2. ADIM: Tüm noktalama işaretlerini tek tek boşluğa çeviriyoruz
            string temizMetin = new string(metin.Select(c => char.IsPunctuation(c) ? ' ' : c).ToArray());

            // Metni sadece boşluklara, satır başlarına göre temiz kelimelere bölüyoruz
            string[] tumKelimeler = temizMetin.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // 3. ADIM: Sözlüğü (Dictionary) Türkçe büyük/küçük harfe duyarsız yapıyoruz
            Dictionary<string, int> kelimeSayilari = new Dictionary<string, int>(StringComparer.Create(turkceKultur, true));
            int toplamFarkliKelime = 0;

            foreach (string kelime in tumKelimeler)
            {
                // Kelimeyi Türkçe kurallarına göre tamamen küçük harfe çeviriyoruz
                string kucukKelime = kelime.ToLower(turkceKultur);

                if (string.IsNullOrEmpty(kucukKelime) || double.TryParse(kucukKelime, out _) || Baglaclar.Contains(kucukKelime))
                {
                    continue;
                }

                if (kelimeSayilari.ContainsKey(kucukKelime))
                {
                    kelimeSayilari[kucukKelime]++;
                }
                else
                {
                    kelimeSayilari[kucukKelime] = 1;
                    toplamFarkliKelime++;
                }
            }

            Console.WriteLine("[Toplam Farklı Kelime Sayısı]: " + toplamFarkliKelime + "\n");
            Console.WriteLine("--- En Çok Tekrar Eden Kelimeler (Sıralı) ---");

            var siraliKelimeler = kelimeSayilari.OrderByDescending(x => x.Value);
            foreach (var sira in siraliKelimeler)
            {
                Console.WriteLine(sira.Key + ": " + sira.Value + " kez");
            }
        }

    }
} 

    


