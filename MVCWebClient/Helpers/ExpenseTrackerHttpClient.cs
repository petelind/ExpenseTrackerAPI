using System;
using System.Net.Http;
using ExpenseTracker;

namespace MVCWebClient.Helpers
{
    public static class ExpenseTrackerHttpClient
    {
        public static HttpClient GetClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(ExpenseTrackerConstants.ExpenseTrackerAPI); // FIX: need to add configuration file for inserting production constants

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }
    }
}