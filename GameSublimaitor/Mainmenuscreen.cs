using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace ElementGame.Screens
{
    public class MainMenuScreen : BaseScreen
    {
        public MainMenuScreen(Managers.ScreenManager sm) : base(sm) { }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int W = ScreenManager.Width;
            int H = ScreenManager.Height;

            sb.Begin();
            DrawBackground(sb, Assets.Logo);

            /* розмір кнопки 20% від меншої сторони, щоб зберігати пропорції */
            int btnSize = (int)(System.Math.Min(W, H) * 0.20f);
            int spacing = (int)(W * 0.04f);

            /* центруємо три кнопки по горизонталі */
            int totalW = btnSize * 3 + spacing * 2;
            int startX = (W - totalW) / 2;

            /* кнопки трохи вище від низу екрана */
            int btnY = H - btnSize - (int)(H * 0.22f);

            var exitRect = new Rectangle(startX, btnY, btnSize, btnSize);
            var settingRect = new Rectangle(startX + btnSize + spacing, btnY, btnSize, btnSize);
            var startRect = new Rectangle(startX + (btnSize + spacing) * 2, btnY, btnSize, btnSize);

            /* малюємо тільки PNG-іконки, без тексту */
            sb.Draw(Assets.ExitBtn, exitRect, Color.White);
            sb.Draw(Assets.SettingBtn, settingRect, Color.White);
            sb.Draw(Assets.StartBtn, startRect, Color.White);

            if (IsClicked(exitRect)) Game1.Instance.Exit();
            if (IsClicked(settingRect)) ScreenManager.Push(new SettingsScreen(ScreenManager, fromGame: false));
            if (IsClicked(startRect)) ScreenManager.Push(new PlayerSelectScreen(ScreenManager));

            sb.End();
        }
    }
}