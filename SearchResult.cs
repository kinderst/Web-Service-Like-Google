using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class SearchResult
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string Key { get; set; }

        public SearchResult()
        {
            this.Key = "";
            this.Title = "";
            this.Date = "";
        }

        public SearchResult(string key, string title, string date)
        {
            this.Key = key;
            this.Title = title;
            this.Date = date;
        }
    }
}