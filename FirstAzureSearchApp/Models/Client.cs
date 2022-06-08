//using System;
//using System.Text.Json.Serialization;
//using Azure.Search.Documents.Indexes;
//using Azure.Search.Documents.Indexes.Models;

namespace FirstAzureSearchApp.Models
{
    public partial class Client
    {
        //[SimpleField(IsKey = true, IsFilterable = true)]
        public string ClientID { get; set; }

        //[SearchableField(IsSortable = true)]
        public string ClientSubID { get; set; }

        //[SearchableField(IsSortable = true)]
        public string ClientSortName { get; set; }

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        public string SubDescription { get; set; }

        //[SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string Office { get; set; }

        //[SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string BuisnessUnit { get; set; }

        //[SearchableField(IsFilterable = true, IsFacetable = true)]
        public string Region { get; set; }

        //[SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string ClientType { get; set; }
     
    }
}
