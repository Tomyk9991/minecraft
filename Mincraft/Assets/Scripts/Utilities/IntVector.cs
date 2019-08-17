using System;

namespace Core.Math
{
    public interface IntVector
    {
        bool AnyAttribute(Predicate<int> predicate, out int value);
    }
}
