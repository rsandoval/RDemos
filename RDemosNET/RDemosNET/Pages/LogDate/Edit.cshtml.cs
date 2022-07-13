using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RDemosNET.Data;
using RDemosNET.Models;

namespace RDemosNET.Pages.LogDate
{
    public class EditModel : PageModel
    {
        private readonly RDemosNET.Data.LogDateContext _context;

        public EditModel(RDemosNET.Data.LogDateContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(DemoLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DemoLogExists(DemoLog.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool DemoLogExists(int id)
        {
            return _context.DemoLog.Any(e => e.ID == id);
        }
    }
}
