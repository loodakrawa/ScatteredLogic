// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal sealed class NamingManager<E>
    {
        private readonly HashSet<E> emptyEntitySet = new HashSet<E>();
        private readonly HashSet<string> emptyStringSet = new HashSet<string>();

        private readonly Dictionary<E, string> names = new Dictionary<E, string>();

        private readonly Dictionary<string, HashSet<E>> entitiesByTag = new Dictionary<string, HashSet<E>>();
        private readonly Dictionary<E, HashSet<string>> tagsByEntity = new Dictionary<E, HashSet<string>>();

        private readonly Stack<HashSet<string>> stringHashPool = new Stack<HashSet<string>>();

        public string GetName(E entity) => names.TryGet(entity);
        public void SetName(E entity, string name) => names[entity] = name;

        public void RemoveEntitySync(E entity)
        {
            names.Remove(entity);
            HashSet<string> tags = tagsByEntity.TryGet(entity);
            if (tags != null)
            {
                foreach (string tag in tags)
                {
                    HashSet<E> entities = entitiesByTag.TryGet(tag);
                    if (entities != null) entities.Remove(entity);
                }
                tags.Clear();
                tagsByEntity.Remove(entity);
                stringHashPool.Push(tags);
            }
        }

        public void AddTag(E entity, string tag)
        {
            HashSet<string> tags = tagsByEntity.TryGet(entity);
            if (tags == null)
            {
                tags = stringHashPool.Count > 0 ? stringHashPool.Pop() : new HashSet<string>();
                tagsByEntity[entity] = tags;
            }
            tags.Add(tag);

            HashSet<E> entities = entitiesByTag.TryGet(tag);
            if(entities == null)
            {
                entities = new HashSet<E>();
                entitiesByTag[tag] = entities;
            }
            entities.Add(entity);
        }

        public void RemoveTag(E entity, string tag)
        {
            HashSet<string> tags = tagsByEntity.TryGet(entity);
            if (tags != null) tags.Remove(tag);
        }

        public bool HasTag(E entity, string tag)
        {
            HashSet<string> tags = tagsByEntity.TryGet(entity);
            return tags != null ? tags.Contains(tag) : false;
        }

        public HashSet<E> GetEntitiesWithTag(string tag) => entitiesByTag.TryGet(tag) ?? emptyEntitySet;
        public HashSet<string> GetEntityTags(E entity) => tagsByEntity.TryGet(entity) ?? emptyStringSet;
    }
}
