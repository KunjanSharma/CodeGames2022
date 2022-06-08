using Azure.Search.Documents.Models;
using Microsoft.Azure.Search.Models;
using System.Collections.Generic;

namespace FirstAzureSearchApp.Models
{
    public class SearchData
    {
        // The text to search for.
        public string searchText { get; set; }

        public string totalCount { get; set; }
        // The list of results.
        public List<Client> resultList;

    }
}
