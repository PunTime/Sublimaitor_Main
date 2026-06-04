using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace ElementGame.Managers
{
    /* Менеджер ресурсів гри. Відповідає за завантаження та зберігання
       всіх текстур, музики та шрифтів через XNA ContentManager. */
    public class AssetManager
    {
        /* ContentManager — стандартний завантажувач ресурсів MonoGame.
           Читає скомпільовані .xnb файли з папки Content. */
        private ContentManager _content;

        /* GraphicsDevice потрібен для створення «заглушок» (1×1 текстур)
           коли реальний файл не знайдено. */
        private GraphicsDevice _gd;

        /* Публічні властивості з приватним сетером — зовні можна лише читати,
           змінює значення тільки сам AssetManager. */
        public Texture2D Logo { get; private set; }
        public Texture2D Table { get; private set; }
        public Texture2D FuseBg { get; private set; }
        public Texture2D ExitBtn { get; private set; }
        public Texture2D SettingBtn { get; private set; }
        public Texture2D StartBtn { get; private set; }
        public Texture2D BookBtn { get; private set; }
        public Texture2D GearBtn { get; private set; }
        public Texture2D BoilerBtn { get; private set; }

        /* Словник текстур елементів: ключ — назва елемента ("Fire", "Water" тощо),
           значення — відповідна текстура. Ініціалізується порожнім при створенні. */
        public Dictionary<string, Texture2D> Elements { get; private set; } = new();

        public Song BackgroundMusic { get; private set; }
        public Song VictoryMusic { get; private set; }
        public Texture2D VictoryBg { get; private set; }

        /* DefaultFont завантажується з .xnb; може бути null якщо файл відсутній. */
        public SpriteFont DefaultFont { get; private set; }

        /* FallbackFont — власна растрова реалізація шрифту, завжди існує
           як запасний варіант коли DefaultFont не вдалось завантажити. */
        public BitmapFont FallbackFont { get; private set; }

        /* Конструктор отримує залежності ззовні (Dependency Injection).
           Нічого не завантажує — лише зберігає посилання. */
        public AssetManager(ContentManager content, GraphicsDevice gd)
        {
            _content = content;
            _gd = gd;
        }

        /* Завантажує всі ресурси гри. Викликається один раз під час
           ініціалізації (зазвичай у Game.LoadContent). */
        public void LoadAll()
        {
            /* Завантаження UI-текстур через безпечний метод LoadSafe —
               якщо файл відсутній, повернеться біла заглушка 1×1. */
            Logo = LoadSafe("Logo");
            Table = LoadSafe("Table");
            FuseBg = LoadSafe("Fuse");
            ExitBtn = LoadSafe("Exit");
            SettingBtn = LoadSafe("Seting");
            StartBtn = LoadSafe("Start");
            BookBtn = LoadSafe("Book");
            GearBtn = LoadSafe("Gear");
            BoilerBtn = LoadSafe("Boiler");

            /* Базові елементи — 4 початкові, з яких гравець починає гру. */
            string[] baseElements = { "Fire", "Water", "Wind", "Earth" };
            foreach (var e in baseElements)
                Elements[e] = LoadSafe(e);

            /* Синтезовані елементи — отримуються шляхом комбінування базових. */
            string[] synthElements = {
                "Steam","Mud","Sand","Cloud","Ash",
                "Brick","Thunder","Stone","Metal",
                "Magnet","Artefact","LifeStone"
            };
            foreach (var e in synthElements)
                Elements[e] = LoadSafe(e);

            /* Музика загортається в try/catch окремо, бо Song не підтримує
               заглушку як Texture2D — при помилці просто залишається null. */
            try { BackgroundMusic = _content.Load<Song>("music"); } catch { }
            try { VictoryMusic = _content.Load<Song>("Victory"); } catch { }

            VictoryBg = LoadSafe("Win_Victory");

            /* Спроба завантажити SpriteFont. Якщо .xnb відсутній —
               DefaultFont = null, і гра використовуватиме FallbackFont. */
            try { DefaultFont = _content.Load<SpriteFont>("DefaultFont"); } catch { }

            /* BitmapFont створюється програмно через GraphicsDevice,
               тому не потребує зовнішнього файлу і завжди успішна. */
            FallbackFont = new BitmapFont(_gd);
        }

        /* Безпечне завантаження текстури за іменем.
           Якщо файл не знайдено або пошкоджено — повертає білий піксель 1×1
           замість виключення, щоб гра не крашилась через відсутній asset. */
        private Texture2D LoadSafe(string name)
        {
            try { return _content.Load<Texture2D>(name); }
            catch
            {
                /* Створення мінімальної текстури-заглушки розміром 1×1 пікселів. */
                var t = new Texture2D(_gd, 1, 1);
                /* Заповнення білим кольором — видно на екрані як біла пляма,
                   що допомагає помітити відсутній ресурс під час розробки. */
                t.SetData(new[] { Microsoft.Xna.Framework.Color.White });
                return t;
            }
        }

        /* Повертає текстуру елемента за назвою.
           Якщо елемент не знайдено у словнику — повертає null
           (викличний код повинен перевіряти результат перед використанням). */
        public Texture2D GetElement(string name)
            => Elements.TryGetValue(name, out var tex) ? tex : null;
    }
}