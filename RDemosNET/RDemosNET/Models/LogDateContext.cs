using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RDemosNET.Models;

namespace RDemosNET.Data
{
    public class LogDateContext : DbContext
    {
        public LogDateContext (DbContextOptions<LogDateContext> options)
            : base(options)
        {
        }

        public DbSet<RDemosNET.Models.DemoLog> DemoLog { get; set; }
    }
}
