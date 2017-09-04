using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExpenseTracker.DTO;

namespace MVCWebClient.Models
{
    public class SingleExpenseGroupViewModel
    {
        public ExpenseGroup ExpenseGroup { get; set; }
        public IEnumerable<ExpenseGroupStatus> Statuses { get; set; }
    }
}