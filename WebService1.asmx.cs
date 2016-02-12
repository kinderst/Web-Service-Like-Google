using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using WorkerRole1;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        //questions:

        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string clearTable()
        {
            CloudTable table = tableClient.GetTableReference("crawlertable");
            table.DeleteIfExists();
            return "Table Cleared";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string clearUrlQueue()
        {
            CloudQueue urlQueue = queueClient.GetQueueReference("urlqueue");
            urlQueue.Clear();
            return "Url Queue Cleared";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string clearAdminQueue()
        {
            CloudQueue adminQueue = queueClient.GetQueueReference("adminqueue");
            adminQueue.Clear();
            return "Admin Queue Cleared";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string stopAllWorkers()
        {
            CloudQueue adminQueue = queueClient.GetQueueReference("adminqueue");
            CloudQueueMessage stop = new CloudQueueMessage("stop");
            adminQueue.AddMessage(stop);
            return "Workers Stopped";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string startCrawlerWithUrl(string rootUrl)
        {
            CloudQueue adminQueue = queueClient.GetQueueReference("adminqueue");
            CloudQueueMessage start = new CloudQueueMessage("start");
            if (!String.IsNullOrEmpty(rootUrl))
            {
                start = new CloudQueueMessage("start;" + rootUrl);
            }
            adminQueue.AddMessage(start);
            return "Crawler Started with urls: " + rootUrl;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string startCrawler()
        {
            CloudQueue adminQueue = queueClient.GetQueueReference("adminqueue");
            CloudQueueMessage start = new CloudQueueMessage("start");
            adminQueue.AddMessage(start);
            return "Crawler Started";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string retrieveIndex(string url)
        {
            try
            {
                var uri = new Uri(url);
                string firstDirectory = "root";
                if (!String.IsNullOrEmpty(uri.Segments[1]))
                {
                    firstDirectory = uri.Segments[1].Replace(":", "").Replace("/", "").Replace(".", "").Replace("?", "").Replace("=", "").Replace("-", "").Replace("&", "");
                }
                string urlRow = url.Replace(":", "").Replace("/", "").Replace(".", "").Replace("?", "").Replace("=", "").Replace("-", "").Replace("&", "");
                string indexTitle = getData(firstDirectory, urlRow).Title;
                if (String.IsNullOrEmpty(indexTitle))
                {
                    return "URL does not exist";
                }
                return indexTitle;
            }
            catch
            {
                return "URL does not exist";
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getCrawlerState()
        {
            CloudTable table = tableClient.GetTableReference("datatable");
            TableQuery<DataEntry> query = new TableQuery<DataEntry>()
            .Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "machine"),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "state"))
            );
            return table.ExecuteQuery(query).First().stringData;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String getRamAvail()
        {
            return getMachineData("ramavail") + "MB";
        }

        private int getMachineData(String type)
        {
            CloudTable table = tableClient.GetTableReference("datatable");
            TableQuery<DataEntry> query = new TableQuery<DataEntry>()
            .Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "machine"),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, type))
            );
            return table.ExecuteQuery(query).First().intData;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String getCpuUtil()
        {
            return getMachineData("cpuutil") + "%";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int totalUrlsCrawled()
        {
            CloudTable table = tableClient.GetTableReference("datatable");
            TableQuery<DataEntry> query = new TableQuery<DataEntry>()
            .Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "total"),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "urls"))
            );
            return table.ExecuteQuery(query).First().intData;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String lastUrl()
        {
            List<String> lastTen = new List<String>();
            CloudTable table = tableClient.GetTableReference("datatable");

            TableQuery<DataEntry> query = new TableQuery<DataEntry>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "url"));

            // Print the fields for each customer.
            foreach (DataEntry entry in table.ExecuteQuery(query))
            {
                return entry.stringData;
            }
            return "";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int getQueueSize()
        {
            return getSize("queue");
        }

        private int getSize(String storage)
        {
            CloudTable table = tableClient.GetTableReference("datatable");
            TableQuery<DataEntry> query = new TableQuery<DataEntry>()
            .Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "counts"),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, storage))
            );
            return table.ExecuteQuery(query).First().intData;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int getIndexSize()
        {
            return getSize("table");
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<String> getErrors()
        {
            List<String> list = new List<String>();
            CloudTable table = tableClient.GetTableReference("datatable");

            TableQuery<DataEntry> query = new TableQuery<DataEntry>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "errors"));

            // Print the fields for each customer.
            foreach (DataEntry entry in table.ExecuteQuery(query))
            {
                list.Add(entry.stringData);
            }
            return list;
        }

        private DataEntry getData(String dataName)
        {
            CloudTable table = tableClient.GetTableReference("datatable");
            TableQuery<DataEntry> query = new TableQuery<DataEntry>()
            .Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "data"),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, dataName))

            );
            return table.ExecuteQuery(query).First();
        }

        private CrawlerEntry getData(String partitionKey, String rowKey)
        {
            CloudTable table = tableClient.GetTableReference("crawlertable");
            TableQuery<CrawlerEntry> query = new TableQuery<CrawlerEntry>()
            .Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey))

            );
            return table.ExecuteQuery(query).First();
        }
    }
}
