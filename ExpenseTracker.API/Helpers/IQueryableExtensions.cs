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
            if (source == null)
            {
                throw new ArgumentNullException();
            }

        if (sort == null)
            {
                return source;
            }
            
            var lstSort = sort.Split(',');

            string completeSortExpression = "";
            foreach (var sortOption in lstSort)
            {
                if (sortOption.StartsWith("-"))
                {
                    completeSortExpression = completeSortExpression + sortOption.Remove(0, 1) + " descending,";
                }
                else
                {
                    completeSortExpression = completeSortExpression + sortOption + ",";
                }
            }

            if (!string.IsNullOrWhiteSpace(completeSortExpression))
            {
                completeSortExpression = completeSortExpression.Remove(completeSortExpression.Count() - 1);
                
                // FIX: OrderBy does not work at all. 
                source.OrderBy("id descending"); //(completeSortExpression);
            }

            return source;

        }
    }
}