// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

using System;

namespace ScatteredGameExample
{
    public static class RequiredTypes
    {
        public static readonly Type[] None = new Type[] { };
        public static Type[] From<T1>() => new Type[] { typeof(T1) };
        public static Type[] From<T1, T2>() => new Type[] { typeof(T1), typeof(T2) };
        public static Type[] From<T1, T2, T3>() => new Type[] { typeof(T1), typeof(T2), typeof(T3) };
        public static Type[] From<T1, T2, T3, T4>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
        public static Type[] From<T1, T2, T3, T4, T5>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
    }
}
