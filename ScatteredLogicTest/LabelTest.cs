// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScatteredLogic;

namespace ScatteredLogicTest
{
    [TestClass]
    public class LabelTest
    {
        private IEntityManager<Entity> em;

        [TestInitialize]
        public void SetUp()
        {
            em = EntityManagerFactory.Create(BitmaskSize.Bit64);
        }

        [TestMethod]
        public void HavingAnEntity_NameEqualsNull()
        {
            Entity entity = em.CreateEntity();
            Assert.IsNull(em.GetName(entity));
        }

        [TestMethod]
        public void HavingAnEntity_SetName_GetNameForEntityEqualsTheOneSet()
        {
            Entity entity = em.CreateEntity();
            em.SetName(entity, "name");
            Assert.AreEqual("name", em.GetName(entity));
        }

        [TestMethod]
        public void HavingAnEntity_SetNameAndThenSetToNull_NameEqualsNull()
        {
            Entity entity = em.CreateEntity();
            em.SetName(entity, "name");
            em.SetName(entity, null);
            Assert.IsNull(em.GetName(entity));
        }

        [TestMethod]
        public void HavingAnEntity_SetNameAndThenSetToAnotherValue_NameEqualsSecondValue()
        {
            Entity entity = em.CreateEntity();
            em.SetName(entity, "name");
            em.SetName(entity, "name2");
            Assert.AreEqual("name2", em.GetName(entity));
        }

        [TestMethod]
        public void HavingTwoEntities_SetNameForOne_NameOfSecondIsNull()
        {
            Entity entity = em.CreateEntity();
            Entity id2 = em.CreateEntity();
            em.SetName(entity, "name");
            Assert.IsNull(em.GetName(id2));
        }

        [TestMethod]
        public void HavingTwoEntities_SetDifferentNameForBothAndClearFirst_SecondOneHasTheSetName()
        {
            Entity entity = em.CreateEntity();
            Entity id2 = em.CreateEntity();
            em.SetName(entity, "name");
            em.SetName(id2, "name2");
            em.SetName(entity, null);
            Assert.AreEqual("name2", em.GetName(id2));
        }

        [TestMethod]
        public void HavingTwoEntities_SetSameNameForBothAndClearFirst_SecondOneHasTheSetName()
        {
            Entity entity = em.CreateEntity();
            Entity id2 = em.CreateEntity();
            em.SetName(entity, "name");
            em.SetName(id2, "name");
            em.SetName(entity, null);
            Assert.AreEqual("name", em.GetName(id2));
        }

        [TestMethod]
        public void HavingAnEntity_AddTag_EntityHasTag()
        {
            Entity entity = em.CreateEntity();
            em.AddTag(entity, "player");
            Assert.IsTrue(em.HasTag(entity, "player"));
        }

        [TestMethod]
        public void HavingAnEntity_AddTagAndRemoveTag_EntityDoesntHaveTheTag()
        {
            Entity entity = em.CreateEntity();
            em.AddTag(entity, "player");
            em.RemoveTag(entity, "player");
            Assert.IsFalse(em.HasTag(entity, "player"));
        }

        [TestMethod]
        public void HavingAnEntity_DoNotAddTag_EntityDoesntHaveTheTag()
        {
            Entity entity = em.CreateEntity();
            Assert.IsFalse(em.HasTag(entity, "player"));
        }

        [TestMethod]
        public void HavingTwoEntitites_AddTagToOne_OtherDoesntHaveTheTag()
        {
            Entity entity = em.CreateEntity();
            Entity entity2 = em.CreateEntity();
            em.AddTag(entity, "player");
            Assert.IsFalse(em.HasTag(entity2, "player"));
        }

        [TestMethod]
        public void AddTagToEntity_SetOfEntityTagsContainsTheTag()
        {
            Entity entity = em.CreateEntity();
            em.AddTag(entity, "player");

            bool found = false;
            foreach (string tag in em.GetEntityTags(entity)) if (tag == "player") found = true;

            Assert.IsTrue(found);
        }

        [TestMethod]
        public void AddTagToEntity_GetEntitiesForTagContainsEntity()
        {
            Entity entity = em.CreateEntity();
            em.AddTag(entity, "player");

            bool found = false;
            foreach (Entity e in em.GetEntitiesWithTag("player")) if (e == entity) found = true;

            Assert.IsTrue(found);
        }
    }
}
