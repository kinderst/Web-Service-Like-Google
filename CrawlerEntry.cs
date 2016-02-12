using Microsoft.WindowsAzure.Storage.Table;
using Mono.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WorkerRole1
{
    class CrawlerEntry : TableEntity
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }

        public CrawlerEntry()
        {
            this.Url = "";
            this.Title = "";
            this.Date = "";
        }

        public CrawlerEntry(string url, string fullTitle, string date, string partTitle)
        {
            this.Url = url;
            this.Title = fullTitle;
            this.Date = date;
            this.PartitionKey = partTitle;

            //Set row key to url without invalid chars
            this.RowKey = System.Web.HttpUtility.UrlEncode(url);
        }

    }
}
