using System;
using System.Collections.Generic;

namespace NTPAC.Common.Extensions
{
  public static class ArrayExtension
  {
    public static void AddRange<T>(this T[] array, Int32 index, IEnumerable<T> toAdd)
    {
      array.AddRange(index, toAdd.GetEnumerator());
    }

    public static void AddRange<T>(this T[] array, Int32 index, IEnumerator<T> toAddEnumerator)
    {
      do
      {
        if (index >= array.Length)
        {
          throw new ArgumentOutOfRangeException();
        }

        array[index++] = toAddEnumerator.Current;
      } while (toAddEnumerator.MoveNext());
    }

    public static Boolean ContentsEqual<T>(this T[] array, T[] otherArray, Int32 arrayOffset, Int32 otherArrayOffset, Int32 len)
      where T : IComparable<T>
    {
      for (var i = 0; i < len; i++)
      {
        if (array[arrayOffset + i].CompareTo(otherArray[otherArrayOffset + i]) != 0)
        {
          return false;
        }
      }

      return true;
    }
  }
}
