using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ScatteredGameExample.Components;
using ScatteredGameExample.Systems;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample
{
    public class ScatteredGame : Game
    {
        private static readonly int Width = 1000;
        private static readonly int Height = 625;

        private static readonly float SecondsPerTick = 1.0f / TimeSpan.TicksPerMillisecond / 1000.0f;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private readonly IEntityManager<Entity> entityManager;
        private readonly HashSet<BaseSystem> systems = new HashSet<BaseSystem>();
        private readonly HashSet<DrawingSystem> drawingSystems = new HashSet<DrawingSystem>();
        private readonly EventBus eventBus = new EventBus();

        public ScatteredGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;

            entityManager = EntityManagerFactory.Create(BitmaskSize.Bit32);
        }

        protected override void Initialize()
        {
            base.Initialize();

            var dm = graphics.GraphicsDevice.DisplayMode;
            Window.Position = new Point((dm.Width - Width) / 2, (dm.Height - Height) / 2);
        }

        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(GraphicsDevice);

            Texture2D ballTexture = TextureUtil.CreateCircle(GraphicsDevice, 10, Color.White);
            Transform ballTransform = new Transform
            {
                Position = new Vector2(100, 100)
            };

            Velocity ballVelocity = new Velocity
            {
                Speed = new Vector2(10, 20)
            };

            Entity ballEntity = entityManager.CreateEntity();
            ballEntity.AddComponent(ballTexture);
            ballEntity.AddComponent(ballTransform);
            ballEntity.AddComponent(ballVelocity);

            AddSystem(new RenderingSystem());
            AddSystem(new VelocitySystem());
            AddSystem(new InputSystem());
            AddSystem(new HudSystem(Content.Load<Texture2D>("crosshair")));
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void AddSystem(BaseSystem system)
        {
            system.EventBus = eventBus;
            systems.Add(system);
            DrawingSystem ds = system as DrawingSystem;
            if (ds != null) drawingSystems.Add(ds);
            entityManager.AddSystem(system);
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = gameTime.ElapsedGameTime.Ticks * SecondsPerTick;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);

            foreach (BaseSystem system in systems) system.Update(deltaTime);

            entityManager.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = gameTime.ElapsedGameTime.Ticks * SecondsPerTick;

            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            spriteBatch.Begin(SpriteSortMode.BackToFront);
            foreach (DrawingSystem system in drawingSystems) system.Draw(deltaTime, spriteBatch);
            spriteBatch.End();
        }
    }
}
