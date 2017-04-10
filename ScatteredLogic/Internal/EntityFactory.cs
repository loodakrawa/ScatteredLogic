// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace ScatteredLogic.Internal
{
    internal sealed class EntityFactory : IEntityFactory<Entity>
    {
        public IEntityManager<Entity> EntityManager { get; set; }

        private int lastId;

        public Entity Get() => new Entity(EntityManager, ++lastId);
        public void Return(Entity entity) { }
    }
}
