// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace ScatteredLogic.Internal
{
    internal struct Bitmask64 : IBitmask<Bitmask64>
    {
        private readonly ulong m;

        private Bitmask64(ulong m)
        {
            this.m = m;
        }

        public bool Get(int index) => (m & (1ul << index)) != 0;
        public Bitmask64 Set(int index) => new Bitmask64(m | (1ul << index));
        public Bitmask64 Clear(int index) => new Bitmask64(m & ~(1ul << index));
        public bool Contains(Bitmask64 other) => (m & other.m) == other.m;

        public bool Equals(Bitmask64 other) => m == other.m;
        public override bool Equals(object obj) => obj is Bitmask64 ? Equals((Bitmask64)obj) : false;
        public override int GetHashCode() => m.GetHashCode();
    }
}
