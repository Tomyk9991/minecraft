using System;

public interface IntVector
{
    bool AnyAttribute(Predicate<int> predicate, out int value);
}
