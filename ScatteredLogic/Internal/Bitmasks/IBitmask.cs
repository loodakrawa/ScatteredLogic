﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredLogic.Internal.Bitmasks
{
    internal interface IBitmask<T> : IEquatable<T> where T : struct
    {
        bool Get(int index);
        T Set(int index);
        T Clear(int index);
        bool Contains(T other);
    }
}
