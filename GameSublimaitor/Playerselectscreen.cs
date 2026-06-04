using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ElementGame.Managers;
namespace ElementGame.Screens
{
    public class PlayerSelectScreen : BaseScreen
    {
        private PlayerManager _playerManager;

        public PlayerSelectScreen(ScreenManager sm) : base(sm)
        {
            /* створюємо менеджер і одразу завантажуємо збережені дані */
            _playerManager = new PlayerManager();
            _playerManager.Load();
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int W = ScreenManager.Width;
            int H = ScreenManager.Height;

            sb.Begin();
            DrawBackground(sb, Assets.Table);

            /* заголовок адаптивний до розміру екрана */
            int titleH = (int)(H * 0.09f);
            DrawCenteredText(sb, "Оберіть гравця",
                new Rectangle(0, (int)(H * 0.06f), W, titleH), Color.White);

            /* 4 кнопки рівномірно по ширині з однаковими відступами */
            int marginX = (int)(W * 0.04f);
            int spacing = (int)(W * 0.025f);
            int btnW = (W - marginX * 2 - spacing * 3) / 4;
            int btnH = (int)(H * 0.28f);
            int btnY = (int)(H * 0.35f);

            for (int i = 0; i < 4; i++)
            {
                int x = marginX + i * (btnW + spacing);
                var rect = new Rectangle(x, btnY, btnW, btnH);
                sb.Draw(Assets.Table, rect, Color.DarkSlateBlue * 0.85f);

                int labelH = (int)(btnH * 0.3f);

                /* якщо гравець переміг — показуємо "Winer" замість "Player" */
                bool hasLifeStone = _playerManager.GetPlayer(i).UnlockedElements.Contains("LifeStone");
                DrawCenteredText(sb, hasLifeStone ? $"Winer {i + 1}" : $"Player {i + 1}",
                    new Rectangle(x, btnY + (int)(btnH * 0.12f), btnW, labelH),
                    Color.Yellow);

                /* збережене ім'я знизу кнопки */
                DrawCenteredText(sb, _playerManager.GetPlayer(i).Name,
                    new Rectangle(x, btnY + btnH - labelH - (int)(btnH * 0.12f), btnW, labelH),
                    Color.LightGray);

                /* клік відкриває екран введення імені для цього гравця */
                if (IsClicked(rect))
                {
                    int captured = i; /* захоплюємо i щоб лямбда не замкнулась на змінну циклу */
                    ScreenManager.Push(new PlayerNameScreen(ScreenManager, _playerManager, captured));
                }
            }

            /* кнопка назад знизу по центру */
            int backW = (int)(W * 0.18f);
            int backH = (int)(H * 0.08f);
            var backRect = new Rectangle((W - backW) / 2, H - backH - (int)(H * 0.05f), backW, backH);
            sb.Draw(Assets.Table, backRect, Color.DarkGray * 0.85f);
            DrawCenteredText(sb, "Назад", backRect, Color.White);
            if (IsClicked(backRect))
                ScreenManager.Pop();

            sb.End();
        }
    }
}