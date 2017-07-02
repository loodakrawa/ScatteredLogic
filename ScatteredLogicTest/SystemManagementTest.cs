﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScatteredLogic;
using System;
using System.Collections.Generic;

namespace ScatteredLogicTest
{
    public class EmptyTestSystem : ISystem
    {
        public IEntitySystemManager EntityWorld { get; set; }
        public virtual IEnumerable<Type> RequiredComponents => RequiredTypes.None;
        public IHandleSet Entities { get; set; }
        public ISystemInfo Info { get; set; }

        public void Added() { }
        public void Removed() { }
        public void EntityAdded(Handle entity) { }
        public void EntityRemoved(Handle entity) { }
    }

    public class TestSystem<T> : ISystem
    {
        public IEntitySystemManager EntityWorld { get; set; }
        public IEnumerable<Type> RequiredComponents => RequiredTypes.From<T>();
        public IHandleSet Entities { get; set; }
        public ISystemInfo Info { get; set; }

        public int Index { get; set; }

        public event Action OnAdded = () => { };
        public event Action OnRemoved = () => { };

        public event Action<Handle> OnEntityAdded = x => { };
        public event Action<Handle> OnEntityRemoved = x => { };

        public int EntityCount { get; private set; }

        public void Added() => OnAdded();
        public void Removed() => OnRemoved();
        public void EntityAdded(Handle entity) => OnEntityAdded(entity);
        public void EntityRemoved(Handle entity) => OnEntityRemoved(entity);
        public void Update(IHandleSet entities) => EntityCount = entities.Count;
    }

    [TestClass]
    public class SystemManagementTest
    {
        private IEntitySystemManager em;
        private TestSystem<string> system;

        [TestInitialize]
        public void SetUp()
        {
            em = EntityManagerFactory.CreateEntitySystemManager(BitmaskSize.Bit64, 256);
            system = new TestSystem<string>();
        }

        [TestMethod, Timeout(100)]
        public void AddSystem_AddedCalled()
        {
            bool added = false;
            system.OnAdded += () => added = true;

            em.AddSystem(system);
            em.Update();

            Assert.IsTrue(added);
        }

        [TestMethod, Timeout(100)]
        public void AddSystemTwice_AddedCalledOnce()
        {
            int count = 0;
            system.OnAdded += () => ++count;

            em.AddSystem(system);
            em.AddSystem(system);

            em.Update();

            Assert.AreEqual(1, count);
        }

        [TestMethod, Timeout(100)]
        public void RemoveSystemWithoutAdding_RemovedNotCalled()
        {
            bool removed = false;
            system.OnRemoved += () => removed = true;

            em.RemoveSystem(system);

            Assert.IsFalse(removed);
        }

        [TestMethod, Timeout(0)]
        public void AddSystemRemoveSystem_RemoveCalled()
        {
            bool removed = false;
            system.OnRemoved += () => removed = true;

            em.AddSystem(system);
            em.RemoveSystem(system);

            Assert.IsFalse(removed);

            em.Update();

            Assert.IsTrue(removed);
        }

        [TestMethod, Timeout(100)]
        public void AddSystemRemoveSystemTwice_RemoveCalledOnce()
        {
            int count = 0;
            system.OnRemoved += () => ++count;

            em.AddSystem(system);
            em.RemoveSystem(system);
            em.RemoveSystem(system);

            em.Update();

            Assert.AreEqual(1, count);
        }

        [TestMethod, Timeout(100)]
        public void AddEntity_EntityAddedCalled()
        {
            Handle? addedEntity = null;
            system.OnEntityAdded += x => addedEntity = x;

            em.AddSystem(system);

            Handle entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);

            Assert.IsFalse(addedEntity.HasValue);

            em.Update();

            Assert.IsTrue(addedEntity.HasValue);
            Assert.AreEqual(entity, addedEntity.Value);
        }

        [TestMethod, Timeout(100)]
        public void AddEntityRemoveEntity_EntityRemovedCalled()
        {
            Handle? removedEntity = null;

            system.OnEntityRemoved += x => removedEntity = x;

            em.AddSystem(system);

            Handle entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);

            Assert.IsFalse(removedEntity.HasValue);

            em.Update();
            em.RemoveComponent<string>(entity);
            em.Update();

            Assert.IsTrue(removedEntity.HasValue);
            Assert.AreEqual(entity, removedEntity.Value);
        }

        [TestMethod, Timeout(100)]
        public void AddEntityWithDifferentComponent_EntityAddedNotCalled()
        {
            Handle? addedEntity = null;
            system.OnEntityAdded += x => addedEntity = x;

            em.AddSystem(system);

            Handle entity = em.CreateEntity();
            em.AddComponent(entity, int.MaxValue);

            em.Update();

            Assert.IsFalse(addedEntity.HasValue);
        }

        [TestMethod, Timeout(100)]
        public void AddTwoEntitiesWithDifferentComponents_OnlyOneEntityAdded()
        {
            em.AddSystem(system);

            Handle entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);

            Handle entity2 = em.CreateEntity();
            em.AddComponent(entity2, int.MaxValue);

            em.Update();

            Assert.AreEqual(1, system.Entities.Count);
        }

        [TestMethod, Timeout(100)]
        public void EntityAdded_DestroyEntity()
        {
            em.AddSystem(system);

            system.OnEntityAdded += x => em.DestroyEntity(x);

            Handle entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);

            em.Update();

            Assert.IsFalse(em.ContainsEntity(entity));
        }

        [TestMethod, Timeout(100)]
        public void EntityAdded_ChainDestroyEntity()
        {
            em.AddSystem(system);
            system.OnEntityRemoved += x => em.DestroyEntity(x);

            bool removed2Called = false;
            TestSystem<int> system2 = new TestSystem<int>();
            em.AddSystem(system2);
            system2.OnEntityRemoved += x => removed2Called = true;

            Handle entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);
            em.AddComponent(entity, int.MaxValue);

            em.Update();

            em.RemoveComponent<string>(entity);

            Assert.IsFalse(removed2Called);

            em.Update();

            Assert.IsTrue(removed2Called);
        }

        [TestMethod, Timeout(100)]
        public void HavingASystemWithNoRequiredComponents_RemovingEntityShouldRemoveItFromSystem()
        {
            EmptyTestSystem sys = new EmptyTestSystem();
            em.AddSystem(sys);

            Handle entity = em.CreateEntity();
            em.Update();

            em.DestroyEntity(entity);
            em.Update();

            Assert.AreEqual(0, sys.Entities.Count);
        }
    }
}
