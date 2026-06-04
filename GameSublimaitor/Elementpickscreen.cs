using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ElementGame.Managers;
using System;

namespace ElementGame.Screens
{
    public class ElementPickScreen : BaseScreen
    {
        private PlayerManager _playerManager;
        private int _playerIndex;
        private Action<string> _onPick; /* колбек при виборі елемента */

        /* стан скролу */
        private int _scrollOffset = 0;
        private bool _draggingScroll = false;

        /* параметри сітки, зберігаємо між Update і Draw */
        private int _cols = 4;
        private int _rowH = 1;
        private int _visibleRows = 1;
        private int _totalRows = 1;
        private int _gridStartY = 0;
        private int _gridH = 0;
        private int _scrollBarX = 0;
        private int _scrollBarY = 0;
        private int _scrollBarW = 0;
        private int _scrollBarH = 0;
        private int _thumbH = 0;
        private int _thumbY = 0;

        /* зберігаємо кнопку назад щоб перемалювати після scissor */
        private Button _backBtn;

        public ElementPickScreen(
            ScreenManager sm, PlayerManager pm,
            int playerIdx, Action<string> onPick) : base(sm)
        {
            _playerManager = pm;
            _playerIndex = playerIdx;
            _onPick = onPick;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            /* скрол колесом миші */
            int wheelDelta = CurrentMouse.ScrollWheelValue - PreviousMouse.ScrollWheelValue;
            if (wheelDelta != 0)
            {
                _scrollOffset -= Math.Sign(wheelDelta);
                ClampScroll();
            }

            /* перетягування повзунка правою кнопкою */
            if (_scrollBarH > 0 && _thumbH > 0)
            {
                var bar = new Rectangle(_scrollBarX, _scrollBarY, _scrollBarW, _scrollBarH);
                if (CurrentMouse.RightButton == ButtonState.Pressed)
                {
                    if (bar.Contains(new Point(CurrentMouse.X, CurrentMouse.Y)) || _draggingScroll)
                    {
                        _draggingScroll = true;
                        float t = (float)(CurrentMouse.Y - _scrollBarY - _thumbH / 2)
                                  / (_scrollBarH - _thumbH);
                        t = Math.Clamp(t, 0f, 1f);
                        _scrollOffset = (int)Math.Round(t * (_totalRows - _visibleRows));
                        ClampScroll();
                    }
                }
                else _draggingScroll = false;
            }
        }

        private void ClampScroll()
        {
            int maxScroll = Math.Max(0, _totalRows - _visibleRows);
            _scrollOffset = Math.Clamp(_scrollOffset, 0, maxScroll);
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int W = ScreenManager.Width;
            int H = ScreenManager.Height;

            sb.Begin();
            DrawBackground(sb, Assets.Table);

            var unlocked = _playerManager.GetPlayer(_playerIndex).UnlockedElements;

            /* заголовок зверху */
            int titleH = (int)(H * 0.08f);
            int titleY = (int)(H * 0.02f);
            DrawCenteredText(sb, "Оберіть елемент",
                new Rectangle(0, titleY, W, titleH), Color.Yellow);

            /* кнопка назад знизу по центру */
            int backW = (int)(W * 0.18f);
            int backH = (int)(H * 0.08f);
            int backY = H - backH - (int)(H * 0.02f);
            _backBtn = new Button(
                new Rectangle((W - backW) / 2, backY, backW, backH),
                "Назад", Color.DarkGray * 0.85f, Color.White);

            if (DrawBtn(sb, _backBtn)) ScreenManager.Pop();

            /* рахуємо розміри сітки */
            int scrollBarW = (int)(W * 0.025f);
            int gridMarginX = (int)(W * 0.04f);
            int gridMarginY = titleY + titleH + (int)(H * 0.01f);
            int gridAreaH = backY - gridMarginY - (int)(H * 0.01f);
            int gridAreaW = W - gridMarginX * 2 - scrollBarW - (int)(W * 0.01f);

            int padX = (int)(W * 0.015f);
            int padY = (int)(H * 0.015f);
            int labelH = (int)(H * 0.038f);
            int cellSize = (gridAreaW - padX * (_cols - 1)) / _cols;
            _rowH = cellSize + padY + labelH;

            _totalRows = (unlocked.Count + _cols - 1) / _cols;
            _visibleRows = Math.Max(1, gridAreaH / _rowH);
            _gridStartY = gridMarginY;
            _gridH = gridAreaH;

            ClampScroll();

            /* смуга прокрутки справа */
            _scrollBarX = gridMarginX + gridAreaW + (int)(W * 0.01f);
            _scrollBarY = gridMarginY;
            _scrollBarW = scrollBarW;
            _scrollBarH = gridAreaH;

            sb.Draw(Assets.Table,
                new Rectangle(_scrollBarX, _scrollBarY, _scrollBarW, _scrollBarH),
                Color.Black * 0.4f);

            if (_totalRows > _visibleRows)
            {
                /* повзунок пропорційний кількості видимих рядків */
                _thumbH = Math.Max(30, (int)(_scrollBarH * (float)_visibleRows / _totalRows));
                float scrollT = (float)_scrollOffset / (_totalRows - _visibleRows);
                _thumbY = _scrollBarY + (int)(scrollT * (_scrollBarH - _thumbH));
                sb.Draw(Assets.Table,
                    new Rectangle(_scrollBarX + 2, _thumbY, _scrollBarW - 4, _thumbH),
                    Color.LightGray * 0.9f);
            }
            else
            {
                /* все влазить — повзунок на всю висоту */
                _thumbH = _scrollBarH;
                _thumbY = _scrollBarY;
                sb.Draw(Assets.Table,
                    new Rectangle(_scrollBarX + 2, _thumbY, _scrollBarW - 4, _thumbH),
                    Color.Gray * 0.5f);
            }

            /* обрізаємо сітку щоб елементи не виходили за область */
            sb.End();
            var scissor = sb.GraphicsDevice.ScissorRectangle;
            sb.GraphicsDevice.ScissorRectangle = new Rectangle(
                gridMarginX, gridMarginY, gridAreaW, gridAreaH);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                null, null, new RasterizerState { ScissorTestEnable = true });

            for (int i = 0; i < unlocked.Count; i++)
            {
                int col = i % _cols;
                int row = i / _cols;
                int x = gridMarginX + col * (cellSize + padX);
                int y = _gridStartY + (row - _scrollOffset) * _rowH;

                if (y + _rowH < _gridStartY || y > _gridStartY + gridAreaH) continue;

                var rect = new Rectangle(x, y, cellSize, cellSize);
                sb.Draw(Assets.Table, rect, Color.Black * 0.3f);

                var tex = Assets.GetElement(unlocked[i]);
                if (tex != null)
                {
                    int ip = (int)(cellSize * 0.06f);
                    sb.Draw(tex, new Rectangle(x + ip, y + ip,
                        cellSize - ip * 2, cellSize - ip * 2), Color.White);
                }

                DrawCenteredText(sb, unlocked[i],
                    new Rectangle(x, y + cellSize + 2, cellSize, labelH),
                    Color.LightGray);

                /* клік вибирає елемент і закриває екран */
                if (IsClicked(rect))
                {
                    _onPick(unlocked[i]);
                    ScreenManager.Pop();
                    sb.End();
                    sb.GraphicsDevice.ScissorRectangle = scissor;
                    return;
                }
            }

            sb.End();
            sb.GraphicsDevice.ScissorRectangle = scissor;

            /* перемальовуємо кнопку назад поверх сітки */
            sb.Begin();
            DrawBtn(sb, _backBtn);
            sb.End();
        }
    }
}
