using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExpenseTracker.DTO;

namespace MVCWebClient.Models
{
    public class ExpenseGroupsViewModel
    {
        public IEnumerable<ExpenseGroup> ExpenseGroups { get; set; }
        public IEnumerable<ExpenseGroupStatus> ExpenseGroupStatuses { get; set; }

    }
}