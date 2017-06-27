// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredLogic
{
    public struct Handle : IEquatable<Handle>
    {
        internal const int IndexBits = 20;
        internal const int IndexMask = (int)(0xffffffff >> (32 - IndexBits));

        public int Index => Id & IndexMask;
        public int Version => Id >> IndexBits;

        private readonly int Id;

        internal Handle(int id) => Id = id;

        public bool Equals(Handle other) => Id == other.Id;
        public override bool Equals(object obj) => obj is Handle ? Equals((Handle)obj) : false;

        public override int GetHashCode() => Id;

        public override string ToString() => string.Format("{0}|{1}", Index, Version);

        public static bool operator ==(Handle a, Handle b) => a.Id == b.Id;
        public static bool operator !=(Handle a, Handle b) => a.Id != b.Id;
    }
}
