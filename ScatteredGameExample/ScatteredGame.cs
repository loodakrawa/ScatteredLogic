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
        private static readonly int Width = 768;
        private static readonly int Height = 768;

        private static readonly float SecondsPerTick = 1.0f / TimeSpan.TicksPerMillisecond / 1000.0f;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private readonly IEntitySystemManager entityManager;
        private readonly HashSet<BaseSystem> systems = new HashSet<BaseSystem>();
        private readonly HashSet<DrawingSystem> drawingSystems = new HashSet<DrawingSystem>();

        private EntityFactory entityFactory;
        private RenderUtil renderUtil;

        public ScatteredGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;

            entityManager = EntityManagerFactory.CreateEntitySystemManager(BitmaskSize.Bit32, 256);
            entityFactory = new EntityFactory(Content, entityManager);

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            var dm = graphics.GraphicsDevice.DisplayMode;
            Window.Position = new Point((dm.Width - Width) / 2, (dm.Height - Height) / 2);

            renderUtil = new RenderUtil(GraphicsDevice);

            AddSystem(new RenderingSystem());
            AddSystem(new VelocitySystem());
            AddSystem(new InputSystem());
            AddSystem(new CollisionSystem());
            AddSystem(new CollisionResolverSystem());
            AddSystem(new HudSystem());
            AddSystem(new PlayerControllerSystem());
            AddSystem(new BoundsSystem(Width / 2 - 250, Height / 2 - 250, 500, 500, renderUtil));
        }

        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(GraphicsDevice);

            Entity b1 = entityFactory.CreateSquare();
            b1.GetComponent<Transform>().Position = new Vector2(100, 100);
            b1.GetComponent<Transform>().Size = new Vector2(10, 10);
            b1.AddComponent(new Velocity { Speed = new Vector2(10, 10) });
            b1.AddComponent(new Collider());

            Entity b2 = entityFactory.CreateSquare();
            b2.GetComponent<Transform>().Position = new Vector2(150, 150);
            b2.GetComponent<Transform>().Size = new Vector2(10, 10);
            b2.AddComponent(new Collider());
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void AddSystem(BaseSystem system)
        {
            systems.Add(system);
            DrawingSystem ds = system as DrawingSystem;
            if (ds != null) drawingSystems.Add(ds);
            system.EntityFactory = entityFactory;
            entityManager.AddSystem(system);
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = gameTime.ElapsedGameTime.Ticks * SecondsPerTick;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);

            entityManager.Update(deltaTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = gameTime.ElapsedGameTime.Ticks * SecondsPerTick;

            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            spriteBatch.Begin(SpriteSortMode.BackToFront);
            renderUtil.Draw(spriteBatch);
            foreach (DrawingSystem system in drawingSystems) system.Draw(deltaTime, spriteBatch);
            spriteBatch.End();
        }
    }
}
