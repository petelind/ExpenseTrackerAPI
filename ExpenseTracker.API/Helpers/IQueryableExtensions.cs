﻿using System;
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

        if (String.IsNullOrEmpty(sort))
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

            if (!string.IsNullOrEmpty(completeSortExpression))
            {
                completeSortExpression = completeSortExpression.Remove(completeSortExpression.Count() - 1);
                
                
                source = source.OrderBy(completeSortExpression);
            }

            return source;

        }
    }
}