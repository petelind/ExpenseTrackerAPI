using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Entities;
using ExpenseTracker.Repository.Factories;
using Microsoft.Ajax.Utilities;
using MVCWebClient.Helpers;
using MVCWebClient.Models;
using Newtonsoft.Json;
using ExpenseGroup = ExpenseTracker.DTO.ExpenseGroup;
using ExpenseGroupStatus = ExpenseTracker.DTO.ExpenseGroupStatus;

namespace MVCWebClient.Controllers
{
    public class ExpenseGroupsController : Controller
    {
        // GET: ExpenseGroups
        
        IExpenseTrackerRepository _repository;
        ExpenseMasterDataFactory _expenseMasterDataFactory;
        private ExpenseGroupFactory _expenseGroupFactory;

        public ExpenseGroupsController()
        {
            _repository = new ExpenseTrackerEFRepository(new ExpenseTrackerContext());
            _expenseMasterDataFactory = new ExpenseMasterDataFactory();
            _expenseGroupFactory = new ExpenseGroupFactory();
        }

        public async Task<ActionResult> Index()
        {
            var client = ExpenseTrackerHttpClient.GetClient();
            ExpenseGroupsViewModel viewModel = new ExpenseGroupsViewModel();

            // Lets fetch status codes we need to display messages
            // IDEA: Shouldnt we use eager loading to supply exacts status with each associated expense group?.. 
            HttpResponseMessage egsHttpResponseMessage = await client.GetAsync("api/expensegroupstatusses");
            if (!egsHttpResponseMessage.IsSuccessStatusCode)
            {
                return Content("There was an error accessing API, cannot get statuses.");
            }
            else
            {
                string codes = await egsHttpResponseMessage.Content.ReadAsStringAsync();
                viewModel.ExpenseGroupStatuses = JsonConvert.DeserializeObject<IEnumerable<ExpenseGroupStatus>>(codes);
            }

            // Now lets fetch expense groups
            HttpResponseMessage egResponseMessage = await client.GetAsync("api/expensegroups");

            if (egResponseMessage.IsSuccessStatusCode)
            {
                string content = await egResponseMessage.Content.ReadAsStringAsync();
                viewModel.ExpenseGroups = JsonConvert.DeserializeObject<IEnumerable<ExpenseGroup>>(content);
            }
            else
            {
                return Content("There was an error accessing API, cannot read Expense Groups.");
            }

            // We have everything client needs to assemble a list, lets return it
            return View(viewModel);

        }

        // GET: ExpenseGroups/Details/5
        public ActionResult Details(int id)
        {
            // TODO: implement it by yourself - it like edit, but without editing 
            return View();
        }

        // GET: ExpenseGroups/Create - returns empty view for editing
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            try
            {
                var client = ExpenseTrackerHttpClient.GetClient();

                SingleExpenseGroupViewModel viewModel = new SingleExpenseGroupViewModel();
                HttpResponseMessage egsHttpResponseMessage = await client.GetAsync("api/expensegroupstatusses");
                if (!egsHttpResponseMessage.IsSuccessStatusCode)
                {
                    return Content("There was an error accessing API.");
                }
                else
                {
                    string codes = await egsHttpResponseMessage.Content.ReadAsStringAsync();
                    viewModel.Statuses = JsonConvert.DeserializeObject<IEnumerable<ExpenseGroupStatus>>(codes);
                }

                viewModel.ExpenseGroup = new ExpenseGroup();
                ViewBag.FormHeader = "Create Expense Group";

                return View("Edit", viewModel);
            }
            catch (Exception e)
            {
                return Content("There was an error accessing API.");
            }
            
            
        }

        // POST: ExpenseGroups/Create
        [HttpPost]
        public ActionResult Create (ExpenseGroup expenseGroup)// (FormCollection collection)
        {
            try
            {
                expenseGroup.UserId = "http://local/api";
                return RedirectToAction("Save", expenseGroup);
                
            }
            catch
            {
                return View();
            }
        }

        // GET: ExpenseGroups/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                // set up new client
                var client = ExpenseTrackerHttpClient.GetClient();
                ExpenseGroup egToUpdateAsDto;
                SingleExpenseGroupViewModel viewModel = new SingleExpenseGroupViewModel();

                // retrieve group
                HttpResponseMessage egToUpdateAsJSON = await client.GetAsync("api/expensegroups/" + id);
                if (!egToUpdateAsJSON.IsSuccessStatusCode)
                {
                    return Content("There was en error accessing an API, cant get Expense Group with ID " + id);
                }
                else
                {
                    var egToUpdateAsString = await egToUpdateAsJSON.Content.ReadAsStringAsync();
                    // Lets not forget that such ID can be non-existent - in this content content will be precisely null
                    if (egToUpdateAsString == null) return Content("No such Expense Group Exist, sorry.");
                    viewModel.ExpenseGroup = JsonConvert.DeserializeObject<ExpenseGroup>(egToUpdateAsString);    
              }
 
                // set form header to EDIT
                ViewBag.FormHeader = "Edit Expense Group";

                // unfortunately, we also need to retrieve statuses along the way
                // TODO: Build a separate method to to this

                HttpResponseMessage egsHttpResponseMessage = await client.GetAsync("api/expensegroupstatusses");
                if (!egsHttpResponseMessage.IsSuccessStatusCode)
                {
                    return Content("There was an error accessing API.");
                }
                else
                {
                    string codes = await egsHttpResponseMessage.Content.ReadAsStringAsync();
                    viewModel.Statuses = JsonConvert.DeserializeObject<IEnumerable<ExpenseGroupStatus>>(codes);
                }

                // and redirect to EDIT view passing retrieved object to the view
                return View("Edit", viewModel);
            }
            catch
            {
                return Content("There was en error accessing an API.");
            }
        }

        // POST: ExpenseGroups/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                // get new client
                var client = ExpenseTrackerHttpClient.GetClient();

                // call method
                HttpResponseMessage response = await client.DeleteAsync("api/expensegroups/" + id);
                if (response.IsSuccessStatusCode)
                {
                    // if ok - return redirect to index
                    return RedirectToAction("Index");
                }
                else
                {
                    // otherwise return an error
                    return Content(
                        "Cannot delete this Expense Groups - it either does not exist, or was deleted already.");
                }
            }
            
            catch
            {
                return Content("There was an error accessing an API.");
            }
        }

        // SAVE: Used by Create & Edit to update database (via api calls, of course!)
        public async Task<ActionResult> Save(SingleExpenseGroupViewModel viewModel)
        {
            // BUG: there is a bug in this method. Can you identify & fix it? :)
            try
            {
                var client = ExpenseTrackerHttpClient.GetClient();
                if (viewModel.ExpenseGroup.UserId.IsNullOrWhiteSpace())
                {
                    viewModel.ExpenseGroup.UserId = "Batch processesor";
                }

                var serializedEg = JsonConvert.SerializeObject(viewModel.ExpenseGroup);

                var result = await client.PostAsync("api/expensegroups", 
                    new StringContent(serializedEg, Encoding.Unicode, "application/json"));

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "ExpenseGroups");
                }
                else
                {
                    return Content("Cannot save this ExpenseGroup, sorry.");
                }
            }

            catch (Exception)
            {
                return Content("Cannot save this ExpenseGroup, sorry.");
            }

        }

    }
}
