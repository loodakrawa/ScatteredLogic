// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

namespace ScatteredLogic.Internal.Bitmasks
{
    internal struct Bitmask128 : IBitmask<Bitmask128>
    {
        private const int Size = sizeof(ulong) * 8;

        private readonly ulong m1;
        private readonly ulong m2;

        private Bitmask128(ulong m1, ulong m2)
        {
            this.m1 = m1;
            this.m2 = m2;
        }

        public bool Get(int index) => index < Size ? ((m1 & (1u << index)) != 0) : ((m2 & (1u << index - Size)) != 0);
        public Bitmask128 Set(int index) => index < Size ? new Bitmask128(m1 | (1ul << index), m2) : new Bitmask128(m1, m2 | (1ul << (index - Size)));
        public Bitmask128 Clear(int index) => index < Size ? new Bitmask128(m1 & ~(1ul << index), m2) : new Bitmask128(m1, m2 & ~(1ul << (index - Size)));
        public bool Contains(Bitmask128 other) => (m1 & other.m1) == other.m1 && (m2 & other.m2) == other.m2;

        public bool Equals(Bitmask128 other) => m1 == other.m1 && m2 == other.m2;
        public override bool Equals(object obj) => obj is Bitmask128 ? Equals((Bitmask128)obj) : false;
        public override int GetHashCode() => (92821 + m1.GetHashCode()) * 92821 + m2.GetHashCode();
    }
}