using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;

using RDemosNET.Models;

namespace RDemosNET
{
    public class RAlizeModel : PageModel
    {
        private IHostEnvironment _environment;

        public IActionResult OnGet()
        {
            CommentDescription = "";
            return Page();
        }

        public RAlizeModel(IHostEnvironment environment)
        {
            _environment = environment;
        }

        [BindProperty]
        public string CommentContents { get; set; }

        public string CommentDescription { get; set; }

        public void OnPostProcessDocument(string txtContents)
        {
            CommentCharacterizer characterizer = new CommentCharacterizer(txtContents);
            CommentContents = txtContents;
            CommentDescription = characterizer.GetDescription();
        }
        public void OnPostRandomSample()
        {
            Random randomNum = new Random(DateTime.Now.Millisecond);
            string[] samples = { "Me parece pésimo que el resultado de la prueba haya sido tan bajo.", "Me parece excelente que el resultado de la prueba haya sido tan alto.", "Un resultado así no era esperable." };
            string strComment = samples[randomNum.Next(samples.Length)];
            CommentContents = strComment;
            CommentCharacterizer characterizer = new CommentCharacterizer(strComment);
            CommentDescription = characterizer.GetDescription();
        }
    }
}