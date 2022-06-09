using Azure;
using Azure.Search.Documents;
using FirstAzureSearchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Configuration;
using System;

namespace FirstAzureSearchApp.Controllers
{
    public class Suggestion : Controller
    {

        private static SearchServiceClient _serviceClient;
        private static IConfigurationBuilder _builder;
        private static IConfigurationRoot _configuration;
        private static SearchClient _srchclient;
        private static ISearchIndexClient _indexClient;

        private void InitSearch()
        {

            // Create a configuration using the appsettings file.
            _builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _configuration = _builder.Build();


            string serviceName = _configuration["SearchServiceName"];
            string apiKey = _configuration["SearchServiceQueryApiKey"];
            string indexName = _configuration["SearchIndex"];

            // Create a SearchIndexClient to send create/delete index commands
            Uri serviceEndpoint = new Uri($"https://{serviceName}.search.windows.net/");
            AzureKeyCredential credential = new AzureKeyCredential(apiKey);

            //// Create a SearchClient to load and query documents
            _srchclient = new SearchClient(serviceEndpoint, indexName, credential);




            // Create a service and index client.
            _serviceClient = new SearchServiceClient($"https://{serviceName}.search.windows.net/", new SearchCredentials(apiKey));
            _indexClient = _serviceClient.Indexes.GetClient(indexName);
        }
        public IActionResult Index(string term)
        {
            //return View();
            InitSearch();

            // Setup the type-ahead search parameters.
            var ap = new AutocompleteOptions()
            {
                Mode = Azure.Search.Documents.Models.AutocompleteMode.OneTermWithContext,
                Size = 1,
            };
            var autocompleteResult = _srchclient.Autocomplete(term, "sg", ap);

            // Setup the suggest search parameters.
            var sp = new SuggestOptions()
            {
                Size = 15,
            };

            // Only one suggester can be specified per index. The name of the suggester is set when the suggester is specified by other API calls.
            // The suggester for the hotel database is called "sg" and simply searches the hotel name.
            var suggestResult = _srchclient.Suggest<Client>(term, "sg", sp);

            // Create an empty list.
            var results = new System.Collections.Generic.List<string>();

            if (autocompleteResult.Value.Results.Count > 0)
            {

                // Add the top result for type-ahead.
                results.Add(autocompleteResult.Value.Results[0].Text);
            }
            else
            {
                // There were no type-ahead suggestions, so add an empty string.
                results.Add("");
            }

            for (int n = 0; n < suggestResult.Value.Results.Count; n++)
            {
                string currentResult = suggestResult.Value.Results[n].Text;
                if (!results.Contains(currentResult))
                    // Now add the suggestions.
                    results.Add(currentResult);
            }
            // Return the list.
            return new JsonResult(results);
        }
    }
}
