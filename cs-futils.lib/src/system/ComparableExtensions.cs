using System;

namespace joham.cs_futils
{

  public static class ComparableExtensions
  {
      public static bool Between<T>(this T a, T b, T c) where T:IComparable
      {
          if (b == null)
              return false;
          if (c == null)
              return (a.CompareTo(b) > 0);
          else
              return (a.CompareTo(b) >= 0 
                  && a.CompareTo(c) < 0);
      }
  }

}