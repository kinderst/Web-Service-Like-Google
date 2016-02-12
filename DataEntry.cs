using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    class DataEntry : TableEntity
    {
        public string stringData { get; set; }
        public int intData { get; set; }

        public DataEntry()
        {
            this.stringData = "";
            this.intData = 0;
            this.PartitionKey = "data";
            this.RowKey = "empty";
        }

        public DataEntry(string stringData, string rowKey, string partitionKey)
        {
            this.stringData = stringData;
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public DataEntry(int intData, string rowKey, string partitionKey)
        {
            this.intData = intData;
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }
    }
}
