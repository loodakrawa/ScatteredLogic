// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredLogic
{
    public struct Handle : IEquatable<Handle>
    {
        public const int MaxIndex = IndexMask;

        private const int IndexBits = 20;
        private const int VersionBits = sizeof(int) * 8 - IndexBits;
        private const int IndexMask = unchecked((int)(~0u >> VersionBits));

        public int Index => Id & IndexMask;
        public int Version => Id >> IndexBits;

        private readonly int Id;

        internal Handle(int id) => Id = id;

        public bool Equals(Handle other) => Id == other.Id;
        public override bool Equals(object obj) => obj is Handle ? Equals((Handle)obj) : false;
        public override int GetHashCode() => Id;

        public override string ToString() => string.Format("{0}|{1}", Index, Version);

        public Handle IncrementVersion()
        {
            int version = Version + 1;
            // don't allow version 0
            if (version == 0) ++version;
            return new Handle(Index | version << IndexBits);
        }
    }
}
