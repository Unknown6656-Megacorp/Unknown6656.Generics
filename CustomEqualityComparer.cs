using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Unknown6656.Generics;


public delegate bool EqualityComparator<T>([MaybeNull] T x, [MaybeNull] T y);

public sealed class CustomEqualityComparer<T>
    : IEqualityComparer<T>
{
    private readonly EqualityComparator<T> _func;


    public CustomEqualityComparer(EqualityComparator<T> equals) => _func = equals;

    public unsafe CustomEqualityComparer(delegate*<T, T, bool> equals) => _func = (x, y) => equals(x, y);

    public bool Equals([MaybeNull] T x, [MaybeNull] T y) => _func(x, y);

    public int GetHashCode(T _) => 0;


    public static unsafe implicit operator CustomEqualityComparer<T>(delegate*<T, T, bool> equals) => new(equals);

    public static implicit operator EqualityComparator<T>(CustomEqualityComparer<T> f) => f._func;

    public static implicit operator CustomEqualityComparer<T>(EqualityComparator<T> f) => new(f);

    public static implicit operator Func<T, T, bool>(CustomEqualityComparer<T> f) => (x, y) => f._func(x, y);

    public static implicit operator CustomEqualityComparer<T>(Func<T, T, bool> f) => new((x, y) => f(x, y));
}

public sealed class SequentialDistinctEqualityCompararer<T>
    : IEqualityComparer<IEnumerable<T>>
{
    public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y) => x?.SequenceEqual(y) ?? y is null;

    public int GetHashCode(IEnumerable<T> obj)
    {
        T[] arr = obj?.ToArray() ?? Array.Empty<T>();

        return arr.Aggregate(arr.Length, (acc, e) => HashCode.Combine(acc, e?.GetHashCode() ?? 0));
    }
}
