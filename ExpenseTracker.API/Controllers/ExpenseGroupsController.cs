using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Marvin.JsonPatch;
using Microsoft.ApplicationInsights;
using ExpenseTracker.API.Helpers;

namespace ExpenseTracker.API.Controllers
{
    public class ExpenseGroupsController : ApiController
    {
        IExpenseTrackerRepository _repository;
        ExpenseGroupFactory _expenseGroupFactory = new ExpenseGroupFactory();

        public ExpenseGroupsController()
        {
            _repository = new ExpenseTrackerEFRepository(new 
                Repository.Entities.ExpenseTrackerContext());
        }

        public ExpenseGroupsController(IExpenseTrackerRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IHttpActionResult Get(string sort = "-id", string status = "")
        {
            int statusCode;

            try
            {

                switch (status.ToLower())
                {
                    case "open":
                        statusCode = 1;
                        break;
                    case "confirmed":
                        statusCode = 2;
                        break;
                    case "processed":
                        statusCode = 3;
                        break;
                    default:
                        statusCode = -1;
                        break;
                }
                
                var results = _repository.GetExpenseGroups()
                    .Where( ex => ex.ExpenseGroupStatusId == statusCode)
                    .ApplySort(sort)
                    .ToList()
                    .Select(e => _expenseGroupFactory.CreateExpenseGroup(e));

                return Ok(results);
            }
            catch (Exception e)
            {
               TelemetryClient telemetry = new TelemetryClient();
               telemetry.TrackException(e);
               return InternalServerError();
            }
        }

        public IHttpActionResult Get(int id)
        {
            try
            {
                var expenseGroup = _repository.GetExpenseGroup(id);
                if (expenseGroup == null) return NotFound();
                return Ok(_expenseGroupFactory.CreateExpenseGroup(expenseGroup));

            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpPost]
        public IHttpActionResult Post(DTO.ExpenseGroup expenseGroup)
        {
            try
            {
                if (expenseGroup == null || !ModelState.IsValid)
                {
                    return BadRequest();
                }
                
                var eg = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);
                var result = _repository.InsertExpenseGroup(eg);

                if (result.Status == RepositoryActionStatus.Created)
                {
                    var newExpenseGroup = _expenseGroupFactory.CreateExpenseGroup(result.Entity);
                    return Created(Request.RequestUri + "/" + newExpenseGroup.Id.ToString(), newExpenseGroup); // basically we return path & JSON representation
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        /// Updates resources with the given ID to the state provided by the supplied ExpenseGroupDTO.
        [HttpPut]
        public IHttpActionResult Put(int id, DTO.ExpenseGroup expenseGroup)
        {
            try
            {
                // check if we got valid input
                if (expenseGroup == null || id < 1 || !ModelState.IsValid) return BadRequest();
                // then find the ExpenseGroup with given ID
                var egToBeUpdated = _repository.GetExpenseGroup(id);
                // if nothing found - return NotFound
                if (egToBeUpdated == null)
                {
                    return NotFound();
                }
                // else get this ExpenseGroup & update it to the status given
                var egUpdateSource = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);
                egToBeUpdated = egUpdateSource;
                // and then save it
                var result = _repository.UpdateExpenseGroup(egToBeUpdated);
                if (result.Status == RepositoryActionStatus.Updated)
                    return Ok(_expenseGroupFactory.CreateExpenseGroup(egToBeUpdated));
                else
                {
                    return BadRequest();
                }
            }
            // return error if failed to do this block
            catch (Exception)
            {
                return InternalServerError();
            }
            
        }

        /// Updates required fields of the EG with given id to the state described in patch.
        [HttpPatch]
        public IHttpActionResult Patch(int id, [FromBody] JsonPatchDocument<DTO.ExpenseGroup> egPatchDocument)
        {
            try
            {
                if (id < 1 || egPatchDocument == null) return BadRequest("Expected: op, path to parameter, new value!");
                var egToBePatched = _repository.GetExpenseGroup(id);
                if (egToBePatched == null) return NotFound();
                var egToBePatchedAsDto = _expenseGroupFactory.CreateExpenseGroup(egToBePatched);
                egPatchDocument.ApplyTo(egToBePatchedAsDto);
                var result = _repository.UpdateExpenseGroup(_expenseGroupFactory.CreateExpenseGroup(egToBePatchedAsDto));
                if (result.Status == RepositoryActionStatus.Updated)
                    return Ok(_expenseGroupFactory.CreateExpenseGroup(result.Entity));
                return BadRequest();
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        /// Delete EG with the ID supplied or returns NotFound if there is none.
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            if (id < 1 )
            {
                return BadRequest("IDs are always positive!");
            }

            var result = _repository.DeleteExpenseGroup(id);
            if (result.Status == RepositoryActionStatus.Deleted) return StatusCode(HttpStatusCode.NoContent);
            else return NotFound();

            return BadRequest();
        }

    }
}
