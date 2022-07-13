using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RDemosNET.Data;
using RDemosNET.Models;

namespace RDemosNET.Pages.LogDate
{
    public class CreateModel : PageModel
    {
        private readonly RDemosNET.Data.LogDateContext _context;

        public CreateModel(RDemosNET.Data.LogDateContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public DemoLog DemoLog { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.DemoLog.Add(DemoLog);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
