using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FirstAzureSearchApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Azure;
using Azure.Search.Documents;
using System.Linq;
//using Azure.Search.Documents.Models;

namespace FirstAzureSearchApp.Controllers
{

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(SearchData model)
        {
            try
            {
                // Ensure the search string is valid.
                if (model.searchText == null)
                {
                    model.searchText = "";
                }

                // Make the Azure Cognitive Search call.
                await RunQueryAsync(model);
            }

            catch
            {
                return View("Error", new ErrorViewModel { RequestId = "1" });
            }
            return View(model);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static SearchServiceClient _serviceClient;
        private static ISearchIndexClient _indexClient;
        private static IConfigurationBuilder _builder;
        private static IConfigurationRoot _configuration;
        private static SearchClient _srchclient;

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

        private async Task<ActionResult> RunQueryAsync(SearchData model)
        {
            InitSearch();

            var parameters = new SearchParameters
            {
                // Enter Hotel property names into this list so only these values will be returned.
                // If Select is empty, all values will be returned, which can be inefficient.
                //Select = new[] { "HotelName", "Description" }
            };
            SearchOptions options;
            //options = new SearchOptions()
            //{
            //    IncludeTotalCount = true,
            //    Filter = "",
            //    OrderBy = { "" }
            //};

            if (model.searchText.Contains('+'))
            {
                string[] searchArray=model.searchText.Split('+');

                string filterQuery = String.Join(",",searchArray).Trim();
                // offiName + ClientId + BussinessUnit + Region
                // torrance + * + 
                filterQuery = "search.in(Office, '" + filterQuery + "', ',') or search.in(ClientID, '" + filterQuery + "', ',') or search.in(BuisnessUnit, '" + filterQuery + "', ',') or search.in(Region, '" + filterQuery + "', ',') or search.in(ContactNo, '" + model.searchText + "', ',')";
                    
                //foreach (string search in searchArray)
                //{

                //    //filterQuery = filterQuery + "search.in(Office, '" + model.searchText + "', ',')";
                //}
                options = new SearchOptions()
                {
                    IncludeTotalCount = true,
                    Filter = filterQuery,
                    OrderBy = { "" },
                    Size=20
                };
            }
            else
            {
                options = new SearchOptions()
                {
                    IncludeTotalCount = true,
                    Filter = "search.in(Office, '" + model.searchText + "', ',') or search.in(ClientID, '" + model.searchText + "', ',') or search.in(BuisnessUnit, '" + model.searchText + "', ',') or search.in(Region, '" + model.searchText + "', ',') or search.in(ContactNo, '" + model.searchText + "', ',')",
                    OrderBy = { "" },                    
                    Size = 20
                };
            }



            // For efficiency, the search call should be asynchronous, so use SearchAsync rather than Search.
           Azure.Search.Documents.Models.SearchResults<Client> response = await _srchclient.SearchAsync<Client>(model.searchText, options);
             //Azure.Search.Documents.Models.SearchResults<Client> response = await _srchclient.SearchAsync<Client>("*", options);
            //model.resultList = response.GetResults();
            model.resultList = new System.Collections.Generic.List<Client>();

            //SearchResults<Hotel> response = await client.SearchAsync<Hotel>("luxury");
            foreach (Azure.Search.Documents.Models.SearchResult<Client> result in response.GetResults())
            {
                //    Hotel doc = result.Document;
                //    Console.WriteLine($"{doc.Id}: {doc.Name}");
                model.resultList.Add(result.Document);
            }

            model.totalCount= model.resultList.Count;
            // Display the results.
            return View("Index", model);
        }

        public async Task<ActionResult> SuggestAsync(bool highlights, bool fuzzy, string term)
        {
            InitSearch();

            // Setup the suggest parameters.
            var options = new SuggestOptions()
            {
                UseFuzzyMatching = fuzzy,
                Size = 8,
            };

            if (highlights)
            {
                options.HighlightPreTag = "<b>";
                options.HighlightPostTag = "</b>";
            }

            // Only one suggester can be specified per index. The name of the suggester is set when the suggester is specified by other API calls.
            // The suggester for the hotel database is called "sg" and simply searches the hotel name.
            var suggestResult = await _srchclient.SuggestAsync<Client>(term, "sg", options).ConfigureAwait(false);

            // Convert the suggest query results to a list that can be displayed in the client.
            System.Collections.Generic.List<string> suggestions = suggestResult.Value.Results.Select(x => x.Text).ToList();

            // Return the list of suggestions.
            return new JsonResult(suggestions);
        }

        [HttpGet]
        public async Task<ActionResult> AutoCompleteAndSuggestAsync(string term)
        {
            InitSearch();

            // Setup the type-ahead search parameters.
            var ap = new AutocompleteOptions()
            {
                Mode = Azure.Search.Documents.Models.AutocompleteMode.OneTermWithContext,
                Size = 1,
            };
            var autocompleteResult = await _srchclient.AutocompleteAsync(term, "sg", ap);

            // Setup the suggest search parameters.
            var sp = new SuggestOptions()
            {
                Size = 8,
            };

            // Only one suggester can be specified per index. The name of the suggester is set when the suggester is specified by other API calls.
            // The suggester for the hotel database is called "sg" and simply searches the hotel name.
            var suggestResult = await _srchclient.SuggestAsync<Client>(term, "sg", sp).ConfigureAwait(false);

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
                // Now add the suggestions.
                results.Add(suggestResult.Value.Results[n].Text);
            }

            // Return the list.
            return new JsonResult(results);
        }
    }
}
