using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ElementGame.Managers
{
    /* Піксельний шрифт 5x7 — не потребує .spritefont файлу.
       Використовується коли SpriteFont не завантажився. */
    public class BitmapFont
    {
        private Texture2D _pixel; // білий піксель 1x1 яким малюємо все
        private GraphicsDevice _gd;

        /* Словник гліфів: символ → 7 рядків по 5 біт */
        private static readonly Dictionary<char, byte[]> Glyphs = BuildGlyphs();

        public int CharWidth => 5;
        public int CharHeight => 7;
        public int Spacing => 1; // відступ між символами

        /* Створюємо білий піксель — він буде перефарбовуватись через Color при Draw */
        public BitmapFont(GraphicsDevice gd)
        {
            _gd = gd;
            _pixel = new Texture2D(gd, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        /* Рахує розміри рядка в пікселях з урахуванням масштабу */
        public Vector2 MeasureString(string text, float scale = 1f)
        {
            if (string.IsNullOrEmpty(text)) return Vector2.Zero;
            float w = text.Length * (CharWidth + Spacing) * scale;
            float h = CharHeight * scale;
            return new Vector2(w, h);
        }

        /* Малює текст піксель за пікселем по бітовій масці гліфа */
        public void DrawString(SpriteBatch sb, string text, Vector2 pos, Color color, float scale = 1f)
        {
            if (string.IsNullOrEmpty(text)) return;
            float cx = pos.X;
            int pw = (int)(scale);
            if (pw < 1) pw = 1; // мінімум 1px інакше нічого не видно

            foreach (char c in text)
            {
                char key = char.ToUpper(c);
                /* Якщо символу нема — беремо '?' як заглушку */
                if (!Glyphs.TryGetValue(key, out var rows))
                    rows = Glyphs.TryGetValue('?', out var q) ? q : new byte[7];

                /* Перебираємо 7 рядків і 5 колонок гліфа */
                for (int row = 0; row < 7; row++)
                {
                    byte bits = rows[row];
                    for (int col = 0; col < 5; col++)
                    {
                        /* Перевіряємо чи встановлений біт для цього пікселя */
                        if ((bits & (1 << (4 - col))) != 0)
                        {
                            sb.Draw(_pixel,
                                new Rectangle(
                                    (int)cx + col * pw,
                                    (int)pos.Y + row * pw,
                                    pw, pw),
                                color);
                        }
                    }
                }
                cx += (CharWidth + Spacing) * scale; // зсуваємось до наступного символу
            }
        }

        /* Малює текст по центру прямокутника, зменшує якщо не влазить */
        public void DrawStringCentered(SpriteBatch sb, string text, Rectangle rect, Color color, float scale = 1f)
        {
            if (string.IsNullOrEmpty(text)) return;
            var size = MeasureString(text, scale);

            /* Автомасштаб якщо текст ширший за кнопку */
            if (size.X > rect.Width - 4)
            {
                scale = scale * (rect.Width - 4) / size.X;
                size = MeasureString(text, scale);
            }

            var pos = new Vector2(
                rect.X + (rect.Width - size.X) / 2f,
                rect.Y + (rect.Height - size.Y) / 2f);

            DrawString(sb, text, pos, color, scale);
        }

        /* Будує таблицю всіх підтримуваних символів.
           Кожен гліф — масив з 7 байт, де кожен байт це рядок пікселів (біти 4..0 = колонки 0..4) */
        private static Dictionary<char, byte[]> BuildGlyphs()
        {
            var g = new Dictionary<char, byte[]>();

            /* Латиниця A-Z */
            g['A'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b00000 };
            g['B'] = new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10001, 0b11110, 0b00000 };
            g['C'] = new byte[] { 0b01110, 0b10001, 0b10000, 0b10000, 0b10001, 0b01110, 0b00000 };
            g['D'] = new byte[] { 0b11100, 0b10010, 0b10001, 0b10001, 0b10010, 0b11100, 0b00000 };
            g['E'] = new byte[] { 0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b11111, 0b00000 };
            g['F'] = new byte[] { 0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b00000 };
            g['G'] = new byte[] { 0b01110, 0b10001, 0b10000, 0b10111, 0b10001, 0b01111, 0b00000 };
            g['H'] = new byte[] { 0b10001, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b00000 };
            g['I'] = new byte[] { 0b01110, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110, 0b00000 };
            g['J'] = new byte[] { 0b00111, 0b00010, 0b00010, 0b00010, 0b10010, 0b01100, 0b00000 };
            g['K'] = new byte[] { 0b10001, 0b10010, 0b10100, 0b11000, 0b10100, 0b10001, 0b00000 };
            g['L'] = new byte[] { 0b10000, 0b10000, 0b10000, 0b10000, 0b10000, 0b11111, 0b00000 };
            g['M'] = new byte[] { 0b10001, 0b11011, 0b10101, 0b10001, 0b10001, 0b10001, 0b00000 };
            g['N'] = new byte[] { 0b10001, 0b11001, 0b10101, 0b10011, 0b10001, 0b10001, 0b00000 };
            g['O'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110, 0b00000 };
            g['P'] = new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10000, 0b10000, 0b00000 };
            g['Q'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b10101, 0b10010, 0b01101, 0b00000 };
            g['R'] = new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10100, 0b10001, 0b00000 };
            g['S'] = new byte[] { 0b01111, 0b10000, 0b10000, 0b01110, 0b00001, 0b11110, 0b00000 };
            g['T'] = new byte[] { 0b11111, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00000 };
            g['U'] = new byte[] { 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110, 0b00000 };
            g['V'] = new byte[] { 0b10001, 0b10001, 0b10001, 0b10001, 0b01010, 0b00100, 0b00000 };
            g['W'] = new byte[] { 0b10001, 0b10001, 0b10001, 0b10101, 0b11011, 0b10001, 0b00000 };
            g['X'] = new byte[] { 0b10001, 0b01010, 0b00100, 0b00100, 0b01010, 0b10001, 0b00000 };
            g['Y'] = new byte[] { 0b10001, 0b10001, 0b01010, 0b00100, 0b00100, 0b00100, 0b00000 };
            g['Z'] = new byte[] { 0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b11111, 0b00000 };

            /* Цифри 0-9 */
            g['0'] = new byte[] { 0b01110, 0b10011, 0b10101, 0b10101, 0b11001, 0b01110, 0b00000 };
            g['1'] = new byte[] { 0b00100, 0b01100, 0b00100, 0b00100, 0b00100, 0b01110, 0b00000 };
            g['2'] = new byte[] { 0b01110, 0b10001, 0b00001, 0b00110, 0b01000, 0b11111, 0b00000 };
            g['3'] = new byte[] { 0b11110, 0b00001, 0b00001, 0b01110, 0b00001, 0b11110, 0b00000 };
            g['4'] = new byte[] { 0b00010, 0b00110, 0b01010, 0b10010, 0b11111, 0b00010, 0b00000 };
            g['5'] = new byte[] { 0b11111, 0b10000, 0b11110, 0b00001, 0b00001, 0b11110, 0b00000 };
            g['6'] = new byte[] { 0b01110, 0b10000, 0b11110, 0b10001, 0b10001, 0b01110, 0b00000 };
            g['7'] = new byte[] { 0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b01000, 0b00000 };
            g['8'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b01110, 0b10001, 0b01110, 0b00000 };
            g['9'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b01111, 0b00001, 0b01110, 0b00000 };

            /* Розділові знаки та спецсимволи */
            g[' '] = new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b00000 };
            g[':'] = new byte[] { 0b00000, 0b00100, 0b00000, 0b00000, 0b00100, 0b00000, 0b00000 };
            g['%'] = new byte[] { 0b11001, 0b11010, 0b00100, 0b00100, 0b01011, 0b10011, 0b00000 };
            g['/'] = new byte[] { 0b00001, 0b00010, 0b00100, 0b00100, 0b01000, 0b10000, 0b00000 };
            g['.'] = new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b00100, 0b00000 };
            g[','] = new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b00100, 0b01000, 0b00000 };
            g['!'] = new byte[] { 0b00100, 0b00100, 0b00100, 0b00100, 0b00000, 0b00100, 0b00000 };
            g['?'] = new byte[] { 0b01110, 0b10001, 0b00010, 0b00100, 0b00000, 0b00100, 0b00000 };
            g['-'] = new byte[] { 0b00000, 0b00000, 0b00000, 0b11111, 0b00000, 0b00000, 0b00000 };
            g['_'] = new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b11111, 0b00000 };
            g['+'] = new byte[] { 0b00000, 0b00100, 0b00100, 0b11111, 0b00100, 0b00100, 0b00000 };
            g['>'] = new byte[] { 0b10000, 0b01000, 0b00100, 0b00010, 0b00100, 0b01000, 0b10000 };
            g['='] = new byte[] { 0b00000, 0b11111, 0b00000, 0b00000, 0b11111, 0b00000, 0b00000 };
            g['|'] = new byte[] { 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100 };
            g['\''] = new byte[] { 0b00100, 0b00100, 0b00000, 0b00000, 0b00000, 0b00000, 0b00000 };

            /* Кирилиця — унікальні літери малюються вручну,
               схожі на латинські просто посилаються на той самий гліф */
            g['А'] = g['A']; g['Б'] = new byte[] { 0b11111, 0b10000, 0b11110, 0b10001, 0b10001, 0b11110, 0b00000 };
            g['В'] = g['B']; g['Г'] = new byte[] { 0b11111, 0b10000, 0b10000, 0b10000, 0b10000, 0b10000, 0b00000 };
            g['Д'] = new byte[] { 0b00110, 0b01010, 0b01010, 0b01010, 0b11111, 0b10001, 0b00000 };
            g['Е'] = g['E']; g['Є'] = g['E'];
            g['Ж'] = new byte[] { 0b10101, 0b10101, 0b01110, 0b01110, 0b10101, 0b10101, 0b00000 };
            g['З'] = new byte[] { 0b11110, 0b00001, 0b00001, 0b01110, 0b00001, 0b11110, 0b00000 };
            g['И'] = new byte[] { 0b10001, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b00000 };
            g['І'] = g['I']; g['Й'] = g['I'];
            g['К'] = g['K']; g['Л'] = new byte[] { 0b00111, 0b00101, 0b00101, 0b00101, 0b10101, 0b11001, 0b00000 };
            g['М'] = g['M']; g['Н'] = g['H']; g['О'] = g['O'];
            g['П'] = new byte[] { 0b11111, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b00000 };
            g['Р'] = g['P']; g['С'] = g['C']; g['Т'] = g['T'];
            g['У'] = g['Y']; g['Ф'] = new byte[] { 0b00100, 0b01110, 0b10101, 0b10101, 0b01110, 0b00100, 0b00000 };
            g['Х'] = g['X']; g['Ц'] = new byte[] { 0b10010, 0b10010, 0b10010, 0b10010, 0b11111, 0b00001, 0b00000 };
            g['Ч'] = new byte[] { 0b10001, 0b10001, 0b10001, 0b01111, 0b00001, 0b00001, 0b00000 };
            g['Ш'] = new byte[] { 0b10101, 0b10101, 0b10101, 0b10101, 0b10101, 0b11111, 0b00000 };
            g['Щ'] = new byte[] { 0b10101, 0b10101, 0b10101, 0b10101, 0b11111, 0b00001, 0b00000 };
            g['Ь'] = new byte[] { 0b10000, 0b10000, 0b11110, 0b10001, 0b10001, 0b11110, 0b00000 };
            g['Ю'] = new byte[] { 0b10010, 0b10101, 0b10101, 0b11101, 0b10101, 0b10010, 0b00000 };
            g['Я'] = new byte[] { 0b01111, 0b10001, 0b10001, 0b01111, 0b00101, 0b10001, 0b00000 };
            g['Ї'] = g['I']; g['Ґ'] = g['Г'];

            /* Малі літери — просто копіюємо гліфи з великих,
               бо при розмірі 5x7 різниця все одно не видна */
            foreach (var kv in new Dictionary<char, byte[]>(g))
            {
                char lower = char.ToLower(kv.Key);
                if (!g.ContainsKey(lower)) g[lower] = kv.Value;
            }

            return g;
        }
    }
}