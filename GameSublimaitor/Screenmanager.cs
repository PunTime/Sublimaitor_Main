using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ElementGame.Screens;
namespace ElementGame.Managers
{
    /* стек екранів — тільки верхній екран активний */
    public class ScreenManager
    {
        private Stack<BaseScreen> _screens = new();
        private Game1 _game;
        private SpriteBatch _spriteBatch;

        public SpriteBatch SpriteBatch => _spriteBatch;
        public Game1 Game => _game;

        /* розмір вікна беремо з viewport щоб коректно реагувати на зміну розширення */
        public int Width => _game.GraphicsDevice.Viewport.Width;
        public int Height => _game.GraphicsDevice.Viewport.Height;

        public ScreenManager(Game1 game, SpriteBatch spriteBatch)
        {
            _game = game;
            _spriteBatch = spriteBatch;
        }

        /* додаємо екран і одразу ініціалізуємо */
        public void Push(BaseScreen screen)
        {
            _screens.Push(screen);
            screen.Initialize();
        }

        /* знімаємо верхній екран і відновлюємо попередній */
        public void Pop()
        {
            if (_screens.Count > 0)
            {
                _screens.Pop();
                if (_screens.Count > 0)
                    _screens.Peek().OnResume();
            }
        }

        /* замінюємо поточний екран новим без накопичення в стеку */
        public void Replace(BaseScreen screen)
        {
            if (_screens.Count > 0) _screens.Pop();
            Push(screen);
        }

        /* очищаємо стек до кореневого екрана */
        public void PopToRoot()
        {
            while (_screens.Count > 1) _screens.Pop();
            if (_screens.Count > 0) _screens.Peek().OnResume();
        }

        /* оновлюємо тільки верхній екран */
        public void Update(GameTime gameTime)
        {
            if (_screens.Count > 0)
                _screens.Peek().Update(gameTime);
        }

        /* малюємо тільки верхній екран */
        public void Draw(GameTime gameTime)
        {
            if (_screens.Count > 0)
                _screens.Peek().Draw(gameTime, _spriteBatch);
        }
    }
}