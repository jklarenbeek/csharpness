using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data;

namespace joham.cs_futils
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

        private static MemberExpression GetMemberExpression<T>(Expression<Func<T, object>> exp)
        {
            var member = exp.Body as MemberExpression;
            if (member != null)
                return member;

            var unary = exp.Body as UnaryExpression;
            if (unary != null)
                return unary.Operand as MemberExpression;

            return null;
        }
        public static DataTable ConvertToDataTable<TSource>(this IEnumerable<TSource>
                         records, params Expression<Func<TSource, object>>[] columns)
        {
            DataTable table = new DataTable();

            Dictionary<string, Func<TSource, object>> functions = new Dictionary<string, Func<TSource, object>>();
            foreach (var col in columns)
            {
                
                var member = GetMemberExpression(col);
                table.Columns.Add(member.Member.Name, member.Type);

                var function = col.Compile();
                functions.Add(member.Member.Name, function);
            }

            foreach (var record in records)
            {
                DataRow row = table.NewRow();

                foreach (string key in functions.Keys)
                {
                    var result = functions[key](record);
                    row[key] = result;
                }

                table.Rows.Add(row);
            }
            return table;
        }

    }

}