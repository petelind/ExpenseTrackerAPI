using ExpenseTracker.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Cors;

namespace ExpenseTracker.Repository.Factories
{

    [EnableCors("*", "*", "*")]
    public class ExpenseFactory
    {

        public ExpenseFactory()
        {

        }

        public DTO.Expense CreateExpense(Expense expense)
        {
            return new DTO.Expense()
            {
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description,
                ExpenseGroupId = expense.ExpenseGroupId,
                Id = expense.Id
            };
        }



        public Expense CreateExpense(DTO.Expense expense)
        {
            return new Expense()
            {
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description,
                ExpenseGroupId = expense.ExpenseGroupId,
                Id = expense.Id
            };
        }

        public object CreatedDatashapedExpense(DTO.Expense expense, string fieldsToRetrieve)
        {

            List<string> fieldsRequested = new List<string>();
            if (fieldsToRetrieve != null) fieldsRequested = fieldsToRetrieve.ToLower().Split(',').ToList();
            
            // No shaping? Fine then, get default object
            if (!fieldsRequested.Any())
            {
                return expense;
            }

            // otherwise we will build object on the fly with ExpandoObject
            ExpandoObject dynamicObject = new ExpandoObject();

            // Now lets extract fields from expense provided one by one
            foreach (var field in fieldsRequested)
            {
                var fieldValueProperty = expense.GetType() // We use reflection to assess property...
                    .GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

                if (fieldValueProperty != null) // and if its indeed exists get its value
                {
                    var fieldValue = fieldValueProperty.GetValue(expense, null);
                    // and then we treat ExpandoObject as usual dictionary by casting it & writing field and its value into it
                    ((IDictionary<String, Object>) dynamicObject).Add(field, fieldValue);
                }
                else
                {
                    ((IDictionary<string, object>)dynamicObject).Add(field, "No such field exist in Expense.");
                }

            }

            return dynamicObject;

        }

        public object CreatedDatashapedExpense(Expense expense, string fieldsToRetrieve)
        {
            // stub call above overload - so we basically always use overload above
            return CreatedDatashapedExpense(CreateExpense(expense), fieldsToRetrieve);
        }

         
    }
}
