using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ElementGame.Managers;
namespace ElementGame.Screens
{
    public class GameFieldScreen : BaseScreen
    {
        private PlayerManager _playerManager;
        private int _playerIndex;

        public GameFieldScreen(ScreenManager sm, PlayerManager pm, int idx) : base(sm)
        {
            _playerManager = pm;
            _playerIndex = idx;
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int W = ScreenManager.Width;
            int H = ScreenManager.Height;
            int S = System.Math.Min(W, H); /* менша сторона — база для масштабу */

            sb.Begin();
            DrawBackground(sb, Assets.Table);

            /* розміри іконок і відступів відносно розміру екрана */
            int iconSize = (int)(S * 0.13f);
            int margin = (int)(S * 0.025f);
            int labelH = (int)(S * 0.04f);

            /* кнопка книги у лівому верхньому куті */
            var bookRect = new Rectangle(margin, margin, iconSize, iconSize);
            sb.Draw(Assets.BookBtn, bookRect, Color.White);
            DrawCenteredText(sb, "Book",
                new Rectangle(margin, margin + iconSize + 4, iconSize, labelH),
                Color.White);
            if (IsClicked(bookRect))
                ScreenManager.Push(new BookScreen(ScreenManager, _playerManager, _playerIndex));

            /* кнопка налаштувань у правому верхньому куті */
            var gearRect = new Rectangle(W - iconSize - margin, margin, iconSize, iconSize);
            sb.Draw(Assets.GearBtn, gearRect, Color.White);
            DrawCenteredText(sb, "Gear",
                new Rectangle(W - iconSize - margin, margin + iconSize + 4, iconSize, labelH),
                Color.White);
            if (IsClicked(gearRect))
                ScreenManager.Push(new SettingsScreen(ScreenManager, fromGame: true));

            /* котел по центру екрана, більший за інші кнопки */
            int boilerSize = (int)(S * 0.30f);
            int boilerX = (W - boilerSize) / 2;
            int boilerY = (H - boilerSize) / 2;
            var boilerRect = new Rectangle(boilerX, boilerY, boilerSize, boilerSize);
            sb.Draw(Assets.BoilerBtn, boilerRect, Color.White);
            DrawCenteredText(sb, "Boiler",
                new Rectangle(boilerX, boilerY + boilerSize + 4, boilerSize, labelH),
                Color.White);
            if (IsClicked(boilerRect))
                ScreenManager.Push(new BoilerScreen(ScreenManager, _playerManager, _playerIndex));

            sb.End();
        }
    }
}