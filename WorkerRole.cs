using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        CloudQueue adminQueue;
        CloudQueue urlQueue;
        CloudTable datatable;
        protected System.Diagnostics.PerformanceCounter ramCounter;
        protected System.Diagnostics.PerformanceCounter cpuCounter;
        Crawler crawler;

        public override void Run()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            datatable = tableClient.GetTableReference("datatable");
            datatable.CreateIfNotExists();

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            urlQueue = queueClient.GetQueueReference("urlqueue");
            urlQueue.CreateIfNotExists();
            adminQueue = queueClient.GetQueueReference("adminqueue");
            adminQueue.CreateIfNotExists();

            ramCounter = new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes");
            cpuCounter = new System.Diagnostics.PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            crawler = new Crawler();

            while (true)
            {
                Thread.Sleep(50);

                //If there is something in the admin queue
                if (adminQueue.PeekMessage() != null)
                {
                    CloudQueueMessage message = adminQueue.GetMessage();
                    crawler.handleAdminMessage(message);
                    adminQueue.DeleteMessage(message);
                }
                if (urlQueue.PeekMessage() != null)
                {
                    crawler.crawlingPhase(urlQueue.GetMessage());
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
