﻿// Copyright (c) 2017 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace ScatteredLogic.Internal
{
    internal static class DictHelper
    {
        public static V TryGet<K,V>(this Dictionary<K,V> dict, K key)
        {
            V value;
            dict.TryGetValue(key, out value);
            return value;
        }
    }
}
