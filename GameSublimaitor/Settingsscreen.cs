using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ElementGame.Managers;

namespace ElementGame.Screens
{
    public class SettingsScreen : BaseScreen
    {
        private bool _fromGame; /* чи відкрито з гри, впливає на кнопки внизу */
        private bool _dragging = false; /* чи тягнуть повзунок гучності */

        /* фіксовані кроки гучності замість плавного слайдера */
        private static readonly float[] VolumeSteps = { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };
        private static readonly string[] VolumeLabels = { "0%", "20%", "40%", "60%", "80%", "100%" };

        /* координати треку зберігаємо між Draw і Update */
        private int _trackX, _trackY, _trackW;

        public SettingsScreen(ScreenManager sm, bool fromGame = false) : base(sm)
        {
            _fromGame = fromGame;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_trackW <= 0) return; /* трек ще не намальований */

            /* перетягування повзунка правою кнопкою */
            if (CurrentMouse.RightButton == ButtonState.Pressed)
            {
                var trackRect = new Rectangle(_trackX, _trackY - 14, _trackW, 36);
                if (trackRect.Contains(new Point(CurrentMouse.X, CurrentMouse.Y)) || _dragging)
                {
                    _dragging = true;
                    /* знаходимо найближчий крок до позиції курсора */
                    float t = System.Math.Clamp((float)(CurrentMouse.X - _trackX) / _trackW, 0f, 1f);
                    int nearest = 0;
                    float minD = float.MaxValue;
                    for (int i = 0; i < VolumeSteps.Length; i++)
                    {
                        float d = System.MathF.Abs(VolumeSteps[i] - t);
                        if (d < minD) { minD = d; nearest = i; }
                    }
                    Settings.SetVolume(VolumeSteps[nearest]);
                    Game1.Instance.ApplyVolume();
                }
            }
            else { _dragging = false; }
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int W = ScreenManager.Width;
            int H = ScreenManager.Height;

            sb.Begin();
            DrawBackground(sb, Assets.Table);

            /* центральна панель налаштувань */
            int panelW = (int)(W * 0.66f);
            int panelH = (int)(H * 0.70f);
            int panelX = (W - panelW) / 2;
            int panelY = (H - panelH) / 2;
            int pad = (int)(panelW * 0.05f);

            sb.Draw(Assets.Table, new Rectangle(panelX, panelY, panelW, panelH), Color.Black * 0.68f);

            /* заголовок */
            int titleH = (int)(panelH * 0.11f);
            DrawCenteredText(sb, "НАЛАШТУВАННЯ",
                new Rectangle(panelX, panelY + (int)(panelH * 0.04f), panelW, titleH), Color.White);

            /* секція вибору розширення */
            int secY1 = panelY + (int)(panelH * 0.18f);
            int secLblH = (int)(panelH * 0.08f);
            DrawCenteredText(sb, "Розширення екрану:",
                new Rectangle(panelX, secY1, panelW, secLblH), Color.Yellow);

            /* кнопки розширень рівномірно в рядок */
            int resBtnH = (int)(panelH * 0.09f);
            int resY = secY1 + secLblH + (int)(panelH * 0.01f);
            int resCount = SettingsManager.Resolutions.Length;
            int resSpacing = (int)(panelW * 0.012f);
            int resBtnW = (panelW - pad * 2 - resSpacing * (resCount - 1)) / resCount;
            int resStartX = panelX + pad;

            for (int i = 0; i < resCount; i++)
            {
                var (rw, rh, label) = SettingsManager.Resolutions[i];
                bool isCur = Settings.ResolutionWidth == rw && Settings.ResolutionHeight == rh;
                var rect = new Rectangle(resStartX + i * (resBtnW + resSpacing), resY, resBtnW, resBtnH);
                /* активне розширення підсвічуємо жовтогарячим */
                sb.Draw(Assets.Table, rect, isCur ? Color.DarkOrange * 0.95f : Color.DimGray * 0.85f);
                DrawCenteredText(sb, label, rect, isCur ? Color.White : Color.LightGray);
                if (IsClicked(rect)) { Settings.SetResolution(rw, rh); Game1.Instance.ApplyResolution(); }
            }

            /* секція повзунка гучності */
            int secY2 = resY + resBtnH + (int)(panelH * 0.09f);
            DrawCenteredText(sb, "Гучність музики:",
                new Rectangle(panelX, secY2, panelW, secLblH), Color.Yellow);

            int trackMargin = pad;
            _trackW = panelW - trackMargin * 2;
            _trackX = panelX + trackMargin;
            _trackY = secY2 + secLblH + (int)(panelH * 0.06f);
            int trackH = (int)(panelH * 0.018f);

            /* сірий фон треку */
            sb.Draw(Assets.Table, new Rectangle(_trackX, _trackY, _trackW, trackH), Color.Gray * 0.9f);
            /* зелена заповнена частина відповідно до гучності */
            int filledW = (int)(_trackW * Settings.Volume);
            if (filledW > 0)
                sb.Draw(Assets.Table, new Rectangle(_trackX, _trackY, filledW, trackH), Color.LimeGreen);

            /* 6 точок-позначок на треку */
            int dotSize = (int)(panelH * 0.028f);
            int lblUnder = (int)(panelH * 0.055f);
            for (int i = 0; i < VolumeSteps.Length; i++)
            {
                int dotX = _trackX + (int)(VolumeSteps[i] * _trackW);
                bool isCur = System.Math.Abs(Settings.Volume - VolumeSteps[i]) < 0.01f;
                /* активна точка біла, решта сірі */
                sb.Draw(Assets.Table,
                    new Rectangle(dotX - dotSize / 2, _trackY - (dotSize - trackH) / 2, dotSize, dotSize),
                    isCur ? Color.White : Color.DarkGray);
                DrawCenteredText(sb, VolumeLabels[i],
                    new Rectangle(dotX - 28, _trackY + dotSize + 3, 56, lblUnder),
                    isCur ? Color.White : Color.Gray);
            }

            /* ручка повзунка, жовтогаряча під час перетягування */
            int handleSize = (int)(panelH * 0.042f);
            int handleX = _trackX + (int)(Settings.Volume * _trackW);
            sb.Draw(Assets.Table,
                new Rectangle(handleX - handleSize / 2, _trackY - (handleSize - trackH) / 2, handleSize, handleSize),
                _dragging ? Color.Orange : Color.White);

            /* числовий відсоток під треком */
            DrawCenteredText(sb, $"Гучність: {(int)(Settings.Volume * 100)}%",
                new Rectangle(panelX, _trackY + handleSize + lblUnder + 4, panelW, (int)(panelH * 0.07f)),
                Color.LightYellow);

            /* кнопки внизу: з гри — "Меню" + "Назад", з меню — тільки "Назад" */
            int btnH2 = (int)(panelH * 0.12f);
            int btnW2 = (int)(panelW * 0.28f);
            int btnY2 = panelY + panelH - btnH2 - (int)(panelH * 0.04f);

            if (_fromGame)
            {
                /* дві кнопки поряд */
                int gap = (int)(panelW * 0.05f);
                int twoW = btnW2 * 2 + gap;
                int twoX = panelX + (panelW - twoW) / 2;

                var menuRect = new Rectangle(twoX, btnY2, btnW2, btnH2);
                var closeRect = new Rectangle(twoX + btnW2 + gap, btnY2, btnW2, btnH2);

                sb.Draw(Assets.Table, menuRect, Color.DarkRed * 0.85f);
                DrawCenteredText(sb, "Меню", menuRect, Color.White);
                if (IsClicked(menuRect)) ScreenManager.PopToRoot(); /* виходимо в головне меню */

                sb.Draw(Assets.Table, closeRect, Color.DarkSlateGray * 0.85f);
                DrawCenteredText(sb, "Назад", closeRect, Color.White);
                if (IsClicked(closeRect)) ScreenManager.Pop();
            }
            else
            {
                /* одна кнопка по центру */
                var closeRect = new Rectangle(panelX + (panelW - btnW2) / 2, btnY2, btnW2, btnH2);
                sb.Draw(Assets.Table, closeRect, Color.DarkSlateGray * 0.85f);
                DrawCenteredText(sb, "Назад", closeRect, Color.White);
                if (IsClicked(closeRect)) ScreenManager.Pop();
            }

            sb.End();
        }
    }
}