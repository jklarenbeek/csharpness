using System;
using System.Globalization;

namespace joham.cs_futils
{
  public static class StringExtensions
  {
      public static string TrimAndLower(this String input)
      {
          return input.Trim().ToLower();
      }

      public static string ToCamelCase(this String input)
      {
          if (input == null) return "";

          TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
          string result = textInfo.ToTitleCase(input.Trim());

          return result;
      }

      public static string RemoveWhiteSpace(this String input)
      {
          int j = 0, inputlen = input.Length;
          char[] newarr = new char[inputlen];

          for (int i = 0; i < inputlen; ++i)
          {
              char tmp = input[i];

              if (!char.IsWhiteSpace(tmp))
              {
                  newarr[j] = tmp;
                  ++j;
              }
          }

          return new String(newarr, 0, j);

          // with linq
          //return new string(input.ToCharArray()
          //    .Where(c => !Char.IsWhiteSpace(c))
          //    .ToArray());
      }

      public static string Left(this string value, int length)
      {
          if (string.IsNullOrEmpty(value))
          {
              return value;
          }

          if (value.Length <= length)
              return value;
          else
              return value.Substring(0, length);
      }

      public static string Mid(this string value, int startPosition, int endPosition)
      {
          if (string.IsNullOrEmpty(value))
          {
              return value;
          }

          return value.Substring(startPosition, endPosition);
      }

      public static string Right(this string value, int length)
      {
          if (string.IsNullOrEmpty(value))
          {
              return value;
          }

          if (value.Length <= length)
              return value;
          else
              return value.Substring(value.Length - length);
      }

      public static string Replace(this string value, string oldValue, string newValue, StringComparison comparison)
      {
          int prevPos = 0;
          string retval = value;
          // find the first occurence of oldValue
          int pos = retval.IndexOf(oldValue, comparison);

          while (pos > -1)
          {
              // remove oldValue from the string
              retval = value.Remove(pos, oldValue.Length);

              // insert newValue in it's place
              retval = retval.Insert(pos, newValue);

              // check if oldValue is found further down
              prevPos = pos + newValue.Length;
              pos = retval.IndexOf(oldValue, prevPos, comparison);
          }

          return retval;
      }
  }
}