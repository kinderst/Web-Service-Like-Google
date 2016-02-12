using HtmlAgilityPack;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace WorkerRole1
{
    class Crawler
    {
        CloudQueue adminQueue;
        CloudQueue urlQueue;
        CloudTable table;
        CloudTable datatable;
        HashSet<String> disallowedUrls;
        HashSet<String> alreadyVisitedUrls;
        DateTime compareDate;
        Regex rgx;
        int tableSize;
        int totalUrls;
        HashSet<String> errorUrls;
        int counter;

        public Crawler()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("crawlertable");
            table.CreateIfNotExists();
            datatable = tableClient.GetTableReference("datatable");
            datatable.CreateIfNotExists();

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            urlQueue = queueClient.GetQueueReference("urlqueue");
            urlQueue.CreateIfNotExists();
            adminQueue = queueClient.GetQueueReference("adminqueue");
            adminQueue.CreateIfNotExists();

            alreadyVisitedUrls = new HashSet<String>();
            disallowedUrls = new HashSet<String>();
            errorUrls = new HashSet<String>();

            tableSize = 0;
            totalUrls = 0;
            counter = 1;

            compareDate = DateTime.ParseExact("2015-04-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);

            //Regex to check for valid html document
            rgx = new Regex(@"^[a-zA-Z0-9\-]+.?(htm|html)?$");
        }

        //make everything inside if into own methods
        public void handleAdminMessage(CloudQueueMessage message)
        {
            String adminMessage = message.AsString;
            //enter loading phase
            if (adminMessage.Contains("start;"))
            {
                String[] splitMessage = adminMessage.Split(';');
                String[] splitRobotsUrl = splitMessage[1].Split(',');
                foreach (String aRobotUrl in splitRobotsUrl)
                {
                    List<String> xmls = robotLoadingPhase(aRobotUrl);
                    while (xmls.Count != 0)
                    {
                        xmls = xmlLoadingPhase(xmls);
                    }
                }
            }
            //else if it's a stop message
            else if (adminMessage.Contains("stop"))
            {
                //infinite loop until you get a start message
                while (true)
                {
                    Thread.Sleep(1000);
                    if (adminQueue.PeekMessage() != null)
                    {
                        CloudQueueMessage newMessage = adminQueue.GetMessage();
                        if (newMessage.AsString.Contains("start"))
                        {
                            if (table.CreateIfNotExists())
                            {
                                tableSize = 0;
                            }
                            adminQueue.DeleteMessage(newMessage);
                            break;
                        }
                    }
                }
            }
        }

        private List<String> robotLoadingPhase(String robotTxtUrl)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(robotTxtUrl);
            StreamReader reader = new StreamReader(stream);
            String line;
            List<String> xmls = new List<String>();
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("Sitemap: "))
                {
                    if (robotTxtUrl.Contains("bleacherreport.com"))
                    {
                        if (line.Contains("/nba.xml"))
                        {
                            xmls.Add(line.Split(' ')[1]);
                        }
                    }
                    else
                    {
                        xmls.Add(line.Split(' ')[1]);
                    }
                }
                else if (line.Contains("Disallow: "))
                {
                    if (robotTxtUrl.Contains("bleacherreport.com"))
                    {
                        disallowedUrls.Add("bleacherreport.com" + line.Split(' ')[1]);
                    }
                    else
                    {
                        disallowedUrls.Add("cnn.com" + line.Split(' ')[1]);

                    }
                }
            }
            return xmls;
        }

        //Crawls the xml page to see if there is any xml documents or html documents
        //If there are xml, it adds those to the new xml list to crawl
        //If its a html (.htm|.html) then it adds to url queue
        //Returns list of xml documents
        private List<String> xmlLoadingPhase(List<String> xmls)
        {
            List<String> newXmls = new List<String>();

            foreach (String xmlUrl in xmls)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlUrl);
                XmlNodeList loc = doc.GetElementsByTagName("loc");
                XmlNodeList lastmod = doc.GetElementsByTagName("lastmod");
                if (lastmod.Count == 0)
                {
                    lastmod = doc.GetElementsByTagName("news:publication_date");
                }
                //For all the urls
                for (int i = 0; i < loc.Count; i++)
                {
                    Boolean isCurrent = true;
                    //If there is a date, checks to see if current date, if not we assume current
                    if (lastmod[i] != null)
                    {
                        String tempDate = lastmod[i].InnerText.Substring(0, 10);
                        DateTime modDate = DateTime.ParseExact(tempDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        if (compareDate.Date > modDate.Date)
                        {
                            isCurrent = false;
                        }
                    }
                    if (isCurrent)
                    {
                        String locUrl = loc[i].InnerText;
                        if (locUrl.Contains(".xml"))
                        {
                            newXmls.Add(locUrl);
                        }
                        else
                        {
                            String[] urlSplit = locUrl.Split('/');
                            //checks to see if is .html, .htm, or empty meaning it has a .html behind it
                            if (rgx.IsMatch(urlSplit[urlSplit.Length - 1]))
                            {
                                //Add url to queue
                                urlQueue.AddMessage(new CloudQueueMessage(locUrl));
                            }
                        }
                    }
                }
            }
            return newXmls;
        }

        public void crawlingPhase(CloudQueueMessage urlMessage)
        {
            totalUrls++;
            String url = urlMessage.AsString;
            if (!alreadyVisitedUrls.Contains(url))
            {
                alreadyVisitedUrls.Add(url);
                try
                {
                    HtmlWeb hw = new HtmlWeb();
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc = hw.Load(url);
                    String date = "";
                    if (doc.DocumentNode.SelectSingleNode("//head/meta[@property='og:pubdate']") != null)
                    {
                        String stringDate = doc.DocumentNode.SelectSingleNode("//head/meta[@property='og:pubdate']").GetAttributeValue("content", "default").Substring(0, 10);
                        date = DateTime.ParseExact(stringDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd");
                    }
                    String fullTitle = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;
                    String[] titles = fullTitle.Split(' ');
                    foreach (string partTitle in titles) {
                        if (!partTitle.Equals(" ") && !partTitle.Equals("-") && !partTitle.Equals("CNN.com") && !partTitle.Equals(""))
                        {
                            CrawlerEntry entry = new CrawlerEntry(url, fullTitle, date, partTitle);
                            TableOperation insertOperation = TableOperation.Insert(entry);
                            table.Execute(insertOperation);
                            tableSize++;
                        }
                    }

                    //get urls in page
                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        string href = link.GetAttributeValue("href", string.Empty);
                        HashSet<String> links = new HashSet<String>();
                        String[] hrefSplit = href.Split('/');
                        String html = hrefSplit[hrefSplit.Length - 1];
                        //if the href is not in the disallowed urls, is not already crawled, is not a duplicate link, is a valid html page, and on cnn or bleacherreport
                        if (!disallowedUrls.Any(s => href.Contains(s)) && !alreadyVisitedUrls.Any(s => s.Equals(href)) && !links.Contains(href) && rgx.IsMatch(html) && (href.Contains("cnn.com") || href.Contains("bleacherreport.com")))
                        {
                            //store remaining into queue
                            urlQueue.AddMessage(new CloudQueueMessage(href));

                            //adds link to current link set
                            links.Add(href);
                        }
                    }
                }
                catch
                {

                }
            }

            updateTotalUrls();
            //Update last 10 urls crawled
            updateLastUrl(urlMessage.AsString);

            urlQueue.DeleteMessage(urlMessage);
        }

        private void updateTotalUrls()
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<DataEntry>("total", "urls");
            TableResult retrievedResult = datatable.Execute(retrieveOperation);
            DataEntry updateEntity = (DataEntry)retrievedResult.Result;
            updateEntity.intData = totalUrls;
            TableOperation updateOperation = TableOperation.Replace(updateEntity);
            try
            {
                datatable.Execute(updateOperation);
            }
            catch 
            {

            }
             
            //DataEntry entry = new DataEntry(totalUrls, "urls", "total");
            //TableOperation insertOperation = TableOperation.Insert(entry);
            //datatable.Execute(insertOperation);
        }

        private void updateLastUrl(string url)
        {

            TableOperation retrieveOperation = TableOperation.Retrieve<DataEntry>("url", "last");
            TableResult retrievedResult = datatable.Execute(retrieveOperation);
            DataEntry updateEntity = (DataEntry)retrievedResult.Result;
            updateEntity.stringData = url;
            TableOperation updateOperation = TableOperation.Replace(updateEntity);
            datatable.Execute(updateOperation);

            //DataEntry entry = new DataEntry(url, "last", "url");
            //TableOperation insertOperation = TableOperation.Insert(entry);
            //datatable.Execute(insertOperation);
        }
    }
}
