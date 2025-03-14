using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Il2CppSystem.Collections.Generic;

namespace TheOtherUs.Helper;

public static class ListHelper
{
    public static Random rnd { get; } = new((int)DateTime.Now.Ticks);

    public static T Get<T>(this List<T> list, int index)
    {
        return list._items[index];
    }

    public static T Get<T>(this List<T> list, Index index)
    {
        return list._items[index];
    }

    public static List<T> ToIl2cppList<T>(this System.Collections.Generic.List<T> list)
    {
        var il2cpList = new List<T>();
        foreach (var value in list) il2cpList.Add(value);
        return il2cpList;
    }


    public static T Random<T>(this List<T> list)
    {
        return list.Get(rnd.Next(list.Count - 1));
    }

    public static T Random<T>(this List<T> list, int Max)
    {
        return list.Get(rnd.Next(Max));
    }

    public static T Random<T>(this List<T> list, int Min, int Max)
    {
        return list.Get(rnd.Next(Min, Max));
    }

    public static T Random<T>(this System.Collections.Generic.List<T> list)
    {
        return list[rnd.Next(list.Count - 1)];
    }

    public static int RandomIndex<T>(this System.Collections.Generic.List<T> list)
    {
        return Random(list.Count - 1);
    }

    public static T Random<T>(this System.Collections.Generic.List<T> list, int Max)
    {
        return list[rnd.Next(Max)];
    }

    public static T Random<T>(this System.Collections.Generic.List<T> list, int Min, int Max)
    {
        return list[rnd.Next(Min, Max)];
    }

    public static IOrderedEnumerable<T> RandomSort<T>(this System.Collections.Generic.List<T> list)
    {
        return list.OrderBy(n => Guid.NewGuid());
    }

    public static int Random(int Min, int Max)
    {
        return rnd.Next(Min, Max);
    }

    public static int Random(int Max)
    {
        return rnd.Next(Max);
    }

    public static int Random()
    {
        return rnd.Next();
    }

    public static T[] CastArray<T>(this IEnumerable enumerable)
    {
        return enumerable.Cast<T>().ToArray();
    }

    public static System.Collections.Generic.List<T> OfTypeList<T>(this IEnumerable enumerable)
    {

        return enumerable.OfType<T>().ToList();
    }

    public static System.Collections.Generic.List<T> CastList<T>(
        this IEnumerable enumerable)
    {
        return enumerable.Cast<T>().ToList();
    }

    public static bool TryGet<T>(this System.Collections.Generic.List<T> list, Func<T, bool> isValue,
        [MaybeNullWhen(false)] out T Get)
    {
        var value = list.Where(isValue).FirstOrDefault();
        Get = value;
        return value == null;
    }
}