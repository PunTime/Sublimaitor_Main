using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ElementGame.Managers;

namespace ElementGame.Screens
{
    /* Проста кнопка з текстом і кольором фону */
    public class Button
    {
        public Rectangle Rect { get; set; }
        public string Text { get; set; }
        public Color BgColor { get; set; }
        public Color TextColor { get; set; }

        public Button(Rectangle rect, string text, Color bg, Color textColor)
        {
            Rect = rect;
            Text = text;
            BgColor = bg;
            TextColor = textColor;
        }
    }

    /* Базовий клас для всіх екранів гри */
    public abstract class BaseScreen
    {
        protected ScreenManager ScreenManager { get; }
        protected AssetManager Assets => Game1.Instance.AssetManager;
        protected SettingsManager Settings => Game1.Instance.SettingsManager;

        protected MouseState CurrentMouse;
        protected MouseState PreviousMouse;

        protected BaseScreen(ScreenManager sm) { ScreenManager = sm; }

        public virtual void Initialize() { }
        public virtual void OnResume() { }

        /* Update оновлює мишу і логіку екрану */
        public virtual void Update(GameTime gameTime)
        {
            PreviousMouse = CurrentMouse;
            CurrentMouse = Mouse.GetState();
        }

        public abstract void Draw(GameTime gameTime, SpriteBatch sb);

        /* Малює текстуру на весь екран як фон */
        protected void DrawBackground(SpriteBatch sb, Texture2D tex)
        {
            if (tex == null) return;
            sb.Draw(tex, new Rectangle(0, 0, ScreenManager.Width, ScreenManager.Height), Color.White);
        }

        /* Малює кнопку Button і повертає true якщо по ній клікнули */
        protected bool DrawBtn(SpriteBatch sb, Button btn)
        {
            sb.Draw(Assets.Table, btn.Rect, btn.BgColor);
            DrawCenteredText(sb, btn.Text, btn.Rect, btn.TextColor);
            return IsClicked(btn.Rect);
        }

        /* Малює кнопку (PNG), повертає true якщо натиснута */
        protected bool DrawButton(SpriteBatch sb, Texture2D tex, Rectangle rect, out bool hovered)
        {
            var mp = new Point(CurrentMouse.X, CurrentMouse.Y);
            hovered = rect.Contains(mp);
            Color tint = hovered ? new Color(180, 180, 180) : Color.White;
            sb.Draw(tex ?? Assets.Table, rect, tint);
            return IsClicked(rect);
        }

        /* Виводить текст по центру прямокутника, автоматично масштабує якщо не влазить */
        protected void DrawCenteredText(SpriteBatch sb, string text, Rectangle rect, Color color)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (Assets.DefaultFont != null)
            {
                var font = Assets.DefaultFont;
                var size = font.MeasureString(text);
                float scaleX = size.X > rect.Width - 4 ? (rect.Width - 4) / size.X : 1f;
                float scaleY = size.Y > rect.Height - 4 ? (rect.Height - 4) / size.Y : 1f;
                float scale = System.Math.Min(scaleX, scaleY);
                var pos = new Vector2(
                    rect.X + (rect.Width - size.X * scale) / 2f,
                    rect.Y + (rect.Height - size.Y * scale) / 2f);
                sb.DrawString(font, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            else
            {
                var bf = Assets.FallbackFont;
                float scale = 2f;
                var size = bf.MeasureString(text, scale);
                if (size.X > rect.Width - 4)
                    scale = scale * (rect.Width - 4) / size.X;
                if (scale < 1f) scale = 1f;
                size = bf.MeasureString(text, scale);
                if (size.X > rect.Width) scale = 1f;
                size = bf.MeasureString(text, scale);
                var pos = new Vector2(
                    rect.X + (rect.Width - size.X) / 2f,
                    rect.Y + (rect.Height - size.Y) / 2f);
                bf.DrawString(sb, text, pos, color, scale);
            }
        }

        /* Клік = права кнопка була натиснута і щойно відпущена над rect */
        protected bool IsClicked(Rectangle rect)
            => rect.Contains(new Point(CurrentMouse.X, CurrentMouse.Y))
            && CurrentMouse.RightButton == ButtonState.Released
            && PreviousMouse.RightButton == ButtonState.Pressed;
    }
}