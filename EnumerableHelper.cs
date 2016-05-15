using System;

namespace UtmCliUtility
{
    public static class EnumerableHelper
    {
        public static int FirstIndexOf<T>(this T[] array, Func<T, bool> predicate)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i])) return i;
            }
            throw new Exception();
        }

        public static bool FirstIndexOf<T>(this T[] array, Func<T, bool> predicate, out int index)
        {
            index = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }
    }
}
