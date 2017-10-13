// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license. See the LICENSE file for details.

namespace ScatteredLogic
{
    public interface IAspect
    {
        int EntityCount { get; }
        Entity[] Entities { get; }

        T[] GetComponents<T>();
    }
}
