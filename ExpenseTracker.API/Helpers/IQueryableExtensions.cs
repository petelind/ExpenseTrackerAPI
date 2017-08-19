using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;

namespace ExpenseTracker.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string sort)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (sort == null) return source;

            var lstSort = sort.Split(',');
            string sortString = "";

            foreach (var sortOption in lstSort)
            {
                if (sortOption.StartsWith("-"))
                {
                    sortString= sortString + sortOption.Remove(0, 1) + " descending,";
                }

                else
                {
                    sortString = sortString + sortOption + ",";
                }
            }

            sortString = sortString.Remove(sortString.Length - 1);

            if (!string.IsNullOrEmpty(sortString))
            {
                source = source.OrderBy(sortString);
            }

            return source;

        }
    }
}