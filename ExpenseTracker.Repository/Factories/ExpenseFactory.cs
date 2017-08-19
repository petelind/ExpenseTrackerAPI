using ExpenseTracker.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository.Factories
{
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

        public object CreatedDatashapedExpense(DTO.Expense expense, List<string> fieldsToRetrieve)
        {
            // No shaping? Fine then, get default object
            if (!fieldsToRetrieve.Any())
            {
                return expense;
            }

            // otherwise we will build object on the fly with ExpandoObject
            ExpandoObject dynamicObject = new ExpandoObject();

            // Now lets extract fields from expense provided one by one
            foreach (var field in fieldsToRetrieve)
            {
                var fieldValue = expense.GetType() // We use reflection to assess property and get its value
                    .GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public)
                    .GetValue(expense, null);

                // and then we treat ExpandoObject as usual dictionary by casting it & writing field and its value into it
                ((IDictionary<String, Object>)dynamicObject).Add(field, fieldValue);
            }

            return dynamicObject;


        }

        public object CreatedDatashapedExpense(Expense expense, List<string> fieldsToRetrieve)
        {
            // stub call above overload - so we basically always use overload above
            return CreatedDatashapedExpense(CreateExpense(expense), fieldsToRetrieve);
        }

         
    }
}
