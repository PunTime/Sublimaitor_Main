using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ElementGame.Managers;
using System.Text;
using System.IO;

namespace ElementGame.Screens
{
    public class PlayerNameScreen : BaseScreen
    {
        private PlayerManager _playerManager;
        private int _playerIndex;
        private StringBuilder _inputName = new StringBuilder(); /* поточний текст у полі вводу */
        private KeyboardState _prevKeys;

        private bool _confirmDelete = false; /* чи показуємо підтвердження видалення */

        public PlayerNameScreen(ScreenManager sm, PlayerManager pm, int idx) : base(sm)
        {
            _playerManager = pm;
            _playerIndex = idx;
            /* заповнюємо поле поточним іменем гравця */
            _inputName.Append(pm.GetPlayer(idx).Name);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var keys = Keyboard.GetState();
            HandleTextInput(keys);
            _prevKeys = keys;
        }

        private void HandleTextInput(KeyboardState keys)
        {
            bool shift = keys.IsKeyDown(Keys.LeftShift) || keys.IsKeyDown(Keys.RightShift);
            foreach (Keys key in keys.GetPressedKeys())
            {
                if (_prevKeys.IsKeyDown(key)) continue; /* ігноруємо утримані клавіші */

                /* backspace видаляє останній символ */
                if (key == Keys.Back && _inputName.Length > 0)
                { _inputName.Remove(_inputName.Length - 1, 1); continue; }

                if (_inputName.Length >= 15) continue; /* ліміт 15 символів */

                char? c = KeyToChar(key, shift);
                if (c.HasValue) _inputName.Append(c.Value);
            }
        }

        /* переводимо клавішу в символ, null якщо непідтримувана */
        private char? KeyToChar(Keys key, bool shift)
        {
            if (key >= Keys.A && key <= Keys.Z)
                return shift ? (char)('A' + (key - Keys.A)) : (char)('a' + (key - Keys.A));
            if (key >= Keys.D0 && key <= Keys.D9 && !shift)
                return (char)('0' + (key - Keys.D0));
            if (key == Keys.Space) return ' ';
            if (key == Keys.OemMinus && !shift) return '-';
            if (key == Keys.OemPeriod && !shift) return '.';
            return null;
        }

        private void DeleteSave()
        {
            /* скидаємо ім'я і елементи до початкових значень */
            string defaultName = $"Player {_playerIndex + 1}";
            _playerManager.SetPlayerName(_playerIndex, defaultName);
            _playerManager.GetPlayer(_playerIndex).UnlockedElements.Clear();
            _playerManager.GetPlayer(_playerIndex).UnlockedElements.AddRange(
                new[] { "Fire", "Water", "Wind", "Earth" });
            _playerManager.Save(_playerIndex);

            /* фізично видаляємо файл сейву */
            string path = $"player{_playerIndex + 1}.txt";
            if (File.Exists(path)) File.Delete(path);

            /* оновлюємо поле вводу і закриваємо підтвердження */
            _inputName.Clear();
            _inputName.Append(defaultName);
            _confirmDelete = false;
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int W = ScreenManager.Width;
            int H = ScreenManager.Height;

            sb.Begin();
            DrawBackground(sb, Assets.Table);

            /* напівпрозора панель по центру */
            int panelW = (int)(W * 0.52f);
            int panelH = (int)(H * 0.60f);
            int panelX = (W - panelW) / 2;
            int panelY = (H - panelH) / 2;
            sb.Draw(Assets.Table, new Rectangle(panelX, panelY, panelW, panelH), Color.Black * 0.72f);

            int pad = (int)(panelW * 0.05f);

            /* заголовок з номером гравця */
            int titleH = (int)(panelH * 0.14f);
            DrawCenteredText(sb,
                $"Введіть ім'я для Player {_playerIndex + 1}",
                new Rectangle(panelX, panelY + (int)(panelH * 0.05f), panelW, titleH),
                Color.Yellow);

            /* поле вводу з мигаючим курсором */
            int fieldY = panelY + (int)(panelH * 0.24f);
            int fieldH = (int)(panelH * 0.17f);
            int fieldX = panelX + pad;
            int fieldW = panelW - pad * 2;
            sb.Draw(Assets.Table, new Rectangle(fieldX, fieldY, fieldW, fieldH), Color.White * 0.18f);

            /* курсор блимає раз на пів секунди */
            string display = _inputName.ToString() + (System.DateTime.Now.Millisecond < 500 ? "|" : " ");
            DrawCenteredText(sb, display, new Rectangle(fieldX, fieldY, fieldW, fieldH), Color.White);

            /* лічильник символів під полем */
            int hintH = (int)(panelH * 0.10f);
            DrawCenteredText(sb, $"{_inputName.Length} / 15",
                new Rectangle(panelX, fieldY + fieldH + (int)(panelH * 0.02f), panelW, hintH),
                Color.Gray);

            /* кнопки назад і старт в один рядок */
            int btnW = (int)(panelW * 0.32f);
            int btnH = (int)(panelH * 0.15f);
            int gap = (int)(panelW * 0.06f);
            int row1Y = panelY + (int)(panelH * 0.56f);
            int btnsX = panelX + (panelW - btnW * 2 - gap) / 2;

            var backRect = new Rectangle(btnsX, row1Y, btnW, btnH);
            var startRect = new Rectangle(btnsX + btnW + gap, row1Y, btnW, btnH);

            sb.Draw(Assets.Table, backRect, Color.DarkGray * 0.85f);
            DrawCenteredText(sb, "Назад", backRect, Color.White);
            if (IsClicked(backRect)) ScreenManager.Pop();

            sb.Draw(Assets.Table, startRect, Color.DarkGreen * 0.85f);
            DrawCenteredText(sb, "Старт", startRect, Color.White);
            /* старт тільки якщо є хоч один символ */
            if (IsClicked(startRect) && _inputName.Length > 0)
            {
                _playerManager.SetPlayerName(_playerIndex, _inputName.ToString());
                ScreenManager.Push(new GameFieldScreen(ScreenManager, _playerManager, _playerIndex));
            }

            /* кнопка видалення сейву під основними кнопками */
            int delBtnW = (int)(panelW * 0.70f);
            int delBtnH = (int)(panelH * 0.13f);
            int delBtnX = panelX + (panelW - delBtnW) / 2;
            int delBtnY = row1Y + btnH + (int)(panelH * 0.04f);
            var delRect = new Rectangle(delBtnX, delBtnY, delBtnW, delBtnH);

            if (!_confirmDelete)
            {
                /* звичайний стан — одна кнопка видалення */
                sb.Draw(Assets.Table, delRect, Color.DarkRed * 0.75f);
                DrawCenteredText(sb, "Видалити сейв", delRect, Color.LightCoral);
                if (IsClicked(delRect))
                    _confirmDelete = true;
            }
            else
            {
                /* режим підтвердження — розбиваємо на дві кнопки */
                int halfW = (delBtnW - gap / 2) / 2;
                var yesRect = new Rectangle(delBtnX, delBtnY, halfW, delBtnH);
                var noRect = new Rectangle(delBtnX + halfW + gap / 2, delBtnY, halfW, delBtnH);

                sb.Draw(Assets.Table, yesRect, Color.DarkRed * 0.9f);
                DrawCenteredText(sb, "Так, видалити", yesRect, Color.White);

                sb.Draw(Assets.Table, noRect, Color.DarkSlateGray * 0.9f);
                DrawCenteredText(sb, "Скасувати", noRect, Color.White);

                if (IsClicked(yesRect)) DeleteSave();
                if (IsClicked(noRect)) _confirmDelete = false;
            }

            sb.End();
        }
    }
}