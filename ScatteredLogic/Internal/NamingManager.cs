// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class NamingManager
    {
        private readonly HashSet<Entity> emptyEntitySet = new HashSet<Entity>();
        private readonly HashSet<string> emptyStringSet = new HashSet<string>();

        private readonly Dictionary<Entity, string> names = new Dictionary<Entity, string>();

        private readonly Dictionary<string, HashSet<Entity>> entitiesByTag = new Dictionary<string, HashSet<Entity>>();
        private readonly Dictionary<Entity, HashSet<string>> tagsByEntity = new Dictionary<Entity, HashSet<string>>();

        private readonly Stack<HashSet<string>> stringHashPool = new Stack<HashSet<string>>();

        public string GetName(Entity entity) => names.TryGet(entity);
        public void SetName(Entity entity, string name) => names[entity] = name;

        public void RemoveEntitySync(Entity entity)
        {
            names.Remove(entity);
            HashSet<string> tags = tagsByEntity.TryGet(entity);
            if (tags != null)
            {
                foreach (string tag in tags)
                {
                    HashSet<Entity> entities = entitiesByTag.TryGet(tag);
                    if (entities != null) entities.Remove(entity);
                }
                tags.Clear();
                tagsByEntity.Remove(entity);
                stringHashPool.Push(tags);
            }
        }

        public void AddTag(Entity entity, string tag)
        {
            HashSet<string> tags = tagsByEntity.TryGet(entity);
            if (tags == null)
            {
                tags = stringHashPool.Count > 0 ? stringHashPool.Pop() : new HashSet<string>();
                tagsByEntity[entity] = tags;
            }
            tags.Add(tag);

            HashSet<Entity> entities = entitiesByTag.TryGet(tag);
            if(entities == null)
            {
                entities = new HashSet<Entity>();
                entitiesByTag[tag] = entities;
            }
            entities.Add(entity);
        }

        public void RemoveTag(Entity entity, string tag)
        {
            HashSet<string> tags = tagsByEntity.TryGet(entity);
            if (tags != null) tags.Remove(tag);
        }

        public bool HasTag(Entity entity, string tag)
        {
            HashSet<string> tags = tagsByEntity.TryGet(entity);
            return tags != null ? tags.Contains(tag) : false;
        }

        public HashSet<Entity> GetEntitiesWithTag(string tag) => entitiesByTag.TryGet(tag) ?? emptyEntitySet;
        public HashSet<string> GetEntityTags(Entity entity) => tagsByEntity.TryGet(entity) ?? emptyStringSet;
    }
}
