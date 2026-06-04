using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ElementGame.Managers;

namespace ElementGame.Screens
{
    public class BoilerScreen : BaseScreen
    {
        private PlayerManager _playerManager;
        private int _playerIndex;
        private string[] _slots = new string[3]; /* 3 слоти: 2 входи + 1 результат */
        private string _incompatibleMsg = null;
        private double _incompatibleTimer = 0;
        private ElementRecipeManager _recipes = new ElementRecipeManager();

        public BoilerScreen(ScreenManager sm, PlayerManager pm, int idx) : base(sm)
        {
            _playerManager = pm;
            _playerIndex = idx;
        }

        /* рахує координати вікна і центри кіл під елементи */
        private void CalcLayout(int W, int H,
            out int winX, out int winY, out int winSize,
            out Point[] centers, out int circleR)
        {
            winSize = (int)(System.Math.Min(W, H) * 0.92f);
            winX = (W - winSize) / 2;
            winY = (H - winSize) / 2;
            circleR = (int)(winSize * 0.18f);

            /* позиції трьох кіл: лівий вхід, правий вхід, результат знизу */
            centers = new Point[]
            {
                new Point(winX + winSize * 27 / 100, winY + winSize * 26 / 100),
                new Point(winX + winSize * 73 / 100, winY + winSize * 27 / 100),
                new Point(winX + winSize * 50 / 100, winY + winSize * 70 / 100),
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            /* рахуємо час показу повідомлення про несумісність */
            if (_incompatibleMsg != null)
            {
                _incompatibleTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (_incompatibleTimer > 2.5) { _incompatibleMsg = null; _incompatibleTimer = 0; }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int W = ScreenManager.Width;
            int H = ScreenManager.Height;

            CalcLayout(W, H, out int winX, out int winY, out int winSize,
                       out Point[] centers, out int circleR);

            sb.Begin();

            /* малюємо фон вікна */
            sb.Draw(Assets.FuseBg ?? Assets.Table,
                new Rectangle(winX, winY, winSize, winSize), Color.White);

            /* малюємо всі три слоти */
            for (int i = 0; i < 3; i++)
            {
                var cr = new Rectangle(
                    centers[i].X - circleR, centers[i].Y - circleR,
                    circleR * 2, circleR * 2);

                if (_slots[i] != null)
                {
                    var tex = Assets.GetElement(_slots[i]);
                    if (tex != null)
                    {
                        /* малюємо текстуру елемента з обрізкою по колу через scissor */
                        sb.End();
                        var prevScissor = sb.GraphicsDevice.ScissorRectangle;
                        var vp = sb.GraphicsDevice.Viewport;
                        int sx = System.Math.Max(cr.X, 0);
                        int sy = System.Math.Max(cr.Y, 0);
                        int sr = System.Math.Min(cr.Right, vp.Width);
                        int sb2 = System.Math.Min(cr.Bottom, vp.Height);
                        sb.GraphicsDevice.ScissorRectangle = new Rectangle(
                            sx, sy,
                            System.Math.Max(0, sr - sx),
                            System.Math.Max(0, sb2 - sy));
                        sb.Begin(Microsoft.Xna.Framework.Graphics.SpriteSortMode.Deferred,
                            Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend,
                            null, null,
                            new Microsoft.Xna.Framework.Graphics.RasterizerState { ScissorTestEnable = true });

                        int pad = (int)(circleR * 0.08f);
                        var inner = new Rectangle(cr.X + pad, cr.Y + pad,
                                                  cr.Width - pad * 2, cr.Height - pad * 2);
                        sb.Draw(tex, inner, Color.White);

                        sb.End();
                        sb.GraphicsDevice.ScissorRectangle = prevScissor;
                        sb.Begin();
                    }
                    /* підпис назви елемента під колом */
                    int labelH = (int)(winSize * 0.05f);
                    DrawCenteredText(sb, _slots[i],
                        new Rectangle(centers[i].X - circleR,
                                      centers[i].Y + circleR - 30,
                                      circleR * 2, labelH),
                        Color.White);
                }
                else if (i < 2)
                {
                    /* підказка що слот порожній */
                    DrawCenteredText(sb, "ПКМ", cr, Color.White * 0.45f);
                }

                /* клік по вхідному слоту відкриває вибір елемента */
                if (i < 2 && IsClicked(cr))
                {
                    int captured = i;
                    ScreenManager.Push(new ElementPickScreen(
                        ScreenManager, _playerManager, _playerIndex,
                        elem => { _slots[captured] = elem; }));
                }
            }

            /* кнопка FUSE внизу по центру */
            bool canFuse = _slots[0] != null && _slots[1] != null;
            int fuseBtnH = (int)(winSize * 0.09f);
            int fuseBtnW = (int)(winSize * 0.32f);
            var fuseBtn = new Button(
                new Rectangle(winX + (winSize - fuseBtnW) / 2,
                              winY + winSize - fuseBtnH - (int)(winSize * 0.03f),
                              fuseBtnW, fuseBtnH),
                canFuse ? "FUSE!" : "FUSE",
                canFuse ? Color.SaddleBrown * 0.92f : Color.Gray * 0.45f,
                canFuse ? Color.White : Color.LightGray * 0.7f);

            if (DrawBtn(sb, fuseBtn))
            {
                if (canFuse)
                {
                    string result = _recipes.Combine(_slots[0], _slots[1]);
                    if (result != null)
                    {
                        _slots[2] = result;
                        _playerManager.UnlockElement(_playerIndex, result);
                        _incompatibleMsg = null;

                        /* якщо створили LifeStone — гравець переміг */
                        if (result == "LifeStone")
                        {
                            _playerManager.SetWinner(_playerIndex);
                            ScreenManager.PopToRoot();
                            ScreenManager.Replace(new VictoryScreen(ScreenManager));
                        }
                    }
                    else
                    {
                        _incompatibleMsg = "Оберіть сумісні елементи";
                        _incompatibleTimer = 0;
                        _slots[2] = null;
                    }
                }
                else
                {
                    _incompatibleMsg = "Оберіть сумісні елементи";
                    _incompatibleTimer = 0;
                }
            }

            /* показуємо повідомлення про помилку над кнопкою */
            if (_incompatibleMsg != null)
                DrawCenteredText(sb, _incompatibleMsg,
                    new Rectangle(winX,
                                  fuseBtn.Rect.Y - (int)(winSize * 0.08f),
                                  winSize, (int)(winSize * 0.07f)),
                    Color.OrangeRed);

            /* кнопка назад у лівому верхньому куті */
            int backW = (int)(winSize * 0.18f);
            int backH = (int)(winSize * 0.08f);
            var backBtn = new Button(
                new Rectangle(winX + 8, winY + 8, backW, backH),
                "Назад", Color.Black * 0.55f, Color.White);

            if (DrawBtn(sb, backBtn))
                ScreenManager.Pop();

            sb.End();
        }
    }
}
