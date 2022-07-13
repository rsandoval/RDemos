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
    public class DetailsModel : PageModel
    {
        private readonly RDemosNET.Data.LogDateContext _context;

        public DetailsModel(RDemosNET.Data.LogDateContext context)
        {
            _context = context;
        }

        public DemoLog DemoLog { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DemoLog = await _context.DemoLog.FirstOrDefaultAsync(m => m.ID == id);

            if (DemoLog == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
