using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Routing;
using System.Web.UI.WebControls;
using ExpenseTracker.Repository.Entities;
using Marvin.JsonPatch;
using ExpenseTracker.API.Helpers;

namespace ExpenseTracker.API.Controllers
{
    [RoutePrefix("api")]
    public class ExpenseGroupsController : ApiController
    {
        IExpenseTrackerRepository _repository;
        ExpenseGroupFactory _expenseGroupFactory;

        public const int MaxPageSize = 100;

        public ExpenseGroupsController()
        {
            _repository = new ExpenseTrackerEFRepository(new 
                Repository.Entities.ExpenseTrackerContext());
            _expenseGroupFactory= new ExpenseGroupFactory();
        }

        public ExpenseGroupsController(IExpenseTrackerRepository repository)
        {
            _repository = repository;
        }    

        /// <summary>
        /// Get all available ExpenseGroups (by default) or subset matching supplied criteria, sorted.
        /// </summary>
        /// <returns>IEnumerable of DTO.ExpenseGroups</returns>
        [HttpGet]
        [Route("expensegroups", Name = "ExpenseGroupsList")]
        public IHttpActionResult Get(string sort = "-id", 
            int page = 1, int pageSize = 5, 
            string fieldsToRetrieve = null,
            string status = null, string userid = null,
            bool attachExpenses=false)
        {
            try
            {
                int statusId = -1;
                if (status != null)
                {
                    switch (status.ToLower())
                    {
                        case "open":
                            statusId = 1;
                            break;
                        case "confirmed":
                            statusId = 2;
                            break;
                        case "processed":
                            statusId = 3;
                            break;
                        default: break;
                    }
                }

                if (fieldsToRetrieve != null)
                {
                    List<string> fieldsList = new List<string>();
                    fieldsList = fieldsToRetrieve.ToLower().Split(',').ToList();
                    if (fieldsList.Contains("expenses")) attachExpenses = true;
                }           

                IQueryable<Repository.Entities.ExpenseGroup> expenseGroups = null;
                expenseGroups = attachExpenses ? _repository.GetExpenseGroupsWithExpenses() : _repository.GetExpenseGroups();

                expenseGroups = expenseGroups.ApplySort(sort)
                    .Where(eg => (statusId == -1 || eg.ExpenseGroupStatusId == statusId))
                    .Where(eg => (userid == null || eg.UserId == userid));
                    

                int totalExpenseGroups = expenseGroups.Count();
                int totalPages = Convert.ToInt16((Math.Ceiling(Convert.ToDouble(totalExpenseGroups / pageSize))));
                if (pageSize > MaxPageSize) pageSize = MaxPageSize;

                var requestedSubset = expenseGroups
                    .Skip((page -1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(eg => _expenseGroupFactory.CreateDatashapedExpenseGroup(eg, fieldsToRetrieve));

                var urlHelper = new UrlHelper(Request);
                var prevLink = page > 1
                    ? urlHelper.Link("ExpenseGroupsList", new
                    {
                        page = page -1,
                        pageSize = pageSize,
                        sort = sort,
                        userid = userid,
                        status = status
                    })
                    : "";

                var nextLink = page > 1
                    ? urlHelper.Link("ExpenseGroupsList", new
                    {
                        page = page + 1,
                        pageSize = pageSize,
                        sort = sort,
                        userid = userid,
                        status = status
                    })
                    : "";
                var paginationHeader = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalExpenseGroups,
                    totalPages = totalPages,
                    nextPage = nextLink,
                    previousPage = prevLink,
                    fieldsToRetrieve = fieldsToRetrieve

                };

                HttpContext.Current
                    .Response.Headers
                    .Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationHeader));

                return Ok(requestedSubset);

            }
            catch (Exception)
            {
                // FIX: Record exception to telemetry
                return InternalServerError();
            }
        }

        /// <summary>
        ///  Returns ExpenseGroups with Id you supplied. Set attachExpenses = true to get also all expenses within a group.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fieldsToRetrieve"></param>
        /// <param name="attachExpenses"></param>
        /// <returns>DTO.ExpenseGroup matching Id.</returns>
        [HttpGet]
        public IHttpActionResult Get (int id, string fieldsToRetrieve = null, bool attachExpenses = false)
        {
            try
            {
                List<string> fieldsRequested = new List<string>();
                if (fieldsToRetrieve != null) fieldsRequested = fieldsToRetrieve.ToLower().Split(',').ToList();

                if (fieldsRequested.Contains("expenses")) attachExpenses = true;
                var result = attachExpenses ? _repository.GetExpenseGroupWithExpenses(id) : _repository.GetExpenseGroup(id);

                if (result == null) return NotFound();
   
                var resultDto = _expenseGroupFactory.CreateDatashapedExpenseGroup(result, fieldsToRetrieve);
                return Ok(resultDto);

            }
            catch (Exception e)
            {
                var telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
                telemetryClient.TrackException(e);
                return InternalServerError();
            }
        }


        /// <summary>
        /// Creates an ExpenseGroup from DTO.ExpenseGroup you provide.
        /// </summary>
        /// <param name="newGroup"></param>
        /// <returns>URI + JSON representation</returns>
        [HttpPost]
        [Route("expensegroups")]
        [EnableCors("*","*","*")]
        public IHttpActionResult Post([FromBody] DTO.ExpenseGroup newGroup)
        {
            try
            {

                if (newGroup == null) return BadRequest();
                
                var eg = _expenseGroupFactory.CreateExpenseGroup(newGroup);
                var attemptingAddition = _repository.InsertExpenseGroup(eg);
                if (attemptingAddition.Status == RepositoryActionStatus.Created)
                {
                    var createdExpenseGroupDto = _expenseGroupFactory.CreateExpenseGroup(attemptingAddition.Entity);
                    return Created(Request.RequestUri + "/" + createdExpenseGroupDto.Id.ToString(),
                        createdExpenseGroupDto);
                }

                return BadRequest();

            }
            catch (Exception exception)
            {
                var telemetryClientclient = new Microsoft.ApplicationInsights.TelemetryClient();
                telemetryClientclient.TrackException(exception);
                return InternalServerError();
            }
            
            
        }

        /// <summary>
        /// Updates ExpenseGroups/{Id} with the values from DTO.ExpenseGroup you supplied.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="withThisNewValues"></param>
        /// <returns>DTO.ExpenseGroup with updates applied.</returns>
        [HttpPut]
        public IHttpActionResult Put(int id, [FromBody] DTO.ExpenseGroup withThisNewValues)
        {
            try
            {

                if (withThisNewValues == null) return BadRequest();

                var newValues = _expenseGroupFactory.CreateExpenseGroup(withThisNewValues);
                var update = _repository.UpdateExpenseGroup(newValues);

                if (update.Status == RepositoryActionStatus.NotFound) return NotFound();
                if (update.Status == RepositoryActionStatus.Updated)
                    return Ok(_expenseGroupFactory.CreateExpenseGroup(update.Entity));

                return BadRequest();

            }
            catch (Exception e)
            {
                var telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
                telemetryClient.TrackException(e);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Applies JSONPatchDocument to the ExpenseGroup with given Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patchToApply"></param>
        /// <returns>JSON of patched DTO.ExpenseGroup.</returns>
        [HttpPatch]
        public IHttpActionResult Patch(int id, [FromBody] JsonPatchDocument<DTO.ExpenseGroup> patchToApply)
        {
            try
            {
                if (patchToApply == null) return BadRequest();

                var egToPatch = _repository.GetExpenseGroup(id);
                if (egToPatch == null) return NotFound();

                // Pay attention: patch should BE APPLIED TO DTO! so...
                var egToPatchDto = _expenseGroupFactory.CreateExpenseGroup(egToPatch);
                patchToApply.ApplyTo(egToPatchDto);

                var attemptedPatch = // basically backward conversion in one-liner
                    _repository.UpdateExpenseGroup(_expenseGroupFactory.CreateExpenseGroup(egToPatchDto));

                if (attemptedPatch.Status == RepositoryActionStatus.Updated)
                {
                    return Ok(_expenseGroupFactory.CreateExpenseGroup(attemptedPatch.Entity));
                }

                return BadRequest();

            }
            catch (Exception e)
            {
                var telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
                telemetryClient.TrackException(e);
                return InternalServerError();
            }
            
        }

        
        /// <summary>
        /// Physically deletes ExpenseGroup with given Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>StatusCode(HttpStatusCode.NoContent) on success.</returns>
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var attemptedDeletion = _repository.DeleteExpenseGroup(id);

                if (attemptedDeletion.Status == RepositoryActionStatus.NotFound)
                {
                    return NotFound();
                }

                if (attemptedDeletion.Status == RepositoryActionStatus.Deleted)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }

                return BadRequest();

            }
            catch (Exception e)
            {
                var telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
                telemetryClient.TrackException(e);
                return InternalServerError();
            }
        }

    }
}
