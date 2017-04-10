// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScatteredLogic;
using System;

namespace ScatteredLogicTest
{
    [TestClass]
    public class EntityManagerTest
    {
        private IEntityManager<Entity> em;

        [TestInitialize]
        public void SetUp()
        {
            em = EntityManagerFactory.Create(BitmaskSize.Bit64);
        }

        [TestMethod]
        public void Empty_EntityNotManaged()
        {
            Assert.IsFalse(em.ContainsEntity(new Entity()));
        }

        [TestMethod]
        public void EntityAdded_EntityManaged()
        {
            Entity entity = em.CreateEntity();
            Assert.IsTrue(em.ContainsEntity(entity));
        }

        [TestMethod]
        public void EntityAddedAndRemoved_EntityNotManaged()
        {
            Entity entity = em.CreateEntity();
            em.DestroyEntity(entity);
            em.Update();

            Assert.IsFalse(em.ContainsEntity(entity));
        }

        [TestMethod]
        public void EntityAddedAndRemovedTwice_EntityNotManaged()
        {
            Entity entity = em.CreateEntity();
            em.DestroyEntity(entity);
            em.DestroyEntity(entity);
            em.Update();

            Assert.IsFalse(em.ContainsEntity(entity));
        }

        [TestMethod]
        public void TwoEntitiesAdded_BothEntitiesManaged()
        {
            Entity entity = em.CreateEntity();
            Entity id2 = em.CreateEntity();
            Assert.IsTrue(em.ContainsEntity(entity));
            Assert.IsTrue(em.ContainsEntity(id2));
        }

        [TestMethod]
        public void TwoEntitiesAdded_HaveDifferentides()
        {
            Entity entity = em.CreateEntity();
            Entity id2 = em.CreateEntity();
            Assert.AreNotEqual(entity, id2);     
        }

        [TestMethod]
        public void TwoEntitiesAddedAndOneRemoved_OneEntityManaged()
        {
            Entity entity = em.CreateEntity();
            Entity id2 = em.CreateEntity();
            em.DestroyEntity(id2);
            Assert.IsTrue(em.ContainsEntity(entity));
        }

        [TestMethod]
        public void TwoEntitiesAddedAndOneRemoved_RemovedEntityNotManaged()
        {
            Entity entity = em.CreateEntity();
            Entity id2 = em.CreateEntity();
            em.DestroyEntity(id2);
            em.Update();
            Assert.IsFalse(em.ContainsEntity(id2));
        }

        [TestMethod]
        public void AddComponentAndUpdate_HasComponent()
        {
            Entity entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);
            em.Update();
            Assert.IsTrue(em.HasComponent<string>(entity));
        }

        [TestMethod]
        public void AddTwoComponentsAndUpdate_HasBothComponents()
        {
            Entity entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);
            em.AddComponent(entity, 0);
            em.Update();
            Assert.IsTrue(em.HasComponent<string>(entity));
            Assert.IsTrue(em.HasComponent<int>(entity));
        }

        [TestMethod]
        public void AddTwoComponentsAndRemoveOneAndUpdate_HasOneAndDoesNotHaveOther()
        {
            Entity entity = em.CreateEntity();
            em.AddComponent(entity, string.Empty);
            em.AddComponent(entity, 0);
            em.RemoveComponent<int>(entity);
            em.Update();
            Assert.IsTrue(em.HasComponent<string>(entity));
            Assert.IsFalse(em.HasComponent<int>(entity));
        }

        [TestMethod]
        public void AddComponentAndUpdate_GetComponentIsTheOneAdded()
        {
            Entity entity = em.CreateEntity();
            string component = string.Empty;
            em.AddComponent(entity, component);
            em.Update();
            Assert.AreEqual(component, em.GetComponent<string>(entity));
        }

        [TestMethod]
        public void AddTwoComponentsAndUpdate_BothComponentsAreTheOnesAdded()
        {
            Entity entity = em.CreateEntity();
            string comp1 = string.Empty;
            int comp2 = 256;
            em.AddComponent(entity, comp1);
            em.AddComponent(entity, comp2);
            em.Update();
            Assert.AreEqual(comp1, em.GetComponent<string>(entity));
            Assert.AreEqual(comp2, em.GetComponent<int>(entity));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetComponentOnANonManagedEntity_ThrowsArgumentException()
        {
            Entity entity = new Entity();
            string value = em.GetComponent<string>(entity);
            Assert.IsNull(value);
        }
    }
}
