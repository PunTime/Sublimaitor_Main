using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using ElementGame.Screens;
using ElementGame.Managers;
namespace ElementGame
{
    /* головний клас гри — точка входу, керує менеджерами */
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        /* глобальні менеджери, доступ через властивості */
        public ScreenManager ScreenManager { get; private set; }
        public AssetManager AssetManager { get; private set; }
        public SettingsManager SettingsManager { get; private set; }

        /* синглтон щоб дістатись гри з будь-якого місця */
        public static Game1 Instance { get; private set; }

        public Game1()
        {
            Instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            /* завантажуємо налаштування до всього іншого */
            SettingsManager = new SettingsManager();
            SettingsManager.Load();

            /* застосовуємо збережене розширення екрана */
            _graphics.PreferredBackBufferWidth = SettingsManager.ResolutionWidth;
            _graphics.PreferredBackBufferHeight = SettingsManager.ResolutionHeight;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            /* завантажуємо всі текстури і звуки */
            AssetManager = new AssetManager(Content, GraphicsDevice);
            AssetManager.LoadAll();

            /* запускаємо з головного меню */
            ScreenManager = new ScreenManager(this, _spriteBatch);
            ScreenManager.Push(new MainMenuScreen(ScreenManager));

            /* вмикаємо фонову музику з повтором */
            ApplyVolume();
            if (AssetManager.BackgroundMusic != null)
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(AssetManager.BackgroundMusic);
            }
        }

        /* міняємо гучність без перезапуску музики */
        public void ApplyVolume()
        {
            MediaPlayer.Volume = SettingsManager.Volume;
        }

        /* застосовуємо нове розширення з налаштувань */
        public void ApplyResolution()
        {
            _graphics.PreferredBackBufferWidth = SettingsManager.ResolutionWidth;
            _graphics.PreferredBackBufferHeight = SettingsManager.ResolutionHeight;
            _graphics.ApplyChanges();
        }

        protected override void Update(GameTime gameTime)
        {
            ScreenManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            /* чорний фон між кадрами */
            GraphicsDevice.Clear(Color.Black);
            ScreenManager.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}