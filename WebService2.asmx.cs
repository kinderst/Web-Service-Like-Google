using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Diagnostics;
using WorkerRole1;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService2
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [ScriptService]
    public class WebService2 : System.Web.Services.WebService
    {

        //The trie
        public static Trie trie = new Trie();
        public static Dictionary<String, List<SearchResult>> cache = new Dictionary<string, List<SearchResult>>();

        //minimum memory value on VM
        public const int minMemory = 50; //50mbs

        //filepath to wiki titles in blob storage
        public static string filepath = "";

        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        //This method downloads the wiki titles to local storage a temporary 
        //file on the VM
        [WebMethod]
        public void downloadWiki()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("pa4");
            CloudBlockBlob blob = container.GetBlockBlobReference("title.txt");
            filepath = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(filepath, blob.DownloadText());

        }

        //This method builds the trie which will be used to assist in query
        //search by providing suggested results
        [WebMethod]
        public void buildTrie()
        {
            //performance counter to measure available memory
            PerformanceCounter cpu = new PerformanceCounter("Memory", "Available MBytes");
            trie = new Trie();
            int counter = 0;
            //reads the temporary file
            using (var fileStream = System.IO.File.OpenRead(filepath))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while (!reader.EndOfStream)
                    {
                        //checks memory every 500 iterations
                        if (counter % 500 == 0)
                        {
                            //if the amount of memory is less than defined amount, end
                            if (cpu.NextValue() < minMemory)
                            {
                                return;
                            }
                        }
                        counter++;
                        //adds to the trie
                        trie.add(reader.ReadLine().Replace("_", " "));
                    }
                }
            }

        }

        //This method searches the trie, and returns a list which can be
        //utilized by JSON
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<String> searchTrie(string search)
        {
            return trie.match(search, 10);
        }

        //This method searches the trie, and returns a list which can be
        //utilized by JSON
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<SearchResult> searchTable(string search)
        {
           if (cache.ContainsKey(search))
            {
                return cache[search];
            }
            List<CrawlerEntry> tableResults = new List<CrawlerEntry>();

            CloudTable table = tableClient.GetTableReference("crawlertable");

            foreach (string word in search.Split(' ')) 
            {
                TableQuery<CrawlerEntry> query = new TableQuery<CrawlerEntry>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, word));
                foreach (CrawlerEntry e in table.ExecuteQuery(query))
                {
                    tableResults.Add(e);
                }
            }

            var results = tableResults
                            .GroupBy(x => x.RowKey)
                            .Select(x => new Tuple<string, int, string, string>(x.Key, x.ToList().Count, x.ToList()[0].Title, x.ToList()[0].Date))
                            //.Select(x => Tuple.Create(x.Key, x.ToList().Count, x.ToList()[0].Title, x.ToList()[0].Date))
                            //.Select(x => new { x.Key, x.ToList().Count, x.ToList()[0].Title, x.ToList()[0].Date }).AsEnumerable().Select(x => Tuple.Create(x.Key, x.Count, x.Title, x.Date))
                            .OrderByDescending(x => x.Item2)
                            .ThenByDescending(x => x.Item4).ToList();

            List<SearchResult> sr = new List<SearchResult>();
            int min = 20;
            if (results.Count < min)
            {
                min = results.Count;
            }
            for (int i = 0; i < min; i++)
            {
                sr.Add(new SearchResult(results[i].Item1, results[i].Item3, results[i].Item4));
            }
            if (!cache.ContainsKey(search))
            {
                cache.Add(search, sr);
            }

                
            return sr;
        }
    }
}
