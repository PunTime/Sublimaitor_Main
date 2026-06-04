using System;
using System.IO;
namespace ElementGame.Managers
{
    /* всі налаштування приховані за властивостями */
    public class SettingsManager
    {
        private const string SettingsFile = "settings.txt";

        /* значення за замовчуванням якщо файл не знайдено */
        public int ResolutionWidth { get; private set; } = 800;
        public int ResolutionHeight { get; private set; } = 600;
        public float Volume { get; private set; } = 0.6f;

        /* список підтримуваних розширень для меню налаштувань */
        public static readonly (int w, int h, string label)[] Resolutions = new[]
        {
            (640,  480,  "640x480"),
            (800,  600,  "800x600"),
            (1024, 768,  "1024x768"),
            (1280, 720,  "1280x720"),
            (1366, 768,  "1366x768"),
            (1920, 1080, "1920x1080"),
        };

        /* зберігаємо після кожної зміни розширення */
        public void SetResolution(int width, int height)
        {
            ResolutionWidth = width;
            ResolutionHeight = height;
            Save();
        }

        /* клампимо гучність щоб не виходила за 0..1 */
        public void SetVolume(float volume)
        {
            Volume = Math.Max(0f, Math.Min(1f, volume));
            Save();
        }

        /* три рядки: ширина, висота, гучність */
        public void Save()
        {
            try
            {
                using var sw = new StreamWriter(SettingsFile);
                sw.WriteLine(ResolutionWidth);
                sw.WriteLine(ResolutionHeight);
                sw.WriteLine(Volume.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            catch { /* мовчки ігноруємо помилки запису */ }
        }

        /* якщо файлу немає або він пошкоджений — лишаємо дефолти */
        public void Load()
        {
            try
            {
                if (!File.Exists(SettingsFile)) return;
                var lines = File.ReadAllLines(SettingsFile);
                if (lines.Length >= 3)
                {
                    ResolutionWidth = int.Parse(lines[0]);
                    ResolutionHeight = int.Parse(lines[1]);
                    /* інваріантна культура бо крапка може відрізнятись в локалях */
                    Volume = float.Parse(lines[2], System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch { /* використовуємо значення за замовчуванням */ }
        }
    }
}