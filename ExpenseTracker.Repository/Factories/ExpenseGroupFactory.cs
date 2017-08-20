using ExpenseTracker.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Repository.Helpers;

namespace ExpenseTracker.Repository.Factories
{
    public class ExpenseGroupFactory
    {
        ExpenseFactory expenseFactory = new ExpenseFactory();

        public ExpenseGroupFactory()
        {

        }

        public ExpenseGroup CreateExpenseGroup(DTO.ExpenseGroup expenseGroup)
        {
            return new ExpenseGroup()
            {
                Description = expenseGroup.Description,
                ExpenseGroupStatusId = expenseGroup.ExpenseGroupStatusId,
                Id = expenseGroup.Id,
                Title = expenseGroup.Title,
                UserId = expenseGroup.UserId,
                Expenses = expenseGroup.Expenses == null ? new List<Expense>() : expenseGroup.Expenses.Select(e => expenseFactory.CreateExpense(e)).ToList()
            };
        }


        public DTO.ExpenseGroup CreateExpenseGroup(ExpenseGroup expenseGroup)
        {
            return new DTO.ExpenseGroup()
            {
                Description = expenseGroup.Description,
                ExpenseGroupStatusId = expenseGroup.ExpenseGroupStatusId,
                Id = expenseGroup.Id,
                Title = expenseGroup.Title,
                UserId = expenseGroup.UserId,
                Expenses = expenseGroup.Expenses.Select(e => expenseFactory.CreateExpense(e)).ToList()
            };
        }

        public object CreateDatashapedExpenseGroup(DTO.ExpenseGroup expenseGroup, string fieldsToRetrieve)
        {

            List<string> fieldsRequested = new List<string>();
            if (fieldsToRetrieve != null) fieldsRequested = fieldsToRetrieve.ToLower().Split(',').ToList();
            
            if (!fieldsRequested.Any())
            {
                return expenseGroup;
            }

            // magic part - we construct Dynamic object on the fly, only fields that were requested:
            ExpandoObject dynamicObject = new ExpandoObject(); // Expando is the type required
            foreach (var field in fieldsRequested)
            {
                // access propery via reflections
                var propertyInfo = expenseGroup.GetType()
                    .GetProperty(field, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);

                // if this property indeed exists - get its value and...
                if (propertyInfo != null)
                {
                    var value = propertyInfo
                        .GetValue(expenseGroup, null);
                    // ...treat dynamicObject as dictionary & add newly retreived field to it
                    ((IDictionary<string, object>) dynamicObject).Add(field, value);
                }
                else
                {
                    ((IDictionary<string, object>) dynamicObject).Add(field,
                        "Field does not exist in the ExpenseGroup.");
                }

            }

            return dynamicObject;
        }

        public object CreateDatashapedExpenseGroup(ExpenseGroup expenseGroup, string fieldsToRetrieve)
        {
            // stub - if somebody comes to us with ExpenseGroup - we turn it into DTO.ExpenseGroup and
            // ship back to proper method - one which uses DTO

            return CreateDatashapedExpenseGroup(CreateExpenseGroup(expenseGroup), fieldsToRetrieve);
        }


         
         
    }
}
