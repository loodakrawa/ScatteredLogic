// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

namespace ScatteredLogic.Internal.Bitmask
{
    internal struct Bitmask32 : IBitmask<Bitmask32>
    {
        private readonly uint m;

        private Bitmask32(uint m)
        {
            this.m = m;
        }

        public bool Get(int index) => (m & (1u << index)) != 0;
        public Bitmask32 Set(int index) => new Bitmask32(m | (1u << index));
        public Bitmask32 Clear(int index) => new Bitmask32(m & ~(1u << index));
        public bool Contains(Bitmask32 other) => (m & other.m) == other.m;

        public bool Equals(Bitmask32 other) => m == other.m;
        public override bool Equals(object obj) => obj is Bitmask32 ? Equals((Bitmask32)obj) : false;
        public override int GetHashCode() => m.GetHashCode();
    }
}
