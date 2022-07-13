using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RDemosNET.Data;
using RDemosNET.Models;

namespace RDemosNET.Pages.LogDate
{
    public class IndexModel : PageModel
    {
        private readonly RDemosNET.Data.LogDateContext _context;

        public IndexModel(RDemosNET.Data.LogDateContext context)
        {
            _context = context;
        }

        public IList<DemoLog> DemoLog { get;set; }

        public async Task OnGetAsync()
        {
            DemoLog = await _context.DemoLog.ToListAsync();
        }
    }
}
