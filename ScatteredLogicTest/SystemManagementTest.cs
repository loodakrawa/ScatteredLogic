// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredLogicTest
{
    public class EmptyTestSystem : ISystem
    {
        public SetEnumerable<Entity> Entities { get; set; }
        public IEntityManager EntityManager { get; set; }
        public IEnumerable<Type> RequiredComponents => Types.None;
        public EventBus EventBus { get; set; }

        public void Added() { }
        public void Removed() { }
        public void EntityAdded(Entity entity) { }
        public void EntityRemoved(Entity entity) { }
        public void Update(float deltaTime) { }
    }

    public class TestSystem<T> : ISystem
    {
        public SetEnumerable<Entity> Entities { get; set; }
        public IEntityManager EntityManager { get; set; }
        public IEnumerable<Type> RequiredComponents => Types.From<T>();
        public EventBus EventBus { get; set; }

        public event Action OnAdded = () => { };
        public event Action OnRemoved = () => { };

        public event Action<Entity> OnEntityAdded = x => { };
        public event Action<Entity> OnEntityRemoved = x => { };

        public void Added() => OnAdded();
        public void Removed() => OnRemoved();
        public void EntityAdded(Entity entity) => OnEntityAdded(entity);
        public void EntityRemoved(Entity entity) => OnEntityRemoved(entity);
        public void Update(float deltaTime) { }
    }

    [TestClass]
    public class SystemManagementTest
    {
        private IEntityManager em;
        private TestSystem<string> system;

        [TestInitialize]
        public void SetUp()
        {
            em = EntityManagerFactory.Create(BitmaskSize.Bit64);
            system = new TestSystem<string>();
        }

        [TestMethod]
        public void AddSystem_AddedCalled()
        {
            bool added = false;
            system.OnAdded += () => added = true;

            em.AddSystem(system);
            em.Update(0);

            Assert.IsTrue(added);
        }

        [TestMethod]
        public void AddSystemTwice_AddedCalledOnce()
        {
            int count = 0;
            system.OnAdded += () => ++count;

            em.AddSystem(system);
            em.AddSystem(system);

            em.Update(0);

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void RemoveSystemWithoutAdding_RemovedNotCalled()
        {
            bool removed = false;
            system.OnRemoved += () => removed = true;

            em.RemoveSystem(system);

            Assert.IsFalse(removed);
        }

        [TestMethod]
        public void AddSystemRemoveSystem_RemoveCalled()
        {
            bool removed = false;
            system.OnRemoved += () => removed = true;

            em.AddSystem(system);
            em.RemoveSystem(system);

            Assert.IsFalse(removed);

            em.Update(0);

            Assert.IsTrue(removed);
        }

        [TestMethod]
        public void AddSystemRemoveSystemTwice_RemoveCalledOnce()
        {
            int count = 0;
            system.OnRemoved += () => ++count;

            em.AddSystem(system);
            em.RemoveSystem(system);
            em.RemoveSystem(system);

            em.Update(0);

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void AddEntity_EntityAddedCalled()
        {
            Entity? addedEntity = null;
            system.OnEntityAdded += x => addedEntity = x;

            em.AddSystem(system);

            Entity entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);

            Assert.IsFalse(addedEntity.HasValue);

            em.Update(0);

            Assert.IsTrue(addedEntity.HasValue);
            Assert.AreEqual(entity, addedEntity.Value);
        }

        [TestMethod]
        public void AddEntityRemoveEntity_EntityRemovedCalled()
        {
            Entity? removedEntity = null;
            system.OnEntityRemoved += x => removedEntity = x;

            em.AddSystem(system);

            Entity entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);

            Assert.IsFalse(removedEntity.HasValue);

            em.Update(0);
            em.RemoveComponent<string>(entity);
            em.Update(0);

            Assert.IsTrue(removedEntity.HasValue);
            Assert.AreEqual(entity, removedEntity.Value);
        }

        [TestMethod]
        public void AddEntityWithDifferentComponent_EntityAddedNotCalled()
        {
            Entity? addedEntity = null;
            system.OnEntityAdded += x => addedEntity = x;

            em.AddSystem(system);

            Entity entity = em.CreateEntity();
            em.AddComponent(entity, int.MaxValue);

            em.Update(0);

            Assert.IsFalse(addedEntity.HasValue);
        }

        [TestMethod]
        public void AddTwoEntitiesWithDifferentComponents_OnlyOneEntityAdded()
        {
            em.AddSystem(system);

            Entity entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);

            Entity entity2 = em.CreateEntity();
            em.AddComponent(entity2, int.MaxValue);

            em.Update(0);

            Assert.AreEqual(1, system.Entities.Count);
        }

        [TestMethod]
        public void EntityAdded_DestroyEntity()
        {
            em.AddSystem(system);

            system.OnEntityAdded += x => em.DestroyEntity(x);

            Entity entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);

            em.Update(0);

            Assert.IsFalse(em.ContainsEntity(entity));
        }

        [TestMethod]
        public void EntityAdded_ChainDestroyEntity()
        {
            em.AddSystem(system);
            system.OnEntityRemoved += x => em.DestroyEntity(x);

            bool removed2Called = false;
            TestSystem<int> system2 = new TestSystem<int>();
            em.AddSystem(system2);
            system2.OnEntityRemoved += x => removed2Called = true;

            Entity entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);
            em.AddComponent(entity, int.MaxValue);

            em.Update(0);

            em.RemoveComponent<string>(entity);

            Assert.IsFalse(removed2Called);

            em.Update(0);

            Assert.IsTrue(removed2Called);
        }

        [TestMethod]
        public void HavingASystemWithNoRequiredComponents_RemovingEntityShouldRemoveItFromSystem()
        {
            ISystem sys = new EmptyTestSystem();
            em.AddSystem(sys);

            Entity entity = em.CreateEntity();
            em.Update(0);

            em.DestroyEntity(entity);
            em.Update(0);

            Assert.AreEqual(0, sys.Entities.Count);
        }
    }
}
