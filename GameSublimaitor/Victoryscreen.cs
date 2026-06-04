using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using ElementGame.Managers;
namespace ElementGame.Screens
{
    public class VictoryScreen : BaseScreen
    {
        private double _timer = 0;
        private const double CloseAfter = 37.0; /* секунд до автозакриття */

        public VictoryScreen(ScreenManager sm) : base(sm) { }

        public override void Initialize()
        {
            /* зупиняємо фонову музику і вмикаємо музику перемоги */
            MediaPlayer.Stop();
            try
            {
                var victory = Game1.Instance.AssetManager.VictoryMusic;
                if (victory != null)
                {
                    MediaPlayer.IsRepeating = false; /* грає один раз */
                    MediaPlayer.Play(victory);
                }
            }
            catch { }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _timer += gameTime.ElapsedGameTime.TotalSeconds;
            /* після закінчення таймера закриваємо гру */
            if (_timer >= CloseAfter)
                Game1.Instance.Exit();
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int W = ScreenManager.Width;
            int H = ScreenManager.Height;

            sb.Begin();

            /* фон перемоги, запасний варіант якщо текстура не завантажилась */
            var bg = Assets.VictoryBg;
            if (bg != null)
                sb.Draw(bg, new Rectangle(0, 0, W, H), Color.White);
            else
                sb.Draw(Assets.Table, new Rectangle(0, 0, W, H), Color.DarkGoldenrod);

            /* зворотній відлік до закриття знизу екрана */
            int remaining = (int)(CloseAfter - _timer) + 1;
            int timerH = (int)(H * 0.07f);
            DrawCenteredText(sb, $"Закриття через: {remaining}",
                new Rectangle(0, H - timerH - (int)(H * 0.03f), W, timerH),
                Color.White * 0.8f);

            sb.End();
        }
    }
}