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

        private readonly IGroupedEntityWorld entityWorld;
        private readonly HashSet<BaseSystem> systems = new HashSet<BaseSystem>();
        private readonly HashSet<DrawingSystem> drawingSystems = new HashSet<DrawingSystem>();
        private readonly EventBus eventBus = new EventBus();

        private EntityFactory entityFactory;
        private RenderUtil renderUtil;

        public ScatteredGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;

            entityWorld = EntityManagerFactory.CreateGroupedEntityWorld(256, BitmaskSize.Bit32);
          
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            var dm = graphics.GraphicsDevice.DisplayMode;
            Window.Position = new Point((dm.Width - Width) / 2, (dm.Height - Height) / 2);

            renderUtil = new RenderUtil(GraphicsDevice);

            RenderingSystem renderingSystem = new RenderingSystem(Content);
            entityFactory = new EntityFactory(renderingSystem, entityWorld);

            AddSystem(renderingSystem);
            AddSystem(new VelocitySystem());
            AddSystem(new InputSystem());
            AddSystem(new CollisionSystem());
            AddSystem(new CollisionResolverSystem());
            AddSystem(new HudSystem());
            AddSystem(new PlayerControllerSystem());
            AddSystem(new BoundsSystem(Width / 2 - 250, Height / 2 - 250, 500, 500, renderUtil));

            Handle b1 = entityFactory.CreateSquare();
            entityWorld.GetComponent<Transform>(b1).Position = new Vector2(100, 100);
            entityWorld.GetComponent<Transform>(b1).Size = new Vector2(10, 10);
            entityWorld.AddComponent(b1, new Velocity { Speed = new Vector2(10, 10) });
            entityWorld.AddComponent(b1, new Collider());

            Handle b2 = entityFactory.CreateSquare();
            entityWorld.GetComponent<Transform>(b2).Position = new Vector2(150, 150);
            entityWorld.GetComponent<Transform>(b2).Size = new Vector2(10, 10);
            entityWorld.AddComponent(b2, new Collider());
        }

        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(GraphicsDevice);
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
            system.EntityWorld = entityWorld;
            system.EntityFactory = entityFactory;
            system.EventBus = eventBus;
            system.GroupId = entityWorld.GetGroupId(system.RequiredComponents);
            system.Added();
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = gameTime.ElapsedGameTime.Ticks * SecondsPerTick;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            base.Update(gameTime);

            foreach (BaseSystem system in systems)
            {
                IArray<Handle> entities = entityWorld.GetEntitiesForGroup(system.GroupId);
                system.Update(entities, deltaTime);
            }
            eventBus.Update();
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
