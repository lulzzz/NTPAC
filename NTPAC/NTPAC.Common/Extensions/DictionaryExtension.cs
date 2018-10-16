using System;
using System.Collections.Generic;

namespace NTPAC.Common.Extensions
{
  public static class DictionaryExtension
  {
    public static Boolean RemoveSingleReferenceValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
      where TValue : class
    {
      foreach (var item in dictionary)
      {
        if (item.Value == value)
        {
          return dictionary.Remove(item.Key);
        }
      }

      return false;
    }
  }
}
