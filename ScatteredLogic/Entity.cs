// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredLogic
{
    public struct Entity : IEquatable<Entity>
    {
        internal const int IndexBits = 20;
        internal const int IndexMask = 0xFFFFF;

        private readonly int Id;

        public int Index => Id & IndexMask;
        public int Version => Id >> IndexBits;

        public bool Equals(Entity other) => Id == other.Id;
        public override bool Equals(object obj) => obj is Entity ? Equals((Entity)obj) : false;
        public override int GetHashCode() => Id;

#if DEBUG
        internal Entity(int id)
        {
            Id = id;
            Name = null;
        }
        public string Name { get; set; }
        public override string ToString() => string.Format("{0} ({1}:{2})", Name, Index, Version);
#else
        internal Entity(int id) => Id = id;
#endif

    }
}
