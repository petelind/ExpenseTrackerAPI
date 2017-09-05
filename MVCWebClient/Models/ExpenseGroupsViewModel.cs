using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExpenseTracker.DTO;
using MVCWebClient.Helpers;
using PagedList;

namespace MVCWebClient.Models
{
    public class ExpenseGroupsViewModel
    {
        public IPagedList<ExpenseGroup> ExpenseGroups { get; set; }
        public IEnumerable<ExpenseGroupStatus> ExpenseGroupStatuses { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }
}