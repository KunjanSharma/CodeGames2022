using System;
using System.Text;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using NPOI.SS.Formula.Functions;

namespace AzureSearch.Quickstart

{
    class Program
    {
        static void Main(string[] args)
        {
            //string serviceName = "codegames2022";
            //string apiKey = "nMOSWY0c29Yp9jp8jxlD12028SPbBIC1eXnn3snf5KAzSeD3P5gj";
            //string indexName = "client-search";



            string serviceName = "clientsearch2022";
            string apiKey = "M6JeWipWcCwld3WtW0UPbhP7FekIP5XJ1HeGT4oBh8AzSeCvZIw9";
            string indexName = "client-search";




            // Create a SearchIndexClient to send create/delete index commands
            Uri serviceEndpoint = new Uri($"https://{serviceName}.search.windows.net/");
            AzureKeyCredential credential = new AzureKeyCredential(apiKey);
            SearchIndexClient adminClient = new SearchIndexClient(serviceEndpoint, credential);

            //// Create a SearchClient to load and query documents
            SearchClient srchclient = new SearchClient(serviceEndpoint, indexName, credential);

            //// Delete index if it exists
            Console.WriteLine("{0}", "Deleting index...\n");
            DeleteIndexIfExists(indexName, adminClient);

            //// Create index
            Console.WriteLine("{0}", "Creating index...\n");
            CreateIndex(indexName, adminClient);

            SearchClient ingesterClient = adminClient.GetSearchClient(indexName);

            // Load documents
            for (int i = 0; i < 20; i++) { 
            
            Console.WriteLine("{0}", "Uploading documents...\n");
            UploadDocuments(ingesterClient,i);
            }

            // Wait 2 secondsfor indexing to complete before starting queries (for demo and console-app purposes only)
            Console.WriteLine("Waiting for indexing...\n");
            System.Threading.Thread.Sleep(2000);

            // Call the RunQueries method to invoke a series of queries
            Console.WriteLine("Starting queries...\n");
            RunQueries(srchclient);

            // End the program
            Console.WriteLine("{0}", "Complete. Press any key to end this program...\n");
            Console.ReadKey();
        }

        // Delete the hotels-quickstart index to reuse its name
        private static void DeleteIndexIfExists(string indexName, SearchIndexClient adminClient)
        {
            adminClient.GetIndexNames();
            {
                adminClient.DeleteIndex(indexName);
            }
        }
        // Create hotels-quickstart index
        private static void CreateIndex(string indexName, SearchIndexClient adminClient)
        {
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(Client));

            var definition = new SearchIndex(indexName, searchFields);

            var suggester = new SearchSuggester("sg", new[] { "ClientID", "Office", "BuisnessUnit", "Region", "ContactNo", "ClientSortName" });
            definition.Suggesters.Add(suggester);

            adminClient.CreateOrUpdateIndex(definition);
        }

        // Upload documents in a single Upload request.
        private static void UploadDocuments(SearchClient searchClient,int j)
        {
            IndexDocumentsBatch<Client> batch = new IndexDocumentsBatch<Client>();
            int initialval=25000*j;
            int finalVal = initialval + 25000;

            Console.WriteLine("Start Batch with intial Value"+ initialval +" Batch #"+j+": Adding all clients, ...\n");
            for (int i = initialval; i < finalVal; i++)
            {
                string clientId = null;
                string clientSubId = null;
                string clientSortName = null;
                string clientType = null;
                string clientOffice = null;
                string clientBuisnessUnit = null;
                string clientRegion = null;
                if (i <= 100000)
                {
                    clientId = "CLAAsia_" + i;
                    clientSubId = "CLASubId" + i;
                    clientSortName = "CLASortName" + i;
                    clientType = "Corporation";
                    clientOffice = "China";
                    clientBuisnessUnit = "ATT";
                    clientRegion = "Japan";
                }
                else if (i > 100000 &&  i <= 200000)
                {
                    clientId = "CLAWestUS_" + i;
                    clientSubId = "CLASubId" + i;
                    clientSortName = "CLASortName" + i;
                    clientType = "Corporation";
                    clientOffice = "Torrance";
                    clientBuisnessUnit = "TAA";
                    clientRegion = "WestUS";
                }
                else if (i > 200000 && i <= 300000)
                {
                    clientId = "CLAEastUS_" + i;
                    clientSubId = "CLASubId" + i;
                    clientSortName = "CLASortName" + i;
                    clientType = "Indiviual";
                    clientOffice = "New York";
                    clientBuisnessUnit = "NYC";
                    clientRegion = "EastUS";
                }
                else if (i > 300000 && i <= 400000)
                {

                    clientId = "CLACentralUS_" + i;
                    clientSubId = "CLASubId" + i;
                    clientSortName = "CLASortName" + i;
                    clientType = "Fiduciary";
                    clientOffice = "Dallas";
                    clientBuisnessUnit = "TX";
                    clientRegion = "CentralUS";
                }
                else
                {
                    clientId = "CLAIndia_" + i;
                    clientSubId = "CLASubId" + i;
                    clientSortName = "CLASortName" + i;
                    clientType = "Partnership";
                    clientOffice = "Delhi";
                    clientBuisnessUnit = "India";
                    clientRegion = "India";

                }
                batch.Actions.Add(IndexDocumentsAction.Upload(

                    new Client()
                    {
                        ClientID = clientId,
                        ClientSubID = clientSubId,
                        ClientSortName = clientSortName,
                        ClientType = clientType,
                        Office = clientOffice,
                        BuisnessUnit = clientBuisnessUnit,
                        Region = clientRegion,
                        SubDescription = "",
                        ContactNo= GetRandomTelNo()
                    }));
            }


            try
            {
                IndexDocumentsResult result = searchClient.IndexDocuments(batch);

                Console.WriteLine("Finished Batch with intial Value" + initialval + " Batch #" + j + ": Done Adding all clients, ...\n");
            }
            catch (Exception ex)
            {
                // If for some reason any documents are dropped during indexing, you can compensate by delaying and
                // retrying. This simple demo just logs the failed document keys and continues.
                Console.WriteLine("Failed to index some of the documents: {0}" +ex.Message);
            }
        }

        // Run queries, use WriteDocuments to print output
        private static void RunQueries(SearchClient srchclient)
        {
            SearchOptions options;
            SearchResults<Client> response;

            // Query 1
            Console.WriteLine("Query #1: Search on empty term '*' to return all clients, showing a subset of fields...\n");

            options = new SearchOptions()
            {
                IncludeTotalCount = true,
                Filter = "",
                OrderBy = { "" }
            };

            //options.Select.Add("ClientID");
            //options.Select.Add("ClientSubID");
            //options.Select.Add("ClientSortName");

            response = srchclient.Search<Client>("*", options);
            WriteDocuments(response);

            // Query 2.1
            Console.WriteLine("Query #2.1: Search on 'hotels', filter on 'Rating gt 4', sort by Rating in descending order...\n");

            options = new SearchOptions()
            {
                IncludeTotalCount = true,
                Filter = "search.in(Office, 'Dallas,Torrance', ',') and Region eq 'WestUS'",
                OrderBy = { "" }
            };


            //options.Select.Add("Office");
            //options.Select.Add("ClientSubID");
            //options.Select.Add("ClientSortName");

            response = srchclient.Search<Client>("*", options);
            WriteDocuments(response);


            // Query 2.3
            Console.WriteLine("Query #2.1: Search on 'hotels', filter on 'Rating gt 4', sort by Rating in descending order...\n");

            options = new SearchOptions()
            {
                IncludeTotalCount = true,
                Filter = "search.in(Office, 'Dallas,Torrance', ',') and search.in(Region, 'WestUS,EastUS', ',')",
                OrderBy = { "" }
            };


            //options.Select.Add("Office");
            //options.Select.Add("ClientSubID");
            //options.Select.Add("ClientSortName");

            response = srchclient.Search<Client>("*", options);
            WriteDocuments(response);

            // Query 2
            Console.WriteLine("Query #2: Search on 'hotels', filter on 'Rating gt 4', sort by Rating in descending order...\n");

            options = new SearchOptions()
            {
                IncludeTotalCount = true,
                Filter = "ClientType eq 'Corporation'",
                OrderBy = { "" }
            };


            options.Select.Add("ClientType");
            //options.Select.Add("ClientSubID");
            //options.Select.Add("ClientSortName");

            response = srchclient.Search<Client>("Client3", options);
            WriteDocuments(response);

            // Query 3
            Console.WriteLine("Query #3: Limit search to specific fields (pool in Tags field)...\n");

            options = new SearchOptions()
            {
                IncludeTotalCount = true,
                Filter = "",
                OrderBy = { "" },
                SearchFields = { "ClientType" }
            };



            options.Select.Add("ClientType");

            response = srchclient.Search<Client>("individual", options);
            WriteDocuments(response);



            //// Query 5
            //Console.WriteLine("Query #5: Look up a specific document...\n");

            //Response<Client> lookupResponse;
            //lookupResponse = srchclient.GetDocument<Client>("raja");

            //Console.WriteLine(lookupResponse.Value.ClientID);


            // Query 6
            options = new SearchOptions()
            {
                IncludeTotalCount = true,
                Filter = "",
                OrderBy = { "" }
            };


            Console.WriteLine("Query #6: Call Autocomplete on HotelName...\n");

            var autoresponse = srchclient.Autocomplete("To", "sg");
            WriteDocuments(autoresponse);

        }

        // Write search results to console
        private static void WriteDocuments(SearchResults<Client> searchResults)
        {
            foreach (SearchResult<Client> result in searchResults.GetResults())
            {
                Console.WriteLine(result.Document);
            }

            Console.WriteLine();
        }

        private static void WriteDocuments(AutocompleteResults autoResults)
        {
            foreach (AutocompleteItem result in autoResults.Results)
            {
                Console.WriteLine(result.Text);
            }

            Console.WriteLine();
        }
        private static string GetRandomTelNo()
        {
            StringBuilder telNo = new StringBuilder(12);
            int number;
            Random rand = new Random();
            for (int i = 0; i < 3; i++)
            {
                number = rand.Next(0, 8); // digit between 0 (incl) and 8 (excl)
                telNo = telNo.Append(number.ToString());
            }
            telNo = telNo.Append("-");
            number = rand.Next(0, 743); // number between 0 (incl) and 743 (excl)
            telNo = telNo.Append(String.Format("{0:D3}", number));
            telNo = telNo.Append("-");
            number = rand.Next(0, 10000); // number between 0 (incl) and 10000 (excl)
            telNo = telNo.Append(String.Format("{0:D4}", number));
            return telNo.ToString();
        }
    }
}
