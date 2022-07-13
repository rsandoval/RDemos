using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDemosNET.Models
{
    public class DemoLog
    {
        public int ID { get; set; }
        public int ApplicationID { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LogDate { get; set; }
        public string IPAddress { get; set; }
        public string LogDetails { get; set; }
    }
}
