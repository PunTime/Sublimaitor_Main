using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ElementGame.Managers;
using System.Collections.Generic;

namespace ElementGame.Screens
{
    public class BookScreen : BaseScreen
    {
        private PlayerManager _playerManager;
        private int _playerIndex;

        /* всі елементи гри в порядку від базових до рідкісних */
        public static readonly List<string> AllElements = new List<string>
        {
            "Fire", "Water", "Wind", "Earth",
            "Steam", "Mud", "Sand", "Cloud", "Ash",
            "Brick", "Thunder", "Stone", "Metal",
            "Magnet", "Artefact", "LifeStone"
        };

        /* базові елементи не потребують крафту */
        public static readonly HashSet<string> BaseElements = new HashSet<string>
        {
            "Fire", "Water", "Wind", "Earth"
        };

        /* стан скролу сітки */
        private int _scrollOffset = 0;
        private int _cols = 4;
        private int _rowH = 1;
        private int _visibleRows = 1;
        private int _totalRows = 1;
        private int _gridStartY = 0;
        private int _gridH = 0;
        private int _scrollBarX, _scrollBarY, _scrollBarW, _scrollBarH;
        private int _thumbH, _thumbY;
        private bool _draggingScroll = false;

        public BookScreen(ScreenManager sm, PlayerManager pm, int idx) : base(sm)
        {
            _playerManager = pm;
            _playerIndex = idx;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            /* скрол колесом миші */
            int wheelDelta = CurrentMouse.ScrollWheelValue - PreviousMouse.ScrollWheelValue;
            if (wheelDelta != 0)
            {
                _scrollOffset -= System.Math.Sign(wheelDelta);
                ClampScroll();
            }

            /* скрол через перетягування повзунка правою кнопкою */
            if (_scrollBarH > 0 && _thumbH > 0)
            {
                var bar = new Rectangle(_scrollBarX, _scrollBarY, _scrollBarW, _scrollBarH);
                if (CurrentMouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    if (bar.Contains(new Point(CurrentMouse.X, CurrentMouse.Y)) || _draggingScroll)
                    {
                        _draggingScroll = true;
                        float t = (float)(CurrentMouse.Y - _scrollBarY - _thumbH / 2)
                                  / (_scrollBarH - _thumbH);
                        t = System.Math.Clamp(t, 0f, 1f);
                        _scrollOffset = (int)System.Math.Round(t * (_totalRows - _visibleRows));
                        ClampScroll();
                    }
                }
                else _draggingScroll = false;
            }
        }

        private void ClampScroll()
        {
            int maxScroll = System.Math.Max(0, _totalRows - _visibleRows);
            _scrollOffset = System.Math.Clamp(_scrollOffset, 0, maxScroll);
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int W = ScreenManager.Width;
            int H = ScreenManager.Height;

            sb.Begin();
            sb.Draw(Assets.Table, new Rectangle(0, 0, W, H), Color.White);

            var unlocked = _playerManager.GetPlayer(_playerIndex).UnlockedElements;

            /* заголовок зверху по центру */
            int titleH = (int)(H * 0.08f);
            int titleY = (int)(H * 0.02f);
            DrawCenteredText(sb, "Книга елементів",
                new Rectangle(0, titleY, W, titleH), Color.SaddleBrown);

            /* кнопка назад знизу по центру */
            int backW = (int)(W * 0.18f);
            int backH = (int)(H * 0.08f);
            int backY = H - backH - (int)(H * 0.02f);
            var backBtn = new Button(
                new Rectangle((W - backW) / 2, backY, backW, backH),
                "Назад", Color.SaddleBrown * 0.85f, Color.White);

            if (DrawBtn(sb, backBtn)) ScreenManager.Pop();

            /* розраховуємо розміри сітки і клітинок */
            int scrollBarW = (int)(W * 0.025f);
            int gridMarginX = (int)(W * 0.04f);
            int gridMarginY = titleY + titleH + (int)(H * 0.01f);
            int gridAreaH = backY - gridMarginY - (int)(H * 0.01f);
            int gridAreaW = W - gridMarginX * 2 - scrollBarW - (int)(W * 0.01f);

            int padX = (int)(W * 0.03f);
            int padY = (int)(H * 0.03f);
            int labelH = (int)(H * 0.038f);
            int cellSize = (gridAreaW - padX * (_cols - 1)) / _cols;
            _rowH = cellSize + padY + labelH;

            _totalRows = (AllElements.Count + _cols - 1) / _cols;
            _visibleRows = System.Math.Max(1, gridAreaH / _rowH);
            _gridStartY = gridMarginY;
            _gridH = gridAreaH;

            ClampScroll();

            /* смуга прокрутки справа від сітки */
            _scrollBarX = gridMarginX + gridAreaW + (int)(W * 0.01f);
            _scrollBarY = gridMarginY;
            _scrollBarW = scrollBarW;
            _scrollBarH = gridAreaH;

            sb.Draw(Assets.Table,
                new Rectangle(_scrollBarX, _scrollBarY, _scrollBarW, _scrollBarH),
                Color.LightGray * 0.6f);

            /* малюємо повзунок тільки якщо є що скролити */
            if (_totalRows > _visibleRows)
            {
                _thumbH = System.Math.Max(30, (int)(_scrollBarH * (float)_visibleRows / _totalRows));
                float scrollT = (float)_scrollOffset / (_totalRows - _visibleRows);
                _thumbY = _scrollBarY + (int)(scrollT * (_scrollBarH - _thumbH));
                sb.Draw(Assets.Table,
                    new Rectangle(_scrollBarX + 2, _thumbY, _scrollBarW - 4, _thumbH),
                    Color.SaddleBrown * 0.8f);
            }

            sb.End();

            /* обрізаємо сітку щоб не виходила за область */
            var scissor = sb.GraphicsDevice.ScissorRectangle;
            sb.GraphicsDevice.ScissorRectangle = new Rectangle(gridMarginX, gridMarginY, gridAreaW, gridAreaH);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                null, null, new RasterizerState { ScissorTestEnable = true });

            for (int i = 0; i < AllElements.Count; i++)
            {
                int col = i % _cols;
                int row = i / _cols;
                int x = gridMarginX + col * (cellSize + padX);
                int y = _gridStartY + (row - _scrollOffset) * _rowH;

                if (y + _rowH < _gridStartY || y > _gridStartY + gridAreaH) continue;

                string elem = AllElements[i];
                bool isUnlocked = unlocked.Contains(elem);
                bool isBase = BaseElements.Contains(elem);
                var rect = new Rectangle(x, y, cellSize, cellSize);

                /* колір фону залежить від типу елемента */
                Color bgColor = isBase ? Color.Peru * 0.25f :
                                isUnlocked ? Color.SteelBlue * 0.20f :
                                             Color.Gray * 0.15f;
                sb.Draw(Assets.Table, rect, bgColor);

                /* рамка навколо клітинки */
                int border = 2;
                Color borderColor = isBase ? Color.SaddleBrown * 0.7f :
                                    isUnlocked ? Color.SteelBlue * 0.5f :
                                                 Color.Gray * 0.4f;
                sb.Draw(Assets.Table, new Rectangle(x, y, cellSize, border), borderColor);
                sb.Draw(Assets.Table, new Rectangle(x, y + cellSize - border, cellSize, border), borderColor);
                sb.Draw(Assets.Table, new Rectangle(x, y, border, cellSize), borderColor);
                sb.Draw(Assets.Table, new Rectangle(x + cellSize - border, y, border, cellSize), borderColor);

                if (isUnlocked)
                {
                    var tex = Assets.GetElement(elem);
                    if (tex != null)
                    {
                        int ip = (int)(cellSize * 0.08f);
                        sb.Draw(tex, new Rectangle(x + ip, y + ip, cellSize - ip * 2, cellSize - ip * 2), Color.White);
                    }
                    DrawCenteredText(sb, elem,
                        new Rectangle(x, y + cellSize + 2, cellSize, labelH),
                        isBase ? Color.SaddleBrown : Color.DarkSlateGray);
                }
                else
                {
                    DrawCenteredText(sb, "???", rect, Color.Gray * 0.6f);
                    DrawCenteredText(sb, "???",
                        new Rectangle(x, y + cellSize + 2, cellSize, labelH),
                        Color.Gray * 0.5f);
                }

                /* клік по клітинці відкриває детальний екран елемента */
                if (IsClicked(rect))
                    ScreenManager.Push(new ElementDetailScreen(
                        ScreenManager, _playerManager, _playerIndex, elem, isUnlocked));
            }

            sb.End();
            sb.GraphicsDevice.ScissorRectangle = scissor;

            /* перемальовуємо кнопку назад поверх сітки */
            sb.Begin();
            DrawBtn(sb, backBtn);
            sb.End();
        }
    }
}