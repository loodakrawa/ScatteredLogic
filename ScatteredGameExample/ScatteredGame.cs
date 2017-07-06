﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ScatteredGameExample.Components;
using ScatteredGameExample.Parallel;
using ScatteredGameExample.Systems;
using ScatteredLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScatteredGameExample
{
    public class ScatteredGame : Game
    {
        private static readonly int Width = 768;
        private static readonly int Height = 768;

        private static readonly float SecondsPerTick = 1.0f / TimeSpan.TicksPerMillisecond / 1000.0f;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private readonly IEntityWorld entityWorld;
        private readonly HashSet<BaseSystem> systems = new HashSet<BaseSystem>();
        private readonly HashSet<DrawingSystem> drawingSystems = new HashSet<DrawingSystem>();
        private readonly EventBus eventBus = new EventBus();

        private EntityFactory entityFactory;
        private RenderUtil renderUtil;

        private readonly List<SystemUpdateTask> systemUpdateTasks = new List<SystemUpdateTask>();
        private readonly List<AspectUpdateTask> aspectUpdateTasks = new List<AspectUpdateTask>();
        private readonly List<ITaskCallback> taskCallbacks = new List<ITaskCallback>();

        public ScatteredGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;

            entityWorld = EntityManagerFactory.CreateEntityWorld(256, 32, BitmaskSize.Bit32);

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Tasks.TaskQueue = new ThreadPoolTaskQueue(8);

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

            //Handle b1 = entityFactory.CreateSquare();
            //entityWorld.GetComponent<Transform>(b1).Position = new Vector2(100, 100);
            //entityWorld.GetComponent<Transform>(b1).Size = new Vector2(10, 10);
            //entityWorld.AddComponent(b1, new Velocity { Speed = new Vector2(10, 10) });
            //entityWorld.AddComponent(b1, new Collider());

            //Handle b2 = entityFactory.CreateSquare();
            //entityWorld.GetComponent<Transform>(b2).Position = new Vector2(150, 150);
            //entityWorld.GetComponent<Transform>(b2).Size = new Vector2(10, 10);
            //entityWorld.AddComponent(b2, new Collider());
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
            IEnumerable<Type> requiredTypes = system.RequiredComponents;
            if (requiredTypes != null && requiredTypes.Count() > 0)
            {
                system.Aspect = entityWorld.CreateAspect(system.RequiredComponents, system.GetType().Name);
                system.Entities = entityWorld.GetAspectEntities(system.Aspect);
                aspectUpdateTasks.Add(new AspectUpdateTask(system.Aspect, entityWorld));
            }
            system.Added();

            systemUpdateTasks.Add(new SystemUpdateTask(system));
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = gameTime.ElapsedGameTime.Ticks * SecondsPerTick;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            base.Update(gameTime);

            foreach(SystemUpdateTask sut in systemUpdateTasks)
            {
                sut.DeltaTime = deltaTime;
                ITaskCallback callback = Tasks.Enqueue(sut);
                taskCallbacks.Add(callback);
            }
            foreach (ITaskCallback callback in taskCallbacks) callback.Wait();
            taskCallbacks.Clear();

            eventBus.Update();
            entityWorld.Step();

            foreach (AspectUpdateTask aut in aspectUpdateTasks)
            {
                ITaskCallback callback = Tasks.Enqueue(aut);
                taskCallbacks.Add(callback);
            }
            foreach (ITaskCallback callback in taskCallbacks) callback.Wait();
            taskCallbacks.Clear();
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

        private class SystemUpdateTask : ITask
        {
            public float DeltaTime { get; set; }
            private readonly BaseSystem system;
            public SystemUpdateTask(BaseSystem system) => this.system = system;
            public void Run() => system.Update(DeltaTime);
        }

        private class AspectUpdateTask : ITask
        {
            private readonly Handle aspect;
            private readonly IEntityWorld entityWorld;

            public AspectUpdateTask(Handle aspect, IEntityWorld entityWorld)
            {
                this.aspect = aspect;
                this.entityWorld = entityWorld;
            }

            public void Run() => entityWorld.UpdateAspect(aspect);
        }

    }
}
