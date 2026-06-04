using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ElementGame.Managers;
using System.Collections.Generic;

namespace ElementGame.Screens
{
    public class ElementDetailScreen : BaseScreen
    {
        private PlayerManager _playerManager;
        private int _playerIndex;
        private string _element;
        private bool _isUnlocked;

        /* всі рецепти гри, дублюємо пари бо порядок інгредієнтів не важливий */
        private static readonly List<(string A, string B, string Result)> AllRecipes =
            new List<(string, string, string)>
        {
            ("Fire",     "Water",    "Steam"),
            ("Water",    "Fire",     "Steam"),
            ("Earth",    "Water",    "Mud"),
            ("Water",    "Earth",    "Mud"),
            ("Earth",    "Wind",     "Sand"),
            ("Wind",     "Earth",    "Sand"),
            ("Steam",    "Wind",     "Cloud"),
            ("Wind",     "Steam",    "Cloud"),
            ("Earth",    "Fire",     "Ash"),
            ("Fire",     "Earth",    "Ash"),
            ("Sand",     "Fire",     "Ash"),
            ("Fire",     "Sand",     "Ash"),
            ("Mud",      "Fire",     "Brick"),
            ("Fire",     "Mud",      "Brick"),
            ("Fire",     "Cloud",    "Thunder"),
            ("Cloud",    "Fire",     "Thunder"),
            ("Brick",    "Fire",     "Stone"),
            ("Fire",     "Brick",    "Stone"),
            ("Stone",    "Fire",     "Metal"),
            ("Fire",     "Stone",    "Metal"),
            ("Thunder",  "Metal",    "Magnet"),
            ("Metal",    "Thunder",  "Magnet"),
            ("Magnet",   "Cloud",    "Artefact"),
            ("Cloud",    "Magnet",   "Artefact"),
            ("Artefact", "Cloud",    "LifeStone"),
            ("Cloud",    "Artefact", "LifeStone"),
        };

        public ElementDetailScreen(ScreenManager sm, PlayerManager pm,
            int idx, string element, bool isUnlocked) : base(sm)
        {
            _playerManager = pm;
            _playerIndex = idx;
            _element = element;
            _isUnlocked = isUnlocked;
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int W = ScreenManager.Width;
            int H = ScreenManager.Height;

            sb.Begin();
            sb.Draw(Assets.Table, new Rectangle(0, 0, W, H), Color.White);

            var unlocked = _playerManager.GetPlayer(_playerIndex).UnlockedElements;
            bool isBase = BookScreen.BaseElements.Contains(_element);

            /* центральна панель з деталями */
            int panelW = (int)(W * 0.80f);
            int panelH = (int)(H * 0.82f);
            int panelX = (W - panelW) / 2;
            int panelY = (int)(H * 0.06f);
            sb.Draw(Assets.Table, new Rectangle(panelX, panelY, panelW, panelH), Color.WhiteSmoke * 0.95f);

            /* рамка панелі */
            int b = 3;
            sb.Draw(Assets.Table, new Rectangle(panelX, panelY, panelW, b), Color.SaddleBrown * 0.6f);
            sb.Draw(Assets.Table, new Rectangle(panelX, panelY + panelH - b, panelW, b), Color.SaddleBrown * 0.6f);
            sb.Draw(Assets.Table, new Rectangle(panelX, panelY, b, panelH), Color.SaddleBrown * 0.6f);
            sb.Draw(Assets.Table, new Rectangle(panelX + panelW - b, panelY, b, panelH), Color.SaddleBrown * 0.6f);

            int pad = (int)(panelW * 0.05f);
            int contentX = panelX + pad;
            int contentW = panelW - pad * 2;

            /* велика іконка елемента зверху по центру панелі */
            int iconSize = (int)(System.Math.Min(W, H) * 0.14f);
            int iconX = panelX + (panelW - iconSize) / 2;
            int iconY = panelY + (int)(panelH * 0.04f);
            int nameH = (int)(panelH * 0.09f);

            if (_isUnlocked)
            {
                var tex = Assets.GetElement(_element);
                if (tex != null)
                    sb.Draw(tex, new Rectangle(iconX, iconY, iconSize, iconSize), Color.White);
                DrawCenteredText(sb, _element,
                    new Rectangle(panelX, iconY + iconSize + 4, panelW, nameH),
                    Color.SaddleBrown);
            }
            else
            {
                /* елемент ще не відкрито */
                DrawCenteredText(sb, "???", new Rectangle(iconX, iconY, iconSize, iconSize), Color.Gray);
                DrawCenteredText(sb, "???",
                    new Rectangle(panelX, iconY + iconSize + 4, panelW, nameH),
                    Color.Gray);
            }

            int contentY = iconY + iconSize + (int)(panelH * 0.12f);

            if (isBase)
            {
                /* для базових елементів просто пишемо що вони початкові */
                int msgH = (int)(panelH * 0.10f);
                DrawCenteredText(sb, "Початковий елемент",
                    new Rectangle(contentX, contentY, contentW, msgH), Color.DarkGoldenrod);
                contentY += msgH + (int)(panelH * 0.04f);
                DrawCenteredText(sb, "Доступний з початку гри",
                    new Rectangle(contentX, contentY, contentW, (int)(panelH * 0.07f)), Color.Gray);
            }
            else
            {
                /* показуємо список рецептів де цей елемент є результатом */
                int hdrH = (int)(panelH * 0.08f);
                DrawCenteredText(sb, "Рецепти:",
                    new Rectangle(contentX, contentY, contentW, hdrH), Color.SteelBlue);
                contentY += hdrH + (int)(panelH * 0.01f);

                int lineH = (int)(panelH * 0.075f);
                int lineSpacing = (int)(panelH * 0.005f);

                foreach (var (A, B, Result) in AllRecipes)
                {
                    if (Result != _element) continue;

                    string dispA = unlocked.Contains(A) ? A : "???";
                    string dispB = unlocked.Contains(B) ? B : "???";
                    string dispR = _isUnlocked ? Result : "???";
                    string line = $"{dispA} + {dispB} --> {dispR}";

                    bool known = unlocked.Contains(A) && unlocked.Contains(B);
                    Color lineColor = known ? Color.DarkSlateGray : Color.Gray * 0.65f;

                    sb.Draw(Assets.Table,
                        new Rectangle(contentX, contentY, contentW, lineH),
                        known ? Color.SteelBlue * 0.08f : Color.Gray * 0.05f);

                    DrawCenteredText(sb, line,
                        new Rectangle(contentX, contentY, contentW, lineH), lineColor);

                    contentY += lineH + lineSpacing;
                    if (contentY > panelY + panelH - (int)(panelH * 0.15f)) break;
                }
            }

            /* кнопка назад всередині панелі знизу */
            int backW = (int)(W * 0.20f);
            int backH = (int)(H * 0.08f);
            var backBtn = new Button(
                new Rectangle(panelX + (panelW - backW) / 2,
                              panelY + panelH - backH - (int)(panelH * 0.03f),
                              backW, backH),
                "Назад", Color.SaddleBrown * 0.85f, Color.White);

            if (DrawBtn(sb, backBtn)) ScreenManager.Pop();

            sb.End();
        }
    }
}