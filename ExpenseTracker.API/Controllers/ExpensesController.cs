using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Factories;
using Marvin.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Routing;
using ExpenseTracker.API.Helpers;

namespace ExpenseTracker.API.Controllers
{
    [RoutePrefix("api")]
    public class ExpensesController : ApiController
    {

        IExpenseTrackerRepository _repository;
        ExpenseFactory _expenseFactory = new ExpenseFactory();

        public const int MaxPageSize = 10;

        public ExpensesController()
        {
            _repository = new ExpenseTrackerEFRepository(new Repository.Entities.ExpenseTrackerContext());
        }

        public ExpensesController(IExpenseTrackerRepository repository)
        {
            _repository = repository;
        }


        [Route("expenses", Name = "expensesList")]
        [Route("expensegroups/{expenseGroupId}/expenses")]
        [HttpGet]
        public IHttpActionResult Get(int? expenseGroupId = null, string sort = null, int pagesize = 10, int page = 1, string fieldsToRetrieve = null)
        {
            try
            {
                
                if (expenseGroupId == null)
                {
                    var expenses = _repository.GetExpenses()
                        .ApplySort(sort)
                        .ToList();

                    pagesize = pagesize > MaxPageSize ? MaxPageSize : pagesize;

                    int expensesCount = expenses.Count();
                    int pagesCount =
                        Convert.ToInt16(Math.Ceiling((Convert.ToDouble(expensesCount) / Convert.ToDouble(pagesize))));

                    var requestedSubset = expenses
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToList()
                        .Select(e => _expenseFactory.CreatedDatashapedExpense(e, fieldsToRetrieve));
                        
                    var urlHelper = new UrlHelper(Request);
                    var prevLink = pagesCount > 1
                        ? urlHelper.Link("ExpensesList", new
                        {
                            page = page - 1,
                            pagesCount = pagesCount,
                            pagesize = pagesize,
                            sort = sort
                        })
                        : "";

                    var nextLink = pagesCount > 1
                        ? urlHelper.Link("expensesList", new
                        {
                            page = page + 1,
                            pagesCount = pagesCount,
                            pagesize = pagesize,
                            sort = sort
                        })
                        : "";

                    var navigationHeader = new
                    {
                        currentPage = page,
                        pagesize = pagesize,
                        pagesCount = pagesCount,
                        prevLink = prevLink,
                        nextLink = nextLink,
                        fieldsToRetrieve = fieldsToRetrieve
                    };

                    HttpContext.Current.Response
                        .Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(navigationHeader));

                    return Ok(requestedSubset);
                }

                var checkIfEgExists = _repository.GetExpenseGroup((int)expenseGroupId);
                if (checkIfEgExists == null) return NotFound();

                var expensesForEg = _repository.GetExpenses((int)expenseGroupId).ToList().Select(e => _expenseFactory.CreatedDatashapedExpense(e, fieldsToRetrieve));
                return Ok(expensesForEg);

            }
            catch (Exception e)
            {
                var telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
                telemetryClient.TrackException(e);
                return InternalServerError();
            }
        }

        /// <summary>
        ///  Returns all Expenses or Expenses corresponding to the ExpenseGroup it is called within.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Inumerable<DTO.Expense> containing Expenses matching criteria.</returns>
        [Route("expenses/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {

                var result = _repository.DeleteExpense(id);

                if (result.Status == RepositoryActionStatus.Deleted)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }
                else if (result.Status == RepositoryActionStatus.NotFound)
                {
                    return NotFound();
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [Route("expenses")]
        public IHttpActionResult Post([FromBody]DTO.Expense expense)
        {
            try
            {
                if (expense == null)
                {
                    return BadRequest();
                }

                // map
                var exp = _expenseFactory.CreateExpense(expense);

                var result = _repository.InsertExpense(exp);
                if (result.Status == RepositoryActionStatus.Created)
                {
                    // map to dto
                    var newExp = _expenseFactory.CreateExpense(result.Entity);
                    return Created<DTO.Expense>(Request.RequestUri + "/" + newExp.Id.ToString(), newExp);
                }

                return BadRequest();

            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }


        [Route("expenses/{id}")]
        public IHttpActionResult Put(int id, [FromBody]DTO.Expense expense)
        {
            try
            {
                if (expense == null)
                {
                    return BadRequest();
                }

                // map
                var exp = _expenseFactory.CreateExpense(expense);

                var result = _repository.UpdateExpense(exp);
                if (result.Status == RepositoryActionStatus.Updated)
                {
                    // map to dto
                    var updatedExpense = _expenseFactory.CreateExpense(result.Entity);
                    return Ok(updatedExpense);
                }
                else if (result.Status == RepositoryActionStatus.NotFound)
                {
                    return NotFound();
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }


        [Route("expenses/{id}")]
        [HttpPatch]
        public IHttpActionResult Patch(int id, [FromBody]JsonPatchDocument<DTO.Expense> expensePatchDocument)
        {
            try
            {
                // find 
                if (expensePatchDocument == null)
                {
                    return BadRequest(); 
                }

                var expense = _repository.GetExpense(id);
                if (expense == null)
                {
                    return NotFound();
                }

                //// map
                var exp = _expenseFactory.CreateExpense(expense);

                // apply changes to the DTO
                expensePatchDocument.ApplyTo(exp);

                // map the DTO with applied changes to the entity, & update
                var result = _repository.UpdateExpense(_expenseFactory.CreateExpense(exp));

                if (result.Status == RepositoryActionStatus.Updated)
                {
                    // map to dto
                    var updatedExpense = _expenseFactory.CreateExpense(result.Entity);
                    return Ok(updatedExpense);
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }


         
    }
}